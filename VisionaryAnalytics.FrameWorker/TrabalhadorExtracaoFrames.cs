using System.Text.Json;
using Microsoft.Extensions.Options;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.DTOs;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.Entities;
using VisionaryAnalytics.Domain.Interfaces.Repositories;

namespace VisionaryAnalytics.FrameWorker
{
    public class TrabalhadorExtracaoFrames : BackgroundService
    {
        private readonly ILogger<TrabalhadorExtracaoFrames> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TrabalhadorExtracaoFrames(ILogger<TrabalhadorExtracaoFrames> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var consumidor = scope.ServiceProvider.GetRequiredService<IConsumidorMessageBroker>();
            var filas = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMQOptions>>().Value.Queues;

            consumidor.IniciarConsumo(AoReceberMensagem, filas.ExtrairFrames, stoppingToken);
            return Task.CompletedTask;
        }

        private async Task AoReceberMensagem(string mensagem)
        {
            using var scope = _serviceProvider.CreateScope();
            var servicoFramesVideo = scope.ServiceProvider.GetRequiredService<IVideoFrameService>();
            var produtor = scope.ServiceProvider.GetRequiredService<IProdutorMessageBroker>();
            var repositorio = scope.ServiceProvider.GetRequiredService<IVideoJobRepository>();
            var filasRabbit = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMQOptions>>().Value.Queues;
            var opcoesArmazenamento = scope.ServiceProvider.GetRequiredService<IOptions<OpcoesArmazenamento>>().Value;
            var diretorioBase = Path.GetFullPath(opcoesArmazenamento.DiretorioBase);

            var video = JsonSerializer.Deserialize<VideoJob>(mensagem);
            if (video == null)
            {
                _logger.LogWarning("Mensagem inválida recebida para extração de frames: {Mensagem}", mensagem);
                return;
            }

            var job = await repositorio.ObterPorIdAsync(video.Id);
            if (job == null)
            {
                _logger.LogWarning("Job {JobId} não encontrado no banco durante a extração de frames.", video.Id);
                return;
            }

            job.IniciarProcessamento();
            await repositorio.AtualizarAsync(job);

            var caminhoVideo = Path.Combine(diretorioBase, video.NomeArmazenadoArquivo);
            if (!File.Exists(caminhoVideo))
            {
                _logger.LogError("Arquivo de vídeo {VideoPath} não encontrado para o job {JobId}.", caminhoVideo, video.Id);
                return;
            }

            _logger.LogInformation("Iniciando extração de frames para o job {JobId}.", video.Id);
            var frames = await servicoFramesVideo.ExtrairFramesAsync(video.Id, caminhoVideo, diretorioBase);
            _logger.LogInformation("Extração de frames concluída para o job {JobId}. Total de frames: {Quantidade}.", video.Id, frames.Count);

            var mensagemProcessamento = new MensagemProcessamentoFrame(video.Id, frames);
            await produtor.ProduzirAsync(mensagemProcessamento, filasRabbit.ProcessarFrames);
        }
    }
}
