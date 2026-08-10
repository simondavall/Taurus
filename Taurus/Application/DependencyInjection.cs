using Taurus.Application.Projects;
using Taurus.Application.Tickets;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = configuration["PegasusApi:BaseAddress"];

        services.AddHttpClient<IProjectService, ProjectService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress!);
        });

        services.AddHttpClient<ITicketService, TicketService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress!);
        });

        return services;
    }
}