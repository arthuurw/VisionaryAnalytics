using Microsoft.AspNetCore.SignalR;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Api.Hubs
{
    public class HubProcessamento : Hub
    {
        public async Task NotificarJobConcluido(NotificacaoJobConcluido notificacao)
        {
            await Clients.All.SendAsync("JobConcluido", notificacao);
        }
    }

    public record NotificacaoJobConcluido(string JobId, IReadOnlyCollection<QrCode> Resultados);
}
