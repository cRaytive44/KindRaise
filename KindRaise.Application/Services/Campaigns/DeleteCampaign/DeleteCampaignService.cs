using KindRaise.Application.Campaigns;
using KindRaise.Application.Campaigns.DeleteCampaign;

namespace KindRaise.Application.Services.Campaigns.DeleteCampaign
{
    public sealed class DeleteCampaignService : IDeleteCampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCampaignService(ICampaignRepository campaignRepository)
        {
            _campaignRepository = campaignRepository;
        }

        public async Task<bool> DeleteAsync(
            DeleteCampaignRequest request,
            CancellationToken cancellationToken)
        {
            var campaign = await _campaignRepository.FirstOrDefaultAsync(
                c => c.Id == request.Id,
                cancellationToken);

            if (campaign is null) return false;

            campaign.EnsureCanBeDeleted();

            _campaignRepository.Remove(campaign);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
