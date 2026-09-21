using KindRaise.Application.Campaigns.GetCampaign;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Application.Services.GetCampaign
{
    public sealed class GetCampaignService : IGetCampaignService
    {
        private readonly KindRaiseDbContext _dbContext;

        public GetCampaignService(KindRaiseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetCampaignResponse?> ExecuteAsync(
            GetCampaignRequest request,
            CancellationToken cancellationToken)
        {
            var campaign = await _dbContext.Campaigns
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Id == request.Id,
                    cancellationToken);

            if (campaign is null) return null;

            return new GetCampaignResponse
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
