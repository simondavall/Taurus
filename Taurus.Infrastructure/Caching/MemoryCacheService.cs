using Microsoft.Extensions.Caching.Memory;
using Taurus.Application.Caching;

namespace Taurus.Infrastructure.Caching;

public sealed class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan duration, Func<Task<T>> factory)
        where T : notnull
    {
        var value = await memoryCache.GetOrCreateAsync(
            key,
            entry => {
                entry.AbsoluteExpirationRelativeToNow = duration;
                return factory();
            });

        return value ?? throw new InvalidOperationException($"Cache factory returned no value for key '{key}'.");
    }

    public void Remove(string key)
    {
        memoryCache.Remove(key);
    }
}