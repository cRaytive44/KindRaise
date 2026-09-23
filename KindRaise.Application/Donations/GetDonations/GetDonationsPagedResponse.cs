using KindRaise.Application.Campaigns.GetCampaigns;

namespace KindRaise.Application.Donations.GetDonations
{
    public sealed class GetDonationsPagedResponse
    {
        public List<GetDonationsResponse> Items { get; init; } = [];
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
    }
}
