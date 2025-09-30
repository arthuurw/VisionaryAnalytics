using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Application.Interfaces
{
    public interface IServicoNotificacaoTempoReal
    {
        Task NotificarConclusaoAsync(string jobId, IReadOnlyCollection<QrCode> resultados, CancellationToken cancellationToken = default);
    }
}
