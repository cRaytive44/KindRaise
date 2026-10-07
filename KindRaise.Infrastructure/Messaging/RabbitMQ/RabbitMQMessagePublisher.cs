using KindRaise.Application.Messaging;
using RabbitMQ.Client;
using System.Text.Json;

namespace KindRaise.Infrastructure.Messaging.RabbitMQ
{
    public sealed class RabbitMQMessagePublisher(
        RabbitMQConnection rabbitMQConnection) 
        : IMessagePublisher
    {
        public async Task PublishAsync<T>(
            T message,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(message);

            var routing = RabbitMQMessageRouting.GetRouting<T>();

            var connection = await rabbitMQConnection.GetConnectionAsync(cancellationToken);

            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                Type = typeof(T).FullName
            };

            await channel.BasicPublishAsync(
                exchange: routing.Exchange,
                routingKey: routing.RoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
