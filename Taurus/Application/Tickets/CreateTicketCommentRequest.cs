namespace Taurus.Application.Tickets;

public sealed record CreateTicketCommentRequest(
    Guid TicketId,
    string Content);