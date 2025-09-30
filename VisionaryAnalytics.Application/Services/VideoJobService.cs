using Microsoft.Extensions.Options;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.DTOs;
using VisionaryAnalytics.Application.Extensions;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.Entities;
using VisionaryAnalytics.Domain.Interfaces.Repositories;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Application.Services
{
    public class VideoJobService : IVideoJobService
    {
        private readonly IValidadorArquivoService _validadorArquivoService;
        private readonly IArmazenamentoArquivoService _armazenamentoArquivoService;
        private readonly IVideoJobRepository _videoJobRepository;
        private readonly IProdutorMessageBroker _produtor;
        private readonly RabbitMQQueues _rabbitQueues;

        public VideoJobService(
            IValidadorArquivoService validadorArquivoService, 
            IArmazenamentoArquivoService armazenamentoArquivoService,
            IVideoJobRepository repository,
            IProdutorMessageBroker produtor,
            IOptions<RabbitMQOptions> rabbitOptions)
        {
            _validadorArquivoService = validadorArquivoService;
            _armazenamentoArquivoService = armazenamentoArquivoService;
            _videoJobRepository = repository;
            _produtor = produtor;
            _rabbitQueues = rabbitOptions.Value.Queues;
        }

        public async Task<Resultado<string>> CriarJobAsync(Stream stream, string nomeArquivo, string extensao, string tipoConteudo, long tamanhoArquivo)
        {
            var resultadoValidacao = _validadorArquivoService.Validar(nomeArquivo, extensao, tamanhoArquivo);
            if (!resultadoValidacao.Sucesso) return resultadoValidacao;

            // Armazenar arquivo
            var novoNome = await _armazenamentoArquivoService.SalvarAsync(stream, nomeArquivo, extensao);

            // Gravar no banco
            var job = VideoJob.Criar(nomeArquivo, novoNome);
            await _videoJobRepository.CriarAsync(job);

            await _produtor.ProduzirAsync<VideoJob>(job, _rabbitQueues.ExtrairFrames);

            return Resultado<string>.Ok(null, job.Id);
        }

        public async Task<Resultado<List<QrCode>>> ObterResultadosAsync(string id)
        {
            var job = await _videoJobRepository.ObterPorIdAsync(id);

            return job == null
                ? Resultado<List<QrCode>>.Falha($"Processamento para o ID {id} não encontrado")
                : Resultado<List<QrCode>>.Ok(null, job.Resultados);
        }

        public async Task<Resultado<string>> ObterStatusAsync(string id)
        {
            var job = await _videoJobRepository.ObterPorIdAsync(id);

            return job == null
                ? Resultado<string>.Falha($"Processamento para o ID {id} não encontrado")
                : Resultado<string>.Ok(null, job.Status.ObterNomeExibicao());
        }
    }
}
