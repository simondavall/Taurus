using Microsoft.Extensions.DependencyInjection;
using Taurus.Application.Projects;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProjectService, ProjectService>();
        
        services.AddHttpClient<IProjectService, ProjectService>(client =>
        {
            var baseAddress = configuration["PegasusApi:BaseAddress"]
                          ?? throw new InvalidOperationException("PegasusApi:BaseAddress is not configured.");
            
            client.BaseAddress = new Uri(baseAddress);
        });
        
        return services;
    }
}