using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Application.Interfaces
{
    public interface IVideoFrameService
    {
        Task<List<Frame>> ExtrairFramesAsync(string id, string caminhoVideo, string diretorioSaida);
    }
}
