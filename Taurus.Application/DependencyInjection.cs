using Microsoft.Extensions.DependencyInjection;
using Taurus.Application.Html;
using Taurus.Application.Markdown;
using Taurus.Application.Projects;
using Taurus.Application.Text;
using Taurus.Application.Tickets;
using Taurus.Application.Tickets.Comments;
using Taurus.Application.Tickets.Lookups;
using Taurus.Application.Users;
using TextProcessor = Taurus.Application.Text.TextProcessor;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static void AddTaurusApplication(this IServiceCollection services)
    {
        services.AddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
        services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketCommentService, TicketCommentService>();
        services.AddScoped<ITicketLookupService, TicketLookupService>();
        services.AddScoped<ITextProcessor, TextProcessor>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IUserService, UserService>();
    }
}