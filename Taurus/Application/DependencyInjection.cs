using Microsoft.Extensions.DependencyInjection;
using Taurus.Application.Projects;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        
        return services;
    }
}