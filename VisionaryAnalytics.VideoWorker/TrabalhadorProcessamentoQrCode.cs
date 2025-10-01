using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.DTOs;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.Interfaces.Repositories;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Worker
{
    public class TrabalhadorProcessamentoQrCode : BackgroundService
    {
        private readonly ILogger<TrabalhadorProcessamentoQrCode> _logger;
        private readonly IServiceProvider _serviceProvider;
        private CancellationToken _stoppingToken;

        public TrabalhadorProcessamentoQrCode(ILogger<TrabalhadorProcessamentoQrCode> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;
            using var scope = _serviceProvider.CreateScope();
            var consumidor = scope.ServiceProvider.GetRequiredService<IConsumidorMessageBroker>();
            var filas = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMQOptions>>().Value.Queues;

            consumidor.IniciarConsumo(AoReceberMensagem, filas.ProcessarFrames, stoppingToken);
            return Task.CompletedTask;
        }

        private async Task AoReceberMensagem(string mensagem)
        {
            MensagemProcessamentoFrame? processamento = null;

            try
            {
                processamento = JsonSerializer.Deserialize<MensagemProcessamentoFrame>(mensagem);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Falha ao desserializar mensagem de processamento de frames: {Mensagem}", mensagem);
            }

            if (processamento == null)
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var servicoDecodificador = scope.ServiceProvider.GetRequiredService<IServicoDecodificadorQrCode>();
            var repositorio = scope.ServiceProvider.GetRequiredService<IVideoJobRepository>();
            var notificador = scope.ServiceProvider.GetRequiredService<IServicoNotificacaoTempoReal>();
            var opcoesArmazenamento = scope.ServiceProvider.GetRequiredService<IOptions<OpcoesArmazenamento>>().Value;
            var diretorioBase = Path.GetFullPath(opcoesArmazenamento.DiretorioBase);

            var job = await repositorio.ObterPorIdAsync(processamento.JobId);
            if (job == null)
            {
                _logger.LogWarning("Job {JobId} não encontrado durante o processamento dos frames.", processamento.JobId);
                return;
            }

            _logger.LogInformation("Iniciando processamento de {Quantidade} frames para o job {JobId}.", processamento.Frames.Count, processamento.JobId);

            var resultados = new ConcurrentBag<QrCode>();
            var opcoesParalelismo = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount),
                CancellationToken = _stoppingToken
            };

            try
            {
                await Parallel.ForEachAsync(processamento.Frames, opcoesParalelismo, async (frame, token) =>
                {
                    try
                    {
                        var qrCode = await servicoDecodificador.DecodificarAsync(frame, token);
                        if (qrCode is not null)
                        {
                            resultados.Add(qrCode);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignora cancelamentos ao encerrar o serviço
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar frame {FramePath} do job {JobId}.", frame.Caminho, processamento.JobId);
                    }
                    finally
                    {
                        RemoverArquivoDoFrame(frame, diretorioBase);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Processamento cancelado para o job {JobId}.", processamento.JobId);
                return;
            }

            var resultadosOrdenados = resultados
                .OrderBy(r => r.Instante)
                .ToList();

            job.ConcluirProcessamento(resultadosOrdenados);
            await repositorio.AtualizarAsync(job);

            //await notificador.NotificarConclusaoAsync(job.Id, job.Resultados);
            _logger.LogInformation("Job {JobId} concluído. Total de QR Codes encontrados: {Quantidade}.", job.Id, job.Resultados.Count);
        }

        private void RemoverArquivoDoFrame(Frame frame, string diretorioBase)
        {
            try
            {
                var caminhoAbsoluto = Path.GetFullPath(frame.Caminho);
                if (!caminhoAbsoluto.StartsWith(diretorioBase, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (File.Exists(caminhoAbsoluto))
                {
                    File.Delete(caminhoAbsoluto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao remover o arquivo do frame {FramePath}.", frame.Caminho);
            }
        }
    }
}
