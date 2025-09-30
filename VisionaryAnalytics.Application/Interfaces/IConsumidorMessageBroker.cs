namespace VisionaryAnalytics.Application.Interfaces
{
    public interface IConsumidorMessageBroker
    {
        Task IniciarConsumo(Func<string, Task> onMessageReceived, string fila, CancellationToken cancellationToken = default);
    }
}
