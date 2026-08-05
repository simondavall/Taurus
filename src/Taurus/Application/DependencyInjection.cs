using Microsoft.Extensions.DependencyInjection;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusApplication(this IServiceCollection services)
    {
        return services;
    }
}