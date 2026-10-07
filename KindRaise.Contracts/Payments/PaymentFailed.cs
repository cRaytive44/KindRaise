namespace KindRaise.Contracts.Payments
{
    public sealed record PaymentFailed(
        Guid DonationId,
        Guid CampaignId,
        decimal Amount,
        string FailureReason);
}
