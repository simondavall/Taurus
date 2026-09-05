namespace Taurus.Application.Tickets;

public interface ITicketRefLinker
{
    Task<string?> LinkTicketRefsAsync(string? content);
}