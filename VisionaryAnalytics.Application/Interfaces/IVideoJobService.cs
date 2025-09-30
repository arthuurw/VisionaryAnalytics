using VisionaryAnalytics.Application.DTOs;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Application.Interfaces
{
    public interface IVideoJobService
    {
        Task<Resultado<string>> CriarJobAsync(Stream stream, string nomeArquivo, string extensao, string contentType, long tamanhoArquivo);
        Task<Resultado<string>> ObterStatusAsync(string id);
        Task<Resultado<List<QrCode>>> ObterResultadosAsync(string id);
    }
}
