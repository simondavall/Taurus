namespace Taurus.Application.Tickets.Lookups;

public interface ITicketLookupDataProvider
{
    Task<IReadOnlyList<TicketPriority>> GetPrioritiesAsync();
    Task<IReadOnlyList<TicketStatus>> GetStatusesAsync();
    Task<IReadOnlyList<TicketType>> GetTypesAsync();
}