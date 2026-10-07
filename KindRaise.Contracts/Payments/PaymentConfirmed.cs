namespace KindRaise.Contracts.Payments
{
    public sealed record PaymentConfirmed(
        Guid DonationId,
        Guid CampaignId,
        decimal Amount);
}
