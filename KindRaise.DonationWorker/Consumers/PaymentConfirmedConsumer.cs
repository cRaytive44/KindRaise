using KindRaise.Contracts.Payments;
using KindRaise.DonationWorker.PaymentProcessor;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace KindRaise.DonationWorker.Consumers
{
    public sealed class PaymentConfirmedConsumer(
        ILogger<PaymentConfirmedConsumer> logger,
        PaymentConfirmedProcessor paymentConfirmedProcessor)
    {

        public async Task StartAsync(IChannel channel, CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                await HandleMessageAsync(
                    channel,
                    eventArgs,
                    cancellationToken);
            };

            await channel.BasicConsumeAsync(
                queue: RabbitMQTopology.PaymentConfirmedQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Started consuming messages from queue '{QueueName}'",
                RabbitMQTopology.PaymentConfirmedQueue);
        }

        private async Task HandleMessageAsync(
            IChannel channel,
            BasicDeliverEventArgs eventArgs,
            CancellationToken cancellationToken)
        {
            var deliveryTag = eventArgs.DeliveryTag;
            var messageId = eventArgs.BasicProperties.MessageId;

            if (string.IsNullOrWhiteSpace(messageId))
            {
                logger.LogWarning(
                    "PaymentConfirmed has no MessageId. DeliveryTag: {DeliveryTag}",
                    deliveryTag);

                await channel.BasicNackAsync(
                    deliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken);

                return;
            }

            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                var message =
                    JsonSerializer.Deserialize<PaymentConfirmed>(json)
                    ?? throw new JsonException(
                        "Failed to deserialize PaymentConfirmed message.");

                await paymentConfirmedProcessor.ProcessAsync(
                    message,
                    messageId,
                    cancellationToken);

                await channel.BasicAckAsync(
                    deliveryTag,
                    multiple: false,
                    cancellationToken);

            }

            catch (JsonException exception)
            {
                logger.LogError(
                    exception,
                    "Invalid PaymentConfirmed message. MessageId: {MessageId}",
                    messageId);

                await channel.BasicNackAsync(
                    deliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken);
            }

            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to process PaymentConfirmed. MessageId: {MessageId}",
                    messageId);

                await channel.BasicNackAsync(
                    deliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken);
            }
        }
    }
}
