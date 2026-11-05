using FluentValidation;
using KindRaise.Application.Campaigns;
using KindRaise.Application.Donations;
using KindRaise.Application.Donations.CreateDonation;
using KindRaise.Application.Messaging;
using KindRaise.Contracts.Donations;
using System.Text.Json;

namespace KindRaise.Application.Services.Donations.CreateDonation
{
    public sealed class CreateDonationService : ICreateDonationService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IDonationRepository _donationRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateDonationRequest> _validator;


        public CreateDonationService(
            ICampaignRepository campaignRepository,
            IDonationRepository donationRepository,
            IOutboxRepository outboxRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateDonationRequest> validator)
        {
            _campaignRepository = campaignRepository;
            _donationRepository = donationRepository;
            _outboxRepository = outboxRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<CreateDonationResponse> ExecuteAsync(
            CreateDonationRequest request,
            CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var campaign = await _campaignRepository.FirstOrDefaultAsync(
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

            var message = new DonationRequested(
                donation.Id,
                donation.CampaignId,
                donation.Amount);

            var content = JsonSerializer.Serialize(message);
            var type = typeof(DonationRequested).FullName!;
            var occurredAt = DateTimeOffset.UtcNow;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                await _donationRepository.AddAsync(donation, cancellationToken);
                await _outboxRepository.AddAsync(type, content, occurredAt, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

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
