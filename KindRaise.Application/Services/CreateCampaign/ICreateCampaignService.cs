using KindRaise.Application.Campaigns.CreateCampaign;

namespace KindRaise.Application.Services.CreateCampaign
{
    public interface ICreateCampaignService
    {
        Task<CreateCampaignResponse> ExecuteAsync(
            CreateCampaignRequest request, 
            CancellationToken cancellationToken);
    }
}
