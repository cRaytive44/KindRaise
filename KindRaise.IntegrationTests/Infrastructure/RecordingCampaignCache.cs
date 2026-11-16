using KindRaise.Application.Campaigns;
using KindRaise.Application.Campaigns.GetCampaign;
using System.Collections.Concurrent;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class RecordingCampaignCache : ICampaignCache
    {
        private readonly ConcurrentQueue<Guid> _removedCampaignIds = new();
        public IReadOnlyCollection<Guid> RemovedCampaignIds => _removedCampaignIds.ToArray();

        public Task<GetCampaignResponse?> GetAsync(Guid campaignId, CancellationToken cancellationToken)
            => Task.FromResult<GetCampaignResponse?>(null);

        public Task SetAsync(GetCampaignResponse campaign, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task RemoveAsync(Guid campaignId, CancellationToken cancellationToken) 
        {
            _removedCampaignIds.Enqueue(campaignId);
            return Task.CompletedTask;
        }

        public void Clear()
        {
            while (_removedCampaignIds.TryDequeue(out _))
            {
            }
        }
    }
}
