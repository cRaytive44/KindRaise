using KindRaise.Domain.Campaign;

namespace KindRaise.Application.Campaigns.GetCampaigns
{
    public sealed class GetCampaignsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTimeOffset? StartDateFrom { get; set; }
        public DateTimeOffset? StartDateTo { get; set; }
        public DateTimeOffset? EndDateFrom { get; set; }
        public DateTimeOffset? EndDateTo { get; set; }
        public decimal? MinGoal { get; set; }
        public decimal? MaxGoal { get; set; }
        public decimal? MinDonatedAmount { get; set; }
        public decimal? MaxDonatedAmount { get; set; }
        public CampaignState? State { get; set; }
    }
}
