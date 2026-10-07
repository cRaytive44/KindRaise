using KindRaise.Contracts.Donations;
using KindRaise.Contracts.Payments;

namespace KindRaise.Infrastructure.Messaging.RabbitMQ
{
    public static class RabbitMQMessageRouting
    {
        public static (string Exchange, string RoutingKey) GetRouting<T>()
        {
            return typeof(T).Name switch
            {
                nameof(DonationRequested) => 
                (
                    RabbitMQTopology.DonationsExchange,
                    RabbitMQMessageRoutingKeys.DonationRequested
                ),

                nameof(DonationConfirmed) =>
                (
                    RabbitMQTopology.EventsExchange,
                    RabbitMQMessageRoutingKeys.DonationConfirmed
                ),

                nameof(DonationFailed) =>
                (
                    RabbitMQTopology.EventsExchange,
                    RabbitMQMessageRoutingKeys.DonationFailed
                ),

                nameof(PaymentConfirmed) => 
                (
                    RabbitMQTopology.EventsExchange,
                    RabbitMQMessageRoutingKeys.PaymentConfirmed
                ),

                nameof(PaymentFailed) => 
                (
                    RabbitMQTopology.EventsExchange,
                    RabbitMQMessageRoutingKeys.PaymentFailed
                ),

                _ => throw new InvalidOperationException(
                    $"No RabbitMQ routing configuration exists for message type '{typeof(T).Name}'.")
            };
        }
    }
}
