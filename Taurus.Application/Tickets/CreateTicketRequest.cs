namespace Taurus.Application.Tickets;

public sealed record CreateTicketRequest(
    string Title,
    string? Description,
    Guid ProjectId,
    int StatusId,
    int TypeId,
    int PriorityId,
    string? FixedInRelease,
    string? ParentTicketRef = null);