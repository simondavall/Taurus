using Microsoft.Extensions.DependencyInjection;
using Taurus.Application.Html;
using Taurus.Application.Markdown;
using Taurus.Application.Projects;
using Taurus.Application.Tickets.Lookups;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusApplication(this IServiceCollection services)
    {
        services.AddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
        services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();
        services.AddScoped<ITicketLookupDataService, TicketLookupDataService>();
        services.AddScoped<IProjectService, ProjectService>();
        
        return services;
    }
}