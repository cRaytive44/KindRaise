using KindRaise.Application.Campaigns.DeleteCampaign;

namespace KindRaise.Application.Services.DeleteCampaign
{
    public interface IDeleteCampaignService
    {
       Task<bool> DeleteAsync(
           DeleteCampaignRequest request,
           CancellationToken cancellationToken);
    }
}
