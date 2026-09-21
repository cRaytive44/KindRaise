using FluentValidation;
using KindRaise.Application.Campaigns.CreateCampaign;
using KindRaise.Domain.Campaign;
using KindRaise.Infrastructure.Database;

namespace KindRaise.Application.Services.CreateCampaign
{
    public sealed class CreateCampaignService
        : ICreateCampaignService
    {
        private readonly KindRaiseDbContext _dbContext;
        private readonly IValidator<CreateCampaignRequest> _validator;

        public CreateCampaignService(
            KindRaiseDbContext dbContext,
            IValidator<CreateCampaignRequest> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<CreateCampaignResponse> ExecuteAsync(
            CreateCampaignRequest request,
            CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(
                request,
                cancellationToken);

            var campaign = new Campaign
            (
                request.Title!,
                request.Description!,
                request.MonetaryGoal,
                request.StartDate,
                request.EndDate
            );

            await _dbContext.Campaigns.AddAsync(campaign, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

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
