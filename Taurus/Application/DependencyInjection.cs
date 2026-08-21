using Taurus.Application.Html;
using Taurus.Application.Markdown;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Taurus.Application.UserState;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = configuration["PegasusApi:BaseAddress"];

        services.AddHttpClient<IProjectService, ProjectService>(client => { client.BaseAddress = new Uri(baseAddress!); });
        services.AddHttpClient<ITicketService, TicketService>(client => { client.BaseAddress = new Uri(baseAddress!); });
        services.AddHttpClient<ITicketReferenceDataService, TicketReferenceDataService>(client => { client.BaseAddress = new Uri(baseAddress!); });
        services.AddHttpClient<ITicketCommentService, TicketCommentService>(client => { client.BaseAddress = new Uri(baseAddress!); });
        services.AddHttpClient<ITicketReferenceLinker, TicketReferenceLinker>(client => { client.BaseAddress = new Uri(baseAddress!); });
        
        services.AddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
        services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();
        services.AddScoped<IUserStateService, UserStateService>();
        
        return services;
    }
}