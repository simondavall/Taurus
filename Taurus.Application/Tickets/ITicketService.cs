namespace Taurus.Application.Tickets;

public interface ITicketService {
    Task<ApplicationResult<TicketDetails>> CreateTicketAsync(CreateTicketRequest request, Guid userId);
    Task<IReadOnlyList<Ticket>> GetSubTasksAsync(string parentTicketRef);
    Task<ApplicationResult<TicketDetails>> GetTicketByRefAsync(string ticketRef);
    Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null);
    Task<ApplicationResult> UpdateTicketAsync(UpdateTicketRequest request, Guid userId);
}