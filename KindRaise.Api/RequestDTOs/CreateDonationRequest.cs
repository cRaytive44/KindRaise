namespace KindRaise.Api.RequestDTOs
{
    public class CreateDonationRequest
    {
        public string DonorName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
