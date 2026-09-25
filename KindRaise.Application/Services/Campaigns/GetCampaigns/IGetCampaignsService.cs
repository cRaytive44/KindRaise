using KindRaise.Application.Campaigns.GetCampaigns;

namespace KindRaise.Application.Services.Campaigns.GetCampaigns
{
    public interface IGetCampaignsService
    {
        Task<GetCampaignsPagedResponse> GetAllAsync(
            GetCampaignsRequest request,
            CancellationToken cancellationToken);
    }
}
