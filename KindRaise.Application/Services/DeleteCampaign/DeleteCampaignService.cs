using KindRaise.Application.Campaigns.DeleteCampaign;
using KindRaise.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KindRaise.Application.Services.DeleteCampaign
{
    public sealed class DeleteCampaignService : IDeleteCampaignService
    {
        private readonly KindRaiseDbContext _dbContext;

        public DeleteCampaignService(KindRaiseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> DeleteAsync(
            DeleteCampaignRequest request,
            CancellationToken cancellationToken)
        {
            var campaign = await _dbContext.Campaigns
                .FirstOrDefaultAsync(
                    c => c.Id == request.Id,
                    cancellationToken);

            if (campaign is null) return false;

            campaign.EnsureCanBeDeleted();

            _dbContext.Campaigns.Remove(campaign);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
