using PegasusApi.Abstractions.Tickets;

namespace Taurus.Application.Tickets;

public interface ITicketService
{
    Task<IReadOnlyList<Ticket>> GetTicketsAsync();
}

public sealed class TicketService(HttpClient httpClient, ILogger<TicketService> logger) : ITicketService
{
    public async Task<IReadOnlyList<Ticket>> GetTicketsAsync()
    {
        logger.LogInformation("Retrieving tickets from PegasusApi");

        try
        {
            var response = await httpClient.GetFromJsonAsync<TicketsResponse>("api/tickets");
            if (response is null)
            {
                throw new InvalidOperationException("PegasusApi returned an empty tickets response.");
            }

            var tickets = response.Items
                .Select(MapTicket)
                .ToArray();

            logger.LogInformation("Retrieved {TicketCount} tickets from PegasusApi", tickets.Length);
            return tickets;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to retrieve tickets from PegasusApi");
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