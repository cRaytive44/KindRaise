namespace KindRaise.Contracts.Donations
{
    public sealed record DonationConfirmed(
        Guid DonationId,
        Guid CampaignId,
        decimal Amount);
}
