using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Infrastructure.Adapters
{
    public class ServicoNotificacaoTempoRealNulo : IServicoNotificacaoTempoReal
    {
        public Task NotificarConclusaoAsync(string jobId, IReadOnlyCollection<QrCode> resultados, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
