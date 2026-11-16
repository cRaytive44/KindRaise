using KindRaise.Application.Campaigns;
using KindRaise.Application.Campaigns.GetCampaign;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace KindRaise.Infrastructure.Caching
{
    public sealed class RedisCampaignCache(
        IDistributedCache distributedCache,
        ILogger<RedisCampaignCache> logger) : ICampaignCache
    {
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromSeconds(30);
        private static string GetCacheKey(Guid campaignId) => $"campaign:{campaignId}";

        public async Task<GetCampaignResponse?> GetAsync(Guid campaignId, CancellationToken cancellationToken)
        {
            try
            {
                var cachedData = await distributedCache.GetStringAsync(
                    GetCacheKey(campaignId), cancellationToken);

                if (cachedData is null) return null;

                return JsonSerializer.Deserialize<GetCampaignResponse>(cachedData);
            }

            catch (RedisException exception)
            {
                logger.LogWarning(
                    exception,
                    "Redis unavailable while retrieving campaign {CampaignId}. Falling back to PostgreSQL.",
                    campaignId);

                return null;
            }

            catch (JsonException exception)
            {
                logger.LogWarning(
                    exception,
                    "Invalid cached data for campaign {CampaignId}. Treating as cache miss.",
                    campaignId);

                return null;
            }
        }

        public async Task SetAsync(GetCampaignResponse campaign, CancellationToken cancellationToken) 
        {
            try
            {
                var serializedData = JsonSerializer.Serialize(campaign);

                await distributedCache.SetStringAsync(
                    GetCacheKey(campaign.Id),
                    serializedData,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = CacheExpiration
                    },
                    cancellationToken);
            }

            catch (RedisException exception)
            {
                logger.LogWarning(
                    exception,
                    "Redis unavailable while caching campaign {CampaignId}.",
                    campaign.Id);
            }
        }

        public async Task RemoveAsync(Guid campaignId, CancellationToken cancellationToken)
        {
            try
            {
                await distributedCache.RemoveAsync(
                    GetCacheKey(campaignId),
                    cancellationToken);
            }

            catch (RedisException exception)
            {
                logger.LogWarning(
                    exception,
                    "Redis unavailable while removing campaign {CampaignId} from cache.",
                    campaignId);
            }
        }
    }
}
