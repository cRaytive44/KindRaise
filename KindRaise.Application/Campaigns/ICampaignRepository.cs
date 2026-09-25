using System.Linq.Expressions;

namespace KindRaise.Application.Campaigns
{
    public interface ICampaignRepository
    {
        Task<Domain.Campaign.Campaign?> FirstOrDefaultAsync(
            Expression<Func<Domain.Campaign.Campaign, bool>> predicate,
            CancellationToken cancellationToken);

        Task AddAsync(
            Domain.Campaign.Campaign campaign,
            CancellationToken cancellationToken);

        IQueryable<Domain.Campaign.Campaign> GetAll(
            CancellationToken cancellationToken);

        void Remove(Domain.Campaign.Campaign campaign);
    }
}
