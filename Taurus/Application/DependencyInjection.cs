using Taurus.Application.Html;
using Taurus.Application.Markdown;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Taurus.Application.Tickets.Comments;
using Taurus.Application.Tickets.Lookups;
using Taurus.Application.Users;
using Taurus.Application.UserState;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = configuration["PegasusApi:BaseAddress"];

        Action<HttpClient> client = c => { c.BaseAddress = new Uri(baseAddress!); };
        
        services.AddHttpClient<IProjectService, ProjectService>(client);
        services.AddHttpClient<ITicketService, TicketService>(client);
        services.AddHttpClient<ITicketLookupDataService, TicketLookupDataService>(client);
        services.AddHttpClient<ITicketCommentService, TicketCommentService>(client);
        services.AddHttpClient<ITicketRefLinker, TicketRefLinker>(client);
        services.AddHttpClient<IUserService, UserService>(client);
        
        services.AddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
        services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();
        services.AddScoped<IUserStateService, UserStateService>();
        
        return services;
    }
}