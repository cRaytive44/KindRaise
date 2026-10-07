namespace KindRaise.Contracts.Donations
{
    public sealed record DonationFailed(
        Guid DonationId,
        Guid CampaignId,
        decimal Amount,
        string FailureReason);
}
