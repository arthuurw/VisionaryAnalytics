using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VisionaryAnalytics.Application.Interfaces;

namespace VisionaryAnalytics.Infrastructure.Adapters
{
    public class RabbitMqConsumer : IConsumidorMessageBroker, IAsyncDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqConsumer(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task IniciarConsumo(Func<string, Task> onMessageReceived, string fila, CancellationToken cancellationToken = default)
        {
            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                    queue: fila,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: cancellationToken);
            await _channel.BasicQosAsync(0, prefetchCount: 10, global: false, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    await onMessageReceived(message);
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao consumir mensagem: {ex.Message}");
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(queue: fila, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null) await _channel.DisposeAsync();
            if (_connection is not null) await _connection.DisposeAsync();
        }
    }
}
