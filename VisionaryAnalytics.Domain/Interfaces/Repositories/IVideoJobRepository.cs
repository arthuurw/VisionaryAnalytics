using VisionaryAnalytics.Domain.Entities;

namespace VisionaryAnalytics.Domain.Interfaces.Repositories
{
    public interface IVideoJobRepository
    {
        Task CriarAsync(VideoJob job);
        Task<VideoJob?> ObterPorIdAsync(string id);
        Task AtualizarAsync(VideoJob job);
    }
}
