using Microsoft.Extensions.DependencyInjection;
using Taurus.Application.Html;
using Taurus.Application.Markdown;

namespace Taurus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaurusApplication(this IServiceCollection services)
    {
        services.AddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
        services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();

        return services;
    }
}