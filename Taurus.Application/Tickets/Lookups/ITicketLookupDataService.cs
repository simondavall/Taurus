namespace Taurus.Application.Tickets.Lookups;

public interface ITicketLookupDataService
{
    Task<IReadOnlyList<TicketPriority>> GetPrioritiesAsync();
    Task<IReadOnlyList<TicketStatus>> GetStatusesAsync();
    Task<IReadOnlyList<TicketType>> GetTypesAsync();
}