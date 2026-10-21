namespace Taurus.Application.Tickets;

public interface ITicketService
{
    Task<ApplicationResult<TicketDetails>> CreateTicketAsync(CreateTicket createTicket, Guid userId);
    Task<IReadOnlyList<Ticket>> GetSubTasksAsync(string parentTicketRef);
    Task<ApplicationResult<TicketDetails>> GetTicketByRefAsync(string ticketRef);
    Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null);
    Task<ApplicationResult> UpdateTicketAsync(UpdateTicket updateTicket, Guid userId);
}

public sealed class TicketService(ITicketDataProvider ticketDataProvider) : ITicketService
{
    public Task<ApplicationResult<TicketDetails>> CreateTicketAsync(CreateTicket createTicket, Guid userId)
    {
        return ticketDataProvider.CreateTicketAsync(createTicket, userId);
    }

    public Task<IReadOnlyList<Ticket>> GetSubTasksAsync(string parentTicketRef)
    {
        return ticketDataProvider.GetSubTasksAsync(parentTicketRef);
    }

    public Task<ApplicationResult<TicketDetails>> GetTicketByRefAsync(string ticketRef)
    {
        return ticketDataProvider.GetTicketByRefAsync(ticketRef);
    }

    public Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null)
    {
        return ticketDataProvider.GetTicketsAsync(projectId);
    }

    public Task<ApplicationResult> UpdateTicketAsync(UpdateTicket updateTicket, Guid userId)
    {
        return ticketDataProvider.UpdateTicketAsync(updateTicket, userId);
    }
}