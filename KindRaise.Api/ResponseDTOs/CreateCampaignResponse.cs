namespace KindRaise.Api.ResponseDTOs
{
    public class CreateCampaignResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MonetaryGoal { get; set; }
    }
}
