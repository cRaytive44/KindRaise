namespace KindRaise.Contracts.Donations
{
    public sealed record DonationRequested(
        Guid DonationId,
        Guid CampaignId,
        decimal Amount);
}
