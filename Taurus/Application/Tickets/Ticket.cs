namespace Taurus.Application.Tickets;

public sealed record Ticket(
    Guid Id,
    string TicketRef,
    string Title,
    int StatusId,
    int TypeId,
    int PriorityId,
    DateTimeOffset LastModified);