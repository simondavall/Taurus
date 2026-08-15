namespace Taurus.Application.Tickets;

public sealed record UpdateTicketComment(
    Guid Id,
    string Content,
    bool IsDeleted);