namespace KindRaise.Domain.Donation
{
    public class Donation
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid CampaignId { get; }
        public string DonorName{ get; }
        public decimal Amount { get; }
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
        public ProcessingState ProcessingState { get; private set; } = ProcessingState.Pending;

        public Donation(Guid campaignId, string donorName, decimal amount)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(donorName);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

            CampaignId = campaignId;
            DonorName = donorName;
            Amount = amount;
        }

        public Donation()
        {
            CampaignId = Guid.Empty;
            DonorName = string.Empty;
            Amount = 1;
        }
    } 
}
