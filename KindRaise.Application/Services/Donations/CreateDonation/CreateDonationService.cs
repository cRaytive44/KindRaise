using FluentValidation;
using KindRaise.Application.Campaigns;
using KindRaise.Application.Donations;
using KindRaise.Application.Donations.CreateDonation;
using KindRaise.Application.Messaging;
using KindRaise.Contracts.Donations;

namespace KindRaise.Application.Services.Donations.CreateDonation
{
    public sealed class CreateDonationService : ICreateDonationService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IDonationRepository _donationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IValidator<CreateDonationRequest> _validator;


        public CreateDonationService(
            ICampaignRepository campaignRepository,
            IDonationRepository donationRepository,
            IUnitOfWork unitOfWork,
            IMessagePublisher messagePublisher,
            IValidator<CreateDonationRequest> validator)
        {
            _campaignRepository = campaignRepository;
            _donationRepository = donationRepository;
            _unitOfWork = unitOfWork;
            _messagePublisher = messagePublisher;
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

            await _donationRepository.AddAsync(donation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var message = new DonationRequested(
                donation.Id,
                donation.CampaignId,
                donation.Amount);

            await _messagePublisher.PublishAsync(
                message,
                "donation.requested",
                cancellationToken);

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
