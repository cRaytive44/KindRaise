using KindRaise.Application.Messaging;
using RabbitMQ.Client;
using System.Text.Json;

namespace KindRaise.Infrastructure.Messaging.RabbitMQ
{
    public sealed class RabbitMQMessagePublisher(
        RabbitMQConnection rabbitMQConnection,
        RabbitMQOptions options) : IMessagePublisher
    {
        public async Task PublishAsync<T>(
            T message,
            string routingKey,
            CancellationToken cancellationToken)
        {
            var connection = await rabbitMQConnection.GetConnectionAsync(cancellationToken);

            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: options.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
