using System.Text.RegularExpressions;
using Taurus.Application.Tickets;

namespace Taurus.Application.Text;

public interface ITextProcessor
{
    Task<string?> LinkTicketRefsAsync(string? content);
}

public sealed partial class TextProcessor(ITicketDataProvider ticketDataProvider) : ITextProcessor
{
    public async Task<string?> LinkTicketRefsAsync(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        var matches = TicketRefRegex().Matches(content);
        if (matches.Count == 0)
            return content;

        var references = matches
            .Select(match => match.Groups["ticketRef"].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var existingReferences = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var ticketRef in references) {
            if (await IsExistingTicketRefAsync(ticketRef)) {
                existingReferences[ticketRef] = ticketRef;
            }
        }

        if (existingReferences.Count == 0)
            return content;

        return TicketRefRegex()
            .Replace(
                content,
                match => {
                    var ticketRef = match.Groups["ticketRef"].Value;
                    return !existingReferences.TryGetValue(ticketRef, out var existingTicketRef)
                        ? match.Value
                        : $"[{existingTicketRef}](/tickets/{existingTicketRef})";
                });
    }

    private async Task<bool> IsExistingTicketRefAsync(string ticketRef)
    {
        //logger.LogInformation("Verifying ticket reference {TicketRef} with PegasusApi", ticketRef);
        var result = await ticketDataProvider.GetTicketByRefAsync(ticketRef);
        return result is { Succeeded: true, Value: not null };
    }

    [GeneratedRegex(@"\[(?<ticketRef>[A-Za-z][A-Za-z0-9]*-\d+)\](?!\()", RegexOptions.CultureInvariant)]
    private static partial Regex TicketRefRegex();
}