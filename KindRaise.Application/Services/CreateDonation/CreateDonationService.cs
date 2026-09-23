using FluentValidation;
using KindRaise.Application.Donations.CreateDonation;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Application.Services.CreateDonation
{
    public sealed class CreateDonationService : ICreateDonationService
    {
        private readonly KindRaiseDbContext _dbContext;
        private readonly IValidator<CreateDonationRequest> _validator;

        public CreateDonationService(
            KindRaiseDbContext dbContext,
            IValidator<CreateDonationRequest> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<CreateDonationResponse> ExecuteAsync(
            CreateDonationRequest request,
            CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var campaign = await _dbContext.Campaigns.FirstOrDefaultAsync(
                c => c.Id == request.CampaignId,
                cancellationToken);

            if (campaign is null)
            {
                throw new InvalidOperationException("Campaign not found");
            }

            var donation = new Domain.Donation.Donation
            (
                request.CampaignId,
                request.DonorName!,
                request.Amount
            );

            await _dbContext.Donations.AddAsync(donation, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateDonationResponse
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
