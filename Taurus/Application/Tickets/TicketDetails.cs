namespace Taurus.Application.Tickets;

public sealed record TicketDetails(
    Guid Id,
    string TicketRef,
    string Title,
    string? Description,
    Guid ProjectId,
    int StatusId,
    int TypeId,
    int PriorityId,
    string? FixedInRelease,
    string? ParentTicketRef,
    Guid? AssignedTo,
    Guid CreatedBy,
    DateTimeOffset CreatedDate,
    Guid LastModifiedBy,
    DateTimeOffset LastModified);