using KindRaise.Domain.Campaign;

namespace KindRaise.Domain.Campain
{
    public class Campaign
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; }
        public string Description { get; }
        public decimal MonetaryGoal { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public CampaignInformation Information { get; set; } = new CampaignInformation();

        public Campaign(string title, string description, decimal monetaryGoal, DateTime startDate, DateTime endDate)
        {
            Title = title;
            Description = description;
            MonetaryGoal = monetaryGoal;
            StartDate = startDate;
            EndDate = endDate;
        }

    }

    public class CampaignInformation 
    {
        public decimal DonatedAmount { get; set; }
        public CampaignState CampaignState { get; set; }
    }
}
