using KindRaise.Application.Campaigns.GetCampaign;

namespace KindRaise.Application.Campaigns
{
    public interface ICampaignCache
    {
        Task<GetCampaignResponse?> GetAsync(Guid campaignId, CancellationToken cancellationToken);

        Task SetAsync(GetCampaignResponse campaign, CancellationToken cancellationToken);

        Task RemoveAsync(Guid campaignId, CancellationToken cancellationToken);
    }
}
