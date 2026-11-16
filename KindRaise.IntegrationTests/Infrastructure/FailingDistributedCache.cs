using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace KindRaise.IntegrationTests.Infrastructure
{
    public sealed class FailingDistributedCache : IDistributedCache
    {
        private static RedisException RedisUnavailable()
            => new RedisConnectionException(
                ConnectionFailureType.InternalFailure,
                "Simulated Redis connection failure");

        public byte[]? Get(string key) => throw RedisUnavailable();

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            => Task.FromException<byte[]?>(RedisUnavailable());

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            => throw RedisUnavailable();

        public Task SetAsync(
            string key, 
            byte[] value, 
            DistributedCacheEntryOptions options, 
            CancellationToken token = default)
            => Task.FromException(RedisUnavailable());

        public void Refresh(string key) => throw RedisUnavailable();

        public Task RefreshAsync(string key, CancellationToken token = default)
            => Task.FromException(RedisUnavailable());

        public void Remove(string key) => throw RedisUnavailable();

        public Task RemoveAsync(string key, CancellationToken token = default)
            => Task.FromException(RedisUnavailable());
    }
}
