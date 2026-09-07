namespace KindRaise.Domain.Donation
{
    public class Donation
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public string DonorName{ get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProcessingState ProcessingState { get; set; }
    }
}
