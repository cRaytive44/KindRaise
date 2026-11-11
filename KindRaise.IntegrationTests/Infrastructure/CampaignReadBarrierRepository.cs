using KindRaise.Application.Campaigns;
using KindRaise.Domain.Campaign;
using System.Linq.Expressions;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class CampaignReadBarrierRepository(
        ICampaignRepository campaignRepository,
        CampaignReadBarrier campaignReadBarrier) : ICampaignRepository
    {
        public Task AddAsync(Campaign campaign, CancellationToken cancellationToken)
        {
            return campaignRepository.AddAsync(campaign, cancellationToken);
        }

        public async Task<Campaign?> FirstOrDefaultAsync(
            Expression<Func<Campaign, bool>> predicate,
            CancellationToken cancellationToken)
        {
            var campaign = await campaignRepository.FirstOrDefaultAsync(predicate, cancellationToken);
            await campaignReadBarrier.WaitForInitialReadsAsync(cancellationToken);
            return campaign;
        }

        public IQueryable<Campaign> GetAll(CancellationToken cancellationToken)
        {
            return campaignRepository.GetAll(cancellationToken);
        }

        public void Remove(Campaign campaign)
        {
            campaignRepository.Remove(campaign);
        }
    }
}
