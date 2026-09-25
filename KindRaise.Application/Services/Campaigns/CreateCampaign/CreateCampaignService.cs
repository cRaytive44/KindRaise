using FluentValidation;
using KindRaise.Application.Campaigns;
using KindRaise.Application.Campaigns.CreateCampaign;
using KindRaise.Domain.Campaign;
namespace KindRaise.Application.Services.Campaigns.CreateCampaign
{
    public sealed class CreateCampaignService : ICreateCampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateCampaignRequest> _validator;

        public CreateCampaignService(
            ICampaignRepository campaignRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateCampaignRequest> validator)
        {
            _campaignRepository = campaignRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<CreateCampaignResponse> ExecuteAsync(
            CreateCampaignRequest request,
            CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var campaign = new Campaign
            (
                request.Title!,
                request.Description!,
                request.MonetaryGoal,
                request.StartDate,
                request.EndDate
            );

            await _campaignRepository.AddAsync(campaign, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateCampaignResponse
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                MonetaryGoal = campaign.MonetaryGoal,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                DonatedAmount = campaign.DonatedAmount,
                CampaignState = campaign.CampaignState
            };
        }
    }
}
