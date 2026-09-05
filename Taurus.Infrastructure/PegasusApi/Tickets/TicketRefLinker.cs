using System.Net;
using System.Text.RegularExpressions;
using PegasusApi.Abstractions.Tickets;

namespace Taurus.Application.Tickets;

public interface ITicketRefLinker
{
    Task<string?> LinkTicketRefsAsync(string? content);
}

public sealed partial class TicketRefLinker(HttpClient httpClient, ILogger<TicketRefLinker> logger) : ITicketRefLinker
{
    public async Task<string?> LinkTicketRefsAsync(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        var matches = TicketRefRegex().Matches(content);
        if (matches.Count == 0)
        {
            return content;
        }

        var references = matches
            .Select(match => match.Groups["ticketRef"].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var existingReferences = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var ticketRef in references)
        {
            var existingTicketRef = await GetExistingTicketRefAsync(ticketRef);
            if (existingTicketRef is not null)
            {
                existingReferences[ticketRef] = existingTicketRef;
            }
        }

        if (existingReferences.Count == 0)
        {
            return content;
        }

        return TicketRefRegex().Replace(
            content,
            match =>
            {
                var ticketRef = match.Groups["ticketRef"].Value;
                if (!existingReferences.TryGetValue(ticketRef, out var existingTicketRef))
                {
                    return match.Value;
                }

                return $"[{existingTicketRef}](/tickets/{existingTicketRef})";
            });
    }

    private async Task<string?> GetExistingTicketRefAsync(string ticketRef)
    {
        logger.LogInformation("Verifying ticket reference {TicketRef} with PegasusApi", ticketRef);

        var escapedTicketRef = Uri.EscapeDataString(ticketRef);
        using var response = await httpClient.GetAsync($"api/tickets/by_ref/{escapedTicketRef}");
        if (response.IsSuccessStatusCode)
        {
            var ticket = await response.Content.ReadFromJsonAsync<TicketResponse>();
            if (ticket is null)
            {
                throw new InvalidOperationException("PegasusApi returned an empty ticket response.");
            }

            return ticket.TicketRef;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("Ticket reference {TicketRef} does not exist and will not be linked", ticketRef);
            return null;
        }

        response.EnsureSuccessStatusCode();

        throw new InvalidOperationException("PegasusApi ticket reference verification failed unexpectedly.");
    }

    [GeneratedRegex(@"\[(?<ticketRef>[A-Za-z][A-Za-z0-9]*-\d+)\](?!\()", RegexOptions.CultureInvariant)]
    private static partial Regex TicketRefRegex();
}