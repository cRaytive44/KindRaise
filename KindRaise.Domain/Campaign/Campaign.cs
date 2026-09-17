namespace KindRaise.Domain.Campaign
{
    public class Campaign
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; }
        public string Description { get; }
        public decimal MonetaryGoal { get; }
        public DateTimeOffset StartDate { get; }
        public DateTimeOffset EndDate { get; }
        public decimal DonatedAmount { get; private set; } = 0;
        public CampaignState CampaignState { get; private set; } = CampaignState.Inactive;

        public Campaign(string title, string description, decimal monetaryGoal, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(monetaryGoal);

            if (endDate <= startDate)
                throw new ArgumentException("End date must be after start date.");

            Title = title;
            Description = description;
            MonetaryGoal = monetaryGoal;
            StartDate = startDate;
            EndDate = endDate;
        }

        // Used by EF Core when materializing persisted entities.
        private Campaign() 
        {
            Title = string.Empty;
            Description = string.Empty;
            MonetaryGoal = 1;
            StartDate = DateTimeOffset.MinValue;
            EndDate = DateTimeOffset.MinValue;
        } 

        public void UpdateCampaignState()
        {
            var now = DateTimeOffset.UtcNow;

            CampaignState = now >= StartDate && now <= EndDate
                ? CampaignState.Active
                : CampaignState.Inactive;
        }

        public void AddDonation(decimal amount)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

            DonatedAmount += amount;
        }

    }
}
