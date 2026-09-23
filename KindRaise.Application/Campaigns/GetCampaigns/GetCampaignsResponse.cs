using KindRaise.Domain.Campaign;

namespace KindRaise.Application.Campaigns.GetCampaigns
{
    public sealed class GetCampaignsResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal MonetaryGoal { get; init; }
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
        public decimal DonatedAmount { get; init; }
        public CampaignState CampaignState { get; init; }
    }
}
