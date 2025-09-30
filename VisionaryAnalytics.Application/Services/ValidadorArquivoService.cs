using VisionaryAnalytics.Application.DTOs;
using VisionaryAnalytics.Application.Interfaces;

namespace VisionaryAnalytics.Application.Services
{
    public class ValidadorArquivoService : IValidadorArquivoService
    {
        public Resultado<string> Validar(string nomeArquivo, string extensao, long tamanhoAquivo)
        {
            List<string> extensoesValidas = [".mp4", ".avi", ".mkv"];
            

            if (!extensoesValidas.Contains(extensao))
            {
                return Resultado<string>.Falha($"Extensão do arquivo não suportada ({string.Join(",", extensoesValidas)}).");
            }

            if (tamanhoAquivo > 10 * 1024 * 1024)
            {
                return Resultado<string>.Falha("O tamanho do arquivo deve possuir no máximo 10mb.");
            }

            return Resultado<string>.Ok();
        }
    }
}
