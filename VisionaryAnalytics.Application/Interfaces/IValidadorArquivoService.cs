using VisionaryAnalytics.Application.DTOs;

namespace VisionaryAnalytics.Application.Interfaces
{
    public interface IValidadorArquivoService
    {
        Resultado<string> Validar(string nomeArquivo, string extensao, long tamanhoAquivo);
    }
}
