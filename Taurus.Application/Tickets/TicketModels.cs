namespace Taurus.Application.Tickets;

public sealed record Ticket(
    Guid Id,
    string TicketRef,
    string Title,
    int StatusId,
    int TypeId,
    int PriorityId,
    DateTimeOffset LastModified);
    
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
    
public sealed record CreateTicketRequest(
    string Title,
    string? Description,
    Guid ProjectId,
    int StatusId,
    int TypeId,
    int PriorityId,
    string? FixedInRelease,
    string? ParentTicketRef = null);

public sealed record UpdateTicketRequest(
    Guid Id,
    string Title,
    string? Description,
    Guid ProjectId,
    int StatusId,
    int TypeId,
    int PriorityId,
    string? FixedInRelease,
    string? ParentTicketRef,
    Guid? AssignedTo);