namespace KindRaise.Api.RequestDTOs
{
    public class CreateCampaignRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MonetaryGoal { get; set; }

    }
}
