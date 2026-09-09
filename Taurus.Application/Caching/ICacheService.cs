namespace Taurus.Application.Caching;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, TimeSpan duration, Func<Task<T>> factory)
        where T : notnull;

    void Remove(string key);
}