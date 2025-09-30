using Microsoft.Extensions.Options;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.Interfaces;

namespace VisionaryAnalytics.Infrastructure.Adapters
{
    public class ArmazenamentoArquivoLocalService : IArmazenamentoArquivoService
    {
        private readonly string _diretorioBase;

        public ArmazenamentoArquivoLocalService(IOptions<OpcoesArmazenamento> storageOptions)
        {
            _diretorioBase = storageOptions.Value.DiretorioBase;
        }

        public async Task<string> SalvarAsync(Stream streamArquivo, string nomeOriginalArquivo, string extensao)
        {
            var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
            var pastaArquivos = Path.GetFullPath(_diretorioBase);
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
