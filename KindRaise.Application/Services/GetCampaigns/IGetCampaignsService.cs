using KindRaise.Application.Campaigns.GetCampaigns;

namespace KindRaise.Application.Services.GetCampaigns
{
    public interface IGetCampaignsService
    {
        Task<GetCampaignsPagedResponse> GetAllAsync(
            GetCampaignsRequest request,
            CancellationToken cancellationToken);
    }
}
