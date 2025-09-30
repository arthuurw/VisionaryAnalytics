using System.Text.Json.Serialization;
using VisionaryAnalytics.Domain.Enums;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Domain.Entities
{
    public class VideoJob
    {
        public string Id { get; private set; } = string.Empty;
        public string NomeOriginalArquivo { get; private set; } = string.Empty;
        public string NomeArmazenadoArquivo { get; private set; } = string.Empty;
        public Status Status { get; private set; }
        public List<QrCode> Resultados { get; private set; } = [];
        public DateTime CriadoEm { get; private set; }
        public DateTime? ConcluidoEm { get; private set; }

        [JsonConstructor]
        public VideoJob(
        string id,
        string nomeOriginalArquivo,
        string nomeArmazenadoArquivo,
        Status status,
        List<QrCode> resultados,
        DateTime criadoEm,
        DateTime? concluidoEm)
        {
            Id = id;
            NomeOriginalArquivo = nomeOriginalArquivo;
            NomeArmazenadoArquivo = nomeArmazenadoArquivo;
            Status = status;
            Resultados = resultados;
            CriadoEm = criadoEm;
            ConcluidoEm = concluidoEm;
        }

        public VideoJob() { }

        public static VideoJob Criar(string nomeOriginalArquivo, string nomeArmazenadoArquivo)
        {
            return new VideoJob()
            {
                Id = Guid.NewGuid().ToString(),
                NomeOriginalArquivo = nomeOriginalArquivo,
                NomeArmazenadoArquivo = nomeArmazenadoArquivo,
                Status = Status.NaFila,
                CriadoEm = DateTime.UtcNow
            };
        }

        public void IniciarProcessamento() => Status = Status.Processando;

        public void ConcluirProcessamento(List<QrCode> resultados)
        {
            Status = Status.Concluido;
            Resultados = resultados;
            ConcluidoEm = DateTime.UtcNow;
        }
    }
}
