using FFMpegCore;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Infrastructure.Adapters
{
    public class FfmpegVideoFrameService : IVideoFrameService
    {
        public async Task<List<Frame>> ExtrairFramesAsync(string id, string caminhoVideo, string diretorioSaida)
        {
            Directory.CreateDirectory(diretorioSaida);

            var info = await FFProbe.AnalyseAsync(caminhoVideo);
            var duracaoEmSegundos = (int)Math.Floor(info.Duration.TotalSeconds);
            var frames = new List<Frame>();

            for (var i = 0; i < duracaoEmSegundos; i++)
            {
                var timestamp = TimeSpan.FromSeconds(i);
                var nomeArquivo = $"{id}_frame_{i:D4}.jpg";
                var caminhoSaida = Path.Combine(diretorioSaida, nomeArquivo);

                await FFMpeg.SnapshotAsync(caminhoVideo, caminhoSaida, null, timestamp);
                frames.Add(new Frame(caminhoSaida, timestamp));
            }

            File.Delete(caminhoVideo);

            return frames;
        }
    }
}
