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

        private Donation()
        {
            CampaignId = Guid.Empty;
            DonorName = string.Empty;
            Amount = 1;
        }

        public void StartProcessing()
        {
            if (ProcessingState != ProcessingState.Pending)
            {
                throw new InvalidOperationException("Donation must be in Pending state to start processing.");
            }
            ProcessingState = ProcessingState.Processing;
        }

        public void MarkAsProcessed()
        {
            if (ProcessingState != ProcessingState.Processing)
            {
                throw new InvalidOperationException("Donation must be in Processing state to mark as processed.");
            }
            ProcessingState = ProcessingState.Processed;
        }

        public void MarkAsRejected()
        {
            if (ProcessingState != ProcessingState.Processing)
            {
                throw new InvalidOperationException("Donation must be in Processing state to mark as rejected.");
            }
            ProcessingState = ProcessingState.Rejected;
        }

        public void MarkAsTemporaryFailure()
        {
            if (ProcessingState != ProcessingState.Processing)
            {
                throw new InvalidOperationException("Donation must be in Processing state to mark as temporary failure.");
            }
            ProcessingState = ProcessingState.TemporaryFailure;
        }
    }
}
