namespace Taurus.Application.Tickets;

public interface ITicketDataProvider
{
    Task<ApplicationResult<TicketDetails>> CreateTicketAsync(CreateTicket createTicket, Guid userId);
    Task<IReadOnlyList<Ticket>> GetSubTasksAsync(string parentTicketRef);
    Task<ApplicationResult<TicketDetails>> GetTicketByRefAsync(string ticketRef);
    Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null);
    Task<ApplicationResult> UpdateTicketAsync(UpdateTicket updateTicket, Guid userId);
}