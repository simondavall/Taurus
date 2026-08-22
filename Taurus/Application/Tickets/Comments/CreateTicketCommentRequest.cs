namespace Taurus.Application.Tickets.Comments;

public sealed record CreateTicketCommentRequest(
    Guid TicketId,
    string Content);