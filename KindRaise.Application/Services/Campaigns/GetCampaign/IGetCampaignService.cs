using KindRaise.Application.Campaigns.GetCampaign;

namespace KindRaise.Application.Services.Campaigns.GetCampaign
{
    public interface IGetCampaignService
    {
        Task<GetCampaignResponse?> ExecuteAsync(
            GetCampaignRequest request,
            CancellationToken cancellationToken);
    }
}
