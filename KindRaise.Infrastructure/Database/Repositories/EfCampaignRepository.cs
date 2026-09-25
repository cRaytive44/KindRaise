using KindRaise.Application.Campaigns;
using KindRaise.Application.Donations;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KindRaise.Infrastructure.Database.Repositories
{
    public sealed class EfCampaignRepository(KindRaiseDbContext dbContext) : ICampaignRepository
    {
        public async Task AddAsync(
            Domain.Campaign.Campaign campaign,
            CancellationToken cancellationToken)
        {
            await dbContext.Campaigns.AddAsync(campaign, cancellationToken);
        }

        public async Task<Domain.Campaign.Campaign?> FirstOrDefaultAsync(
            Expression<Func<Domain.Campaign.Campaign, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await dbContext.Campaigns.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public IQueryable<Domain.Campaign.Campaign> GetAll(
            CancellationToken cancellationToken)
        {
            return dbContext.Campaigns.AsNoTracking().AsQueryable();
        }

        public void Remove(Domain.Campaign.Campaign campaign)
        {
            dbContext.Campaigns.Remove(campaign);
        }
    }
}
