namespace KindRaise.Application.Campaigns.CreateCampaign
{
    public sealed class CreateCampaignRequest
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public decimal MonetaryGoal { get; init; }
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
    }
}
