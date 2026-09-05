using Ganss.Xss;

namespace Taurus.Application.Html;

public interface IHtmlContentSanitizer
{
    string Sanitize(string? html);
}

public sealed class HtmlContentSanitizer : IHtmlContentSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlContentSanitizer()
    {
        _sanitizer = new HtmlSanitizer();

        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.UnionWith(
        [
            "a",
            "b",
            "blockquote",
            "br",
            "code",
            "div",
            "em",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "i",
            "li",
            "ol",
            "p",
            "pre",
            "s",
            "span",
            "strong",
            "u",
            "ul"
        ]);

        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.UnionWith(
        [
            "class",
            "href",
            "rel",
            "style",
            "target",
            "title"
        ]);

        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedCssProperties.UnionWith(
        [
            "background-color",
            "color",
            "font-style",
            "font-weight",
            "margin-left",
            "text-align",
            "text-decoration"
        ]);

        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.UnionWith(
        [
            "http",
            "https",
            "mailto"
        ]);
    }

    public string Sanitize(string? html)
    {
        return string.IsNullOrWhiteSpace(html)
            ? string.Empty
            : _sanitizer.Sanitize(html);
    }
}