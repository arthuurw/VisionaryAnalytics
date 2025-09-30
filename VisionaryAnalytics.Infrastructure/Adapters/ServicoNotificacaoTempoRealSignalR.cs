using System.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VisionaryAnalytics.Application.Configuration;
using VisionaryAnalytics.Application.Interfaces;
using VisionaryAnalytics.Domain.VOs;

namespace VisionaryAnalytics.Infrastructure.Adapters
{
    public class ServicoNotificacaoTempoRealSignalR : IServicoNotificacaoTempoReal, IAsyncDisposable
    {
        private readonly ILogger<ServicoNotificacaoTempoRealSignalR> _logger;
        private readonly HubConnection? _hubConnection;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);

        public ServicoNotificacaoTempoRealSignalR(IOptions<OpcoesSignalR> options, ILogger<ServicoNotificacaoTempoRealSignalR> logger)
        {
            _logger = logger;
            var hubUrl = options.Value.UrlHub;

            if (!string.IsNullOrWhiteSpace(hubUrl))
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect()
                    .Build();
            }
        }

        public async Task NotificarConclusaoAsync(string jobId, IReadOnlyCollection<QrCode> resultados, CancellationToken cancellationToken = default)
        {
            if (_hubConnection is null)
            {
                _logger.LogDebug("Hub SignalR não configurado. Notificação ignorada para o job {JobId}.", jobId);
                return;
            }

            await GarantirConexaoAsync(cancellationToken);
            await _hubConnection.InvokeAsync("NotificarJobConcluido", new MensagemJobConcluido(jobId, resultados), cancellationToken);
        }

        private async Task GarantirConexaoAsync(CancellationToken cancellationToken)
        {
            if (_hubConnection is null)
            {
                return;
            }

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                return;
            }

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (_hubConnection.State != HubConnectionState.Connected)
                {
                    await _hubConnection.StartAsync(cancellationToken);
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }

            _connectionLock.Dispose();
        }

        private record MensagemJobConcluido(string JobId, IReadOnlyCollection<QrCode> Resultados);
    }
}
