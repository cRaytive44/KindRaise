using KindRaise.Application.Messaging;
using KindRaise.Contracts.Donations;
using RabbitMQ.Client;
using System.Text;

namespace KindRaise.Infrastructure.Messaging.RabbitMQ
{
    public sealed class OutboxRabbitMQPublisher(
        RabbitMQConnection rabbitMQConnection)
        : IOutboxMessagePublisher
    {
        public async Task PublishAsync(
            Guid messageId,
            string type,
            string content,
            CancellationToken cancellationToken)
        {
            if (type != typeof(DonationRequested).FullName)
            {
                throw new NotSupportedException($"Unsupported outbox message type: '{type}'.");
            }

            var connection = await rabbitMQConnection.GetConnectionAsync(cancellationToken);

            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);

            await using var channel = await connection.CreateChannelAsync(
                options: channelOptions,
                cancellationToken: cancellationToken);

            await RabbitMQTopology.ConfigureAsync(
                channel,
                cancellationToken);

            var body = Encoding.UTF8.GetBytes(content);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                Type = type,
                MessageId = messageId.ToString()
            };

            await channel.BasicPublishAsync(
                exchange: RabbitMQTopology.DonationsExchange,
                routingKey: RabbitMQMessageRoutingKeys.DonationRequested,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
