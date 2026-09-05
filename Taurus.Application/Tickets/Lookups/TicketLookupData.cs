namespace Taurus.Application.Tickets.Lookups;

public sealed record TicketStatus(
    int Id,
    string Code,
    string Title,
    int DisplayOrder);

public sealed record TicketPriority(
    int Id,
    string Code,
    string Title,
    int DisplayOrder);

public sealed record TicketType(
    int Id,
    string Code,
    string Title,
    int DisplayOrder);