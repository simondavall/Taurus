using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Taurus.Application.Tickets.Comments;
using Taurus.Application.Tickets.Lookups;
using Taurus.Application.Users;
using Taurus.Infrastructure.PegasusApi.Projects;
using Taurus.Infrastructure.PegasusApi.Tickets;
using Taurus.Infrastructure.PegasusApi.Tickets.Comments;
using Taurus.Infrastructure.PegasusApi.Tickets.Lookups;
using Taurus.Infrastructure.PegasusApi.Users;

namespace Taurus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseAddress = configuration["PegasusApi:BaseAddress"];

        Action<HttpClient> client = httpClient => { httpClient.BaseAddress = new Uri(baseAddress!); };

        services.AddHttpClient<IProjectService, ProjectService>(client);
        services.AddHttpClient<ITicketService, TicketService>(client);
        services.AddHttpClient<ITicketLookupDataService, TicketLookupDataService>(client);
        services.AddHttpClient<ITicketCommentService, TicketCommentService>(client);
        services.AddHttpClient<ITicketRefLinker, TicketRefLinker>(client);
        services.AddHttpClient<IUserService, UserService>(client);

        return services;
    }
}