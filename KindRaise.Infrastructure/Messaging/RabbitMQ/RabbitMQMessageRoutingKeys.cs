namespace KindRaise.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMQMessageRoutingKeys
    {
        public const string DonationRequested = "donation.requested";

        public const string PaymentConfirmed = "payment.confirmed";
        public const string PaymentFailed = "payment.failed";

        public const string DonationConfirmed = "donation.confirmed";
        public const string DonationFailed = "donation.failed";
    }
}
