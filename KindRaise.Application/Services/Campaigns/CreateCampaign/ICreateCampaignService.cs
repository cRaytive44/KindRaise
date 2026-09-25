using KindRaise.Application.Campaigns.CreateCampaign;

namespace KindRaise.Application.Services.Campaigns.CreateCampaign
{
    public interface ICreateCampaignService
    {
        Task<CreateCampaignResponse> ExecuteAsync(
            CreateCampaignRequest request, 
            CancellationToken cancellationToken);
    }
}
