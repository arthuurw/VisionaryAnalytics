using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionaryAnalytics.Application.Interfaces
{
    public interface IArmazenamentoArquivoService
    {
        Task<string> SalvarAsync(Stream streamArquivo, string nomeOriginalArquivo, string extensao);
    }
}
