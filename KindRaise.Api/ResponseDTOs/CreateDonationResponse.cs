using KindRaise.Domain.Donation;

namespace KindRaise.Api.ResponseDTOs
{
    public class CreateDonationResponse
    {
        public Guid Id { get; init; }
        public Guid CampaignId { get; init; }
        public string DonorName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public ProcessingState ProcessingState { get; init; }
    }
}
