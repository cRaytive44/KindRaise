using KindRaise.Application.Campaigns;
using KindRaise.Application.Campaigns.GetCampaign;
using Microsoft.Extensions.Logging;

namespace KindRaise.Application.Services.Campaigns.GetCampaign
{
    public sealed class GetCampaignService : IGetCampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly ICampaignCache _campaignCache;
        private readonly ILogger<GetCampaignService> _logger;

        public GetCampaignService(
            ICampaignRepository campaignRepository,
            ICampaignCache campaignCache,
            ILogger<GetCampaignService> logger)
        {
            _campaignRepository = campaignRepository;
            _campaignCache = campaignCache;
            _logger = logger;
        }

        public async Task<GetCampaignResponse?> ExecuteAsync(
            GetCampaignRequest request,
            CancellationToken cancellationToken)
        {
            var cachedCampaign = await _campaignCache.GetAsync(request.Id, cancellationToken);

            if (cachedCampaign is not null)
            {
                _logger.LogInformation("Campaign {CampaignId} retrieved from cache.", request.Id);

                return cachedCampaign;
            }

            var campaign = await _campaignRepository.FirstOrDefaultAsync(
                c => c.Id == request.Id,
                    cancellationToken);

            if (campaign is null) return null;

            var response = new GetCampaignResponse
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

            await _campaignCache.SetAsync(response, cancellationToken);
            return response;
        }  
    }
}
