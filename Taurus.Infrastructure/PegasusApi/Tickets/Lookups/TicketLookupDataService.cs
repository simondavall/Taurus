using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PegasusApi.Abstractions.Lookups;
using Taurus.Application.Tickets.Lookups;

namespace Taurus.Infrastructure.PegasusApi.Tickets.Lookups;

public sealed class TicketLookupDataService(HttpClient httpClient, ILogger<TicketLookupDataService> logger) : ITicketLookupDataService
{
    public Task<IReadOnlyList<TicketStatus>> GetStatusesAsync()
    {
        return GetLookupAsync(
            "api/ticket-status",
            "ticket statuses",
            item => new TicketStatus(item.Id, item.Code, item.Title, item.DisplayOrder));
    }

    public Task<IReadOnlyList<TicketPriority>> GetPrioritiesAsync()
    {
        return GetLookupAsync(
            "api/ticket-priority",
            "ticket priorities",
            item => new TicketPriority(item.Id, item.Code, item.Title, item.DisplayOrder));
    }

    public Task<IReadOnlyList<TicketType>> GetTypesAsync()
    {
        return GetLookupAsync(
            "api/ticket-type",
            "ticket types",
            item => new TicketType(item.Id, item.Code, item.Title, item.DisplayOrder));
    }

    private async Task<IReadOnlyList<T>> GetLookupAsync<T>(string requestUri, string lookupName, Func<LookupResponse, T> map)
    {
        logger.LogInformation("Retrieving {LookupName} from PegasusApi", lookupName);

        try {
            var response = await httpClient.GetFromJsonAsync<LookupResponses>(requestUri);
            if (response is null) throw new InvalidOperationException($"PegasusApi returned an empty {lookupName} response.");

            var items = response.Items
                .OrderBy(item => item.DisplayOrder)
                .Select(map)
                .ToArray();

            logger.LogInformation("Retrieved {LookupCount} {LookupName} from PegasusApi", items.Length, lookupName);
            return items;
        } catch (Exception exception) {
            logger.LogError(exception, "Failed to retrieve {LookupName} from PegasusApi", lookupName);
            throw;
        }
    }
}