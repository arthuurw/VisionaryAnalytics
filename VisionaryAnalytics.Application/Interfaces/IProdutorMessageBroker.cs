namespace VisionaryAnalytics.Application.Interfaces
{
    public interface IProdutorMessageBroker
    {
        Task ProduzirAsync<T>(T mensagem, string fila, CancellationToken cancellationToken = default);
    }
}
