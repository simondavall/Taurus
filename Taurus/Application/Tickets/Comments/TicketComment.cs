namespace Taurus.Application.Tickets.Comments;

public sealed record TicketComment(
    Guid Id,
    Guid TicketId,
    string Content,
    bool IsDeleted,
    string DisplayName,
    Guid LastModifiedBy,
    DateTimeOffset LastModified,
    Guid CreatedBy,
    DateTimeOffset CreatedDate);