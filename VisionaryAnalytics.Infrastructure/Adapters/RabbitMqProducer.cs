using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using VisionaryAnalytics.Application.Interfaces;

namespace VisionaryAnalytics.Infrastructure.Adapters
{
    public class RabbitMqProducer : IProdutorMessageBroker, IAsyncDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqProducer(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task ProduzirAsync<T>(T mensagem, string fila, CancellationToken cancellationToken = default)
        {
            _connection ??= await _connectionFactory.CreateConnectionAsync(cancellationToken);
            _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            
            await _channel.QueueDeclareAsync(
                    queue: fila,
                    durable: true,
                    exclusive: false,
                    autoDelete: false, 
                    cancellationToken: cancellationToken);

            var jsonMessage = JsonSerializer.Serialize(mensagem);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: fila,
                mandatory: true,
                basicProperties: new BasicProperties { Persistent = true },
                body: body,
                cancellationToken: cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null) await _channel.DisposeAsync();
            if (_connection is not null) await _connection.DisposeAsync();
        }
    }
}
