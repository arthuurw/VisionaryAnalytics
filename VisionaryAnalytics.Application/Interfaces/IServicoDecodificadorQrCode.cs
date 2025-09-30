using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Application.Interfaces
{
    public interface IServicoDecodificadorQrCode
    {
        Task<QrCode?> DecodificarAsync(Frame frame, CancellationToken cancellationToken = default);
    }
}
