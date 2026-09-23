using KindRaise.Application.Donations.GetDonation;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Application.Services.GetDonation
{
    public sealed class GetDonationService : IGetDonationService
    {
        private readonly KindRaiseDbContext _dbContext;

        public GetDonationService(KindRaiseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetDonationResponse?> ExecuteAsync(
            GetDonationRequest request,
            CancellationToken cancellationToken)
        {
            var donation = await _dbContext.Donations
                .AsNoTracking()
                .FirstOrDefaultAsync(
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
