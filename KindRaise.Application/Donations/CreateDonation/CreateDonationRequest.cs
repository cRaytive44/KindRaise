namespace KindRaise.Application.Donations.CreateDonation
{
    public sealed class CreateDonationRequest
    {
        public Guid CampaignId { get; init; }
        public string DonorName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
