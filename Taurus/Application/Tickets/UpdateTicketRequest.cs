namespace Taurus.Application.Tickets;

public sealed record UpdateTicketRequest(
    Guid Id,
    string Title,
    string? Description,
    Guid ProjectId,
    int StatusId,
    int TypeId,
    int PriorityId,
    string? FixedInRelease,
    Guid? ParentTicketId,
    Guid? AssignedTo);