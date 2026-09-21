using KindRaise.Domain.Exceptions;

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
        public CampaignState CampaignState
        {
            get
            {
                var now = DateTimeOffset.UtcNow;

                return now >= StartDate &&
                       now <= EndDate
                    ? CampaignState.Active
                    : CampaignState.Inactive;
            }
        }

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

        public void AddDonation(decimal amount)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

            DonatedAmount += amount;
        }

        public void EnsureCanBeDeleted()
        {
            if (DonatedAmount > 0 &&
                CampaignState == CampaignState.Active)
            {
                throw new CampaignDeletionNotAllowedException(
                    "An active campaign with donations cannot be deleted.");
            }
        }
    }
}
