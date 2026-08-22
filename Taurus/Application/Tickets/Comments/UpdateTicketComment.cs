namespace Taurus.Application.Tickets.Comments;

public sealed record UpdateTicketComment(
    Guid Id,
    string Content,
    bool IsDeleted);