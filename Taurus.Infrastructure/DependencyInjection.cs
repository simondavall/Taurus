using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Taurus.Application.Caching;
using Taurus.Application.Configuration;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Taurus.Application.Tickets.Comments;
using Taurus.Application.Tickets.Lookups;
using Taurus.Application.Users;
using Taurus.Infrastructure.Caching;
using Taurus.Infrastructure.PegasusApi.Projects;
using Taurus.Infrastructure.PegasusApi.Tickets;
using Taurus.Infrastructure.PegasusApi.Tickets.Comments;
using Taurus.Infrastructure.PegasusApi.Tickets.Lookups;
using Taurus.Infrastructure.PegasusApi.Users;

namespace Taurus.Infrastructure;

public static class DependencyInjection
{
    public static void AddTaurusInfrastructure(this IServiceCollection services, TaurusSettings settings)
    {
        var baseAddress = settings.PegasusApi.BaseAddress;
        Action<HttpClient> client = httpClient => { httpClient.BaseAddress = baseAddress; };

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton(new TicketLookupCacheOptions(settings.Caching.TicketLookups.Duration));
        services.AddSingleton(new ProjectCacheOptions(settings.Caching.Projects.Duration));

        services.AddHttpClient<IProjectDataProvider, PegasusProjectDataProvider>(client);
        services.AddHttpClient<ITicketDataProvider, TicketDataProvider>(client);
        services.AddHttpClient<ITicketLookupDataProvider, TicketLookupDataProvider>(client);
        services.AddHttpClient<ITicketCommentDataProvider, TicketCommentDataProvider>(client);
        services.AddHttpClient<IUserDataProvider, UserDataProvider>(client);
    }
}