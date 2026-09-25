using KindRaise.Application.Campaigns;
using KindRaise.Application.Campaigns.GetCampaign;

namespace KindRaise.Application.Services.Campaigns.GetCampaign
{
    public sealed class GetCampaignService : IGetCampaignService
    {
        private readonly ICampaignRepository _campaignRepository;

        public GetCampaignService(ICampaignRepository campaignRepository)
        {
            _campaignRepository = campaignRepository;
        }

        public async Task<GetCampaignResponse?> ExecuteAsync(
            GetCampaignRequest request,
            CancellationToken cancellationToken)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(
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
