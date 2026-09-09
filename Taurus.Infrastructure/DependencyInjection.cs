using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Taurus.Application.Caching;
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
    private const int DefaultTicketLookupCacheDurationMinutes = 60;
    
    public static IServiceCollection AddTaurusInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        var baseAddress = configuration["PegasusApi:BaseAddress"];

        Action<HttpClient> client = httpClient => { httpClient.BaseAddress = new Uri(baseAddress!); };

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton(new TicketLookupCacheOptions(ResolveTicketLookupCacheDuration(configuration)));
        
        services.AddHttpClient<IProjectService, ProjectService>(client);
        services.AddHttpClient<ITicketService, TicketService>(client);
        services.AddHttpClient<ITicketLookupDataProvider, TicketLookupDataProvider>(client);
        services.AddHttpClient<ITicketCommentService, TicketCommentService>(client);
        services.AddHttpClient<ITicketRefLinker, TicketRefLinker>(client);
        services.AddHttpClient<IUserService, UserService>(client);

        return services;
    }
    
    private static TimeSpan ResolveTicketLookupCacheDuration(IConfiguration configuration)
    {
        var durationMinutes = configuration.GetValue<int?>("Caching:TicketLookups:DurationMinutes");

        return durationMinutes is > 0
            ? TimeSpan.FromMinutes(durationMinutes.Value)
            : TimeSpan.FromMinutes(DefaultTicketLookupCacheDurationMinutes);
    }
}