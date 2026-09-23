namespace KindRaise.Application.Campaigns.GetCampaigns
{
    public sealed class GetCampaignsPagedResponse
    {
        public List<GetCampaignsResponse> Items { get; init; } = [];
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
    }
}
