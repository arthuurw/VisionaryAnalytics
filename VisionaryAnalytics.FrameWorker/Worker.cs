using System.Text.Json;
using Microsoft.Extensions.Options;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.Entities;

namespace VisionaryAnalytics.FrameWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var consumidor = scope.ServiceProvider.GetRequiredService<IConsumidorMessageBroker>();
            var queues = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMQOptions>>().Value.Queues;

            consumidor.IniciarConsumo(OnMessageReceived, queues.ExtrairFrames, stoppingToken);
            return Task.CompletedTask;        
        }

        private async Task OnMessageReceived(string mensagem)
        {
            using var scope = _serviceProvider.CreateScope();
            var videoFrameService = scope.ServiceProvider.GetRequiredService<IVideoFrameService>();
            var produtor = scope.ServiceProvider.GetRequiredService<IProdutorMessageBroker>();
            var queues = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMQOptions>>().Value.Queues;
            var cts = new CancellationTokenSource();
            var caminhoArquivos = "C:\\Uploads";

            var video = JsonSerializer.Deserialize<VideoJob>(mensagem);
            if (video == null)
            {
                _logger.LogInformation("Erro ao desserializar mensagem");
                return;
            }

            var frames = await videoFrameService.ExtrairFramesAsync(video.Id, Path.Combine(caminhoArquivos, video.NomeArmazenadoArquivo), caminhoArquivos);

            await Parallel.ForEachAsync(frames, cts.Token, async (frame, token) =>
            {
                await produtor.ProduzirAsync(frame, queues.ProcessarFrames, cts.Token);
            });
        }

    }
}
