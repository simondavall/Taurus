using Taurus.Application.Tickets;

namespace Taurus.Application.UserState;

public interface IUserStateService
{
    Task<Guid?> GetSelectedProjectIdAsync();
    Task SetSelectedProjectIdAsync(Guid? projectId);
    Task<TicketFilter?> GetSelectedTicketFilterAsync();
    Task SetSelectedTicketFilterAsync(TicketFilter filter);
}