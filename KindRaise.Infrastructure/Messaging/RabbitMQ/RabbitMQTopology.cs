using RabbitMQ.Client;

namespace KindRaise.Infrastructure.Messaging.RabbitMQ
{
    public static class RabbitMQTopology
    {
        // Exchange and queue names
        public const string DonationsExchange = "kindraise.donations";
        public const string DonationRequestsQueue = "kindraise.donation.requests";

        // Retry exchange and queue names
        public const string RetryExchange = "kindraise.retry";
        public const string RetryQueue = "kindraise.donations-requests.retry";

        // Dead-letter exchange and queue names
        public const string DeadLetterExchange ="kindraise.donations.dlx";
        public const string DeadLetterQueue = "kindraise.donation-requests.dlq";
        public const string DeadLetterRoutingKey = "donation.dead";

        // Events exchange name
        public const string EventsExchange = "kindraise.events";

        // Retry delay in milliseconds
        public const int RetryDelayMilliseconds = 5000;


        public static async Task ConfigureAsync(
            IChannel channel,
            RabbitMQOptions options,
            CancellationToken cancellationToken)
        {
            // Exchanges
            await channel.ExchangeDeclareAsync(
                exchange: DonationsExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: RetryExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: DeadLetterExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: EventsExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            // Main queue
            var mainQueueArguments =
                new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = DeadLetterExchange,
                    ["x-dead-letter-routing-key"] = DeadLetterRoutingKey
                };

            await channel.QueueDeclareAsync(
                queue: DonationRequestsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: mainQueueArguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: DonationRequestsQueue,
                exchange: DonationsExchange,
                routingKey: RabbitMQMessageRoutingKeys.DonationRequested,
                cancellationToken: cancellationToken);

            // Retry queue
            var retryQueueArguments =
                new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = RetryDelayMilliseconds,
                    ["x-dead-letter-exchange"] = DonationsExchange,
                    ["x-dead-letter-routing-key"] =
                        RabbitMQMessageRoutingKeys.DonationRequested
                };

            await channel.QueueDeclareAsync(
                queue: RetryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryQueueArguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: RetryQueue,
                exchange: RetryExchange,
                routingKey: RabbitMQMessageRoutingKeys.DonationRequested,
                cancellationToken: cancellationToken);

            // Dead Letter Queue
            await channel.QueueDeclareAsync(
                queue: DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: DeadLetterQueue,
                exchange: DeadLetterExchange,
                routingKey: DeadLetterRoutingKey,
                cancellationToken: cancellationToken);
        }
    }
}
