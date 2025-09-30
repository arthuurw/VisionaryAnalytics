using System.Text.Json.Serialization;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Application.DTOs
{
    public class MensagemProcessamentoFrame
    {
        [JsonConstructor]
        public MensagemProcessamentoFrame(string jobId, List<Frame> frames)
        {
            JobId = jobId;
            Frames = frames;
        }

        public string JobId { get; }
        public List<Frame> Frames { get; }
    }
}
