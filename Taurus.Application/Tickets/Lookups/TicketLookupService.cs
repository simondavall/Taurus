using Taurus.Application.Caching;

namespace Taurus.Application.Tickets.Lookups;

public interface ITicketLookupService
{
    Task<IReadOnlyList<TicketPriority>> GetPrioritiesAsync();
    Task<IReadOnlyList<TicketStatus>> GetStatusesAsync();
    Task<IReadOnlyList<TicketType>> GetTypesAsync();
}

public sealed class TicketLookupService(
    ITicketLookupDataProvider dataProvider,
    ICacheService cacheService,
    TicketLookupCacheOptions cacheOptions) : ITicketLookupService
{
    private const string StatusesCacheKey = "ticket-lookups:statuses";
    private const string PrioritiesCacheKey = "ticket-lookups:priorities";
    private const string TypesCacheKey = "ticket-lookups:types";

    public Task<IReadOnlyList<TicketStatus>> GetStatusesAsync()
    {
        return cacheService.GetOrCreateAsync(
            StatusesCacheKey,
            cacheOptions.Duration,
            dataProvider.GetStatusesAsync);
    }

    public Task<IReadOnlyList<TicketPriority>> GetPrioritiesAsync()
    {
        return cacheService.GetOrCreateAsync(
            PrioritiesCacheKey,
            cacheOptions.Duration,
            dataProvider.GetPrioritiesAsync);
    }

    public Task<IReadOnlyList<TicketType>> GetTypesAsync()
    {
        return cacheService.GetOrCreateAsync(
            TypesCacheKey,
            cacheOptions.Duration,
            dataProvider.GetTypesAsync);
    }
}