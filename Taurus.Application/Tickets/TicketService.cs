namespace Taurus.Application.Tickets;

public interface ITicketService
{
    Task<ApplicationResult<TicketDetails>> CreateTicketAsync(CreateTicketRequest request, Guid userId);
    Task<IReadOnlyList<Ticket>> GetSubTasksAsync(string parentTicketRef);
    Task<ApplicationResult<TicketDetails>> GetTicketByRefAsync(string ticketRef);
    Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null);
    Task<ApplicationResult> UpdateTicketAsync(UpdateTicketRequest request, Guid userId);
}

public sealed class TicketService(ITicketDataProvider ticketDataProvider) : ITicketService
{
    public Task<ApplicationResult<TicketDetails>> CreateTicketAsync(CreateTicketRequest request, Guid userId)
    {
        return ticketDataProvider.CreateTicketAsync(request, userId);
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

    public Task<ApplicationResult> UpdateTicketAsync(UpdateTicketRequest request, Guid userId)
    {
        return ticketDataProvider.UpdateTicketAsync(request, userId);
    }
}