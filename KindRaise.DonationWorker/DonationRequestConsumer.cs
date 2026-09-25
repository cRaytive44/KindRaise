using KindRaise.Contracts.Donations;
using KindRaise.Infrastructure.Messaging.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace KindRaise.DonationWorker
{
    public sealed class DonationRequestConsumer(ILogger<DonationRequestConsumer> logger)
    {
        public async Task StartAsync(IChannel channel, CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                await HandleMessageAsync(channel, eventArgs, cancellationToken);
            };

            await channel.BasicConsumeAsync(
                queue: RabbitMQTopology.DonationRequestsQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Started consuming messages from queue {QueueName}",
                RabbitMQTopology.DonationRequestsQueue);

            try 
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            
            catch (OperationCanceledException) 
                when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Donation request consumer is stopping.");
            }
        }

        private async Task HandleMessageAsync(
            IChannel channel,
            BasicDeliverEventArgs eventArgs,
            CancellationToken cancellationToken) 
        {
            var deliveryTag = eventArgs.DeliveryTag;

            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                var donationRequested = JsonSerializer.Deserialize<DonationRequested>(json);
            
                if(donationRequested is null) 
                {
                    throw new JsonException("Message could not be deserialized.");
                }

                var retryCount = GetRetryCount(eventArgs.BasicProperties);
                var attemptNumber = retryCount + 1;

                logger.LogInformation(
                   "Received DonationRequested message. " +
                   "DeliveryTag: {DeliveryTag}, " +
                   "DonationId: {DonationId}, " +
                   "CampaignId: {CampaignId}, " +
                   "Amount: {Amount}" +
                   "Attempt: {AttemptNumber}",
                   deliveryTag,
                   donationRequested.DonationId,
                   donationRequested.CampaignId,
                   donationRequested.Amount,
                   attemptNumber);

                await ProcessDonationAsync(donationRequested, cancellationToken);

                await channel.BasicAckAsync(
                    deliveryTag: deliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken);

                logger.LogInformation(
                    "Message acknowledged. " +
                    "DonationId: {DonationId}, " +
                    "Attempt: {AttemptNumber}",
                    donationRequested.DonationId,
                    attemptNumber);
            }

            catch (JsonException exception)
            {
                logger.LogError(
                    exception,
                    "Error processing donation request. " +
                    "DeliveryTag: {DeliveryTag}",
                    deliveryTag);

                await channel.BasicNackAsync(
                    deliveryTag: deliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: cancellationToken);
            }

            catch (Exception exception)
            {
                await HandleProcessingFailureAsync(
                    channel,
                    eventArgs,
                    exception,
                    cancellationToken);
            }
        }

        private async Task HandleProcessingFailureAsync(
            IChannel channel,
            BasicDeliverEventArgs eventArgs,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var retryCount = GetRetryCount(eventArgs.BasicProperties);
            var attemptNumber = retryCount + 1;

            if (attemptNumber < RetryPolicy.MaxAttempts)
            {
                logger.LogWarning(
                    exception,
                    "Donation processing failed. " +
                    "Scheduling retry. " +
                    "Attempt: {AttemptNumber}, " +
                    "MaxAttempts: {MaxAttempts}",
                    attemptNumber,
                    RetryPolicy.MaxAttempts);

                await PublishForRetryAsync(
                    channel,
                    eventArgs,
                    retryCount + 1,
                    cancellationToken);

                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken);

                return;
            }

            logger.LogError(
                exception,
                "Donation processing failed permanently. " +
                "Maximum attempts reached: {MaxAttempts}",
                RetryPolicy.MaxAttempts);

            await channel.BasicNackAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken: cancellationToken);
        }

        private async Task PublishForRetryAsync(
            IChannel channel,
            BasicDeliverEventArgs eventArgs,
            int nextRetryCount,
            CancellationToken cancellationToken)
        {
            var properties = new BasicProperties
            {
                ContentType = eventArgs.BasicProperties.ContentType
                    ?? "application/json",

                DeliveryMode = DeliveryModes.Persistent,

                Headers = new Dictionary<string, object?>
                {
                    [RetryPolicy.RetryCountHeader] = nextRetryCount
                }
            };

            await channel.BasicPublishAsync(
                exchange: RabbitMQTopology.RetryExchange,
                routingKey: RabbitMQTopology.DonationRequestedRoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: eventArgs.Body,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Message published to retry exchange. " +
                "RetryCount: {RetryCount}",
                nextRetryCount);
        }

        private static int GetRetryCount(IReadOnlyBasicProperties properties)
        {
            if (properties.Headers is null)
            {
                return 0;
            }

            if (!properties.Headers.TryGetValue(
                    RetryPolicy.RetryCountHeader,
                    out var value))
            {
                return 0;
            }

            return value switch
            {
                int count => count,
                long count => checked((int)count),

                byte[] bytes when
                    int.TryParse(
                        Encoding.UTF8.GetString(bytes),
                        out var count)
                    => count,

                _ => 0
            };
        }

        private Task ProcessDonationAsync(
            DonationRequested message,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing donation {DonationId}.", message.DonationId);

            // Substituir pelo processamento real.
            return Task.CompletedTask;
        }
    }
}
