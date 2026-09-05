using Markdig;
using Taurus.Application.Html;

namespace Taurus.Application.Markdown;

public interface IMarkdownRenderer
{
    string Render(string? markdown);
}

public sealed class MarkdownRenderer(IHtmlContentSanitizer htmlContentSanitizer) : IMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

    public string Render(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var html = Markdig.Markdown.ToHtml(markdown, Pipeline);

        return htmlContentSanitizer.Sanitize(html);
    }
}