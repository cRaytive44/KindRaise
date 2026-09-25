using KindRaise.Application.Donations;
using KindRaise.Application.Donations.GetDonation;

namespace KindRaise.Application.Services.Donations.GetDonation
{
    public sealed class GetDonationService : IGetDonationService
    {
        private readonly IDonationRepository _donationRepository;

        public GetDonationService(IDonationRepository donationRepository)
        {
            _donationRepository = donationRepository;
        }

        public async Task<GetDonationResponse?> ExecuteAsync(
            GetDonationRequest request,
            CancellationToken cancellationToken)
        {
            var donation = await _donationRepository.FirstOrDefaultAsync(
                d => d.Id == request.Id,
                    cancellationToken);

            if (donation is null) return null;

            return new GetDonationResponse
            {
                Id = donation.Id,
                CampaignId = donation.CampaignId,
                DonorName = donation.DonorName,
                Amount = donation.Amount,
                CreatedAt = donation.CreatedAt,
                ProcessingState = donation.ProcessingState
            };
        }
    }
}
