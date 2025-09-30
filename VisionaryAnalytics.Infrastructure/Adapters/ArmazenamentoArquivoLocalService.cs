using VisionaryAnalytics.Application.Interfaces;

namespace VisionaryAnalytics.Infrastructure.Adapters
{
    public class ArmazenamentoArquivoLocalService : IArmazenamentoArquivoService
    {
        public async Task<string> SalvarAsync(Stream streamArquivo, string nomeOriginalArquivo, string extensao)
        {
            var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
            var pastaArquivos = Path.Combine("C:\\Uploads");
            Directory.CreateDirectory(pastaArquivos);
            var caminhoArquivo = Path.Combine(pastaArquivos, nomeArquivo);

            await using (var streamDestino = new FileStream(caminhoArquivo, FileMode.Create))
            {
                await streamArquivo.CopyToAsync(streamDestino);
            }

            return nomeArquivo;
        }
    }
}
