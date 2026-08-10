using PegasusApi.Abstractions.Tickets;

namespace Taurus.Application.Tickets;

public interface ITicketService
{
    Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null);
}

public sealed class TicketService(HttpClient httpClient, ILogger<TicketService> logger) : ITicketService
{
    public async Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null)
    {
        logger.LogInformation("Retrieving tickets from PegasusApi for project {ProjectId}", projectId);

        try
        {
            var requestUri = projectId.HasValue
                ? $"api/tickets?ProjectId={Uri.EscapeDataString(projectId.Value.ToString())}"
                : "api/tickets";

            var response = await httpClient.GetFromJsonAsync<TicketsResponse>(requestUri);
            if (response is null)
            {
                throw new InvalidOperationException("PegasusApi returned an empty tickets response.");
            }

            var tickets = response.Items
                .Select(MapTicket)
                .ToArray();

            logger.LogInformation("Retrieved {TicketCount} tickets from PegasusApi for project {ProjectId}", tickets.Length, projectId);
            return tickets;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to retrieve tickets from PegasusApi for project {ProjectId}", projectId);
            throw;
        }
    }

    private static Ticket MapTicket(TicketResponse ticket)
    {
        return new Ticket(
            ticket.Id,
            ticket.TicketRef,
            ticket.Title,
            ticket.StatusId,
            ticket.LastModified);
    }
}