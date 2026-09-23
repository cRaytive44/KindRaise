using KindRaise.Domain.Donation;

namespace KindRaise.Application.Donations.GetDonations
{
    public sealed class GetDonationsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? CampaignId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public ProcessingState? ProcessingState { get; set; }
    }
}
