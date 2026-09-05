using Taurus.Application.Tickets.Lookups;

namespace Taurus.Components.Features.Tickets;

public sealed record TicketLookupIds(
    int BacklogStatusId,
    int CompletedStatusId,
    int ObsoleteStatusId,
    int InProgressStatusId,
    int OnHoldStatusId,
    int HighPriorityId,
    int CriticalPriorityId)
{
    public static TicketLookupIds Resolve(IReadOnlyList<TicketStatus> statuses, IReadOnlyList<TicketPriority> priorities)
    {
        return new TicketLookupIds(
            ResolveStatusId(statuses, TicketLookupCodes.Status.Backlog),
            ResolveStatusId(statuses, TicketLookupCodes.Status.Completed),
            ResolveStatusId(statuses, TicketLookupCodes.Status.Obsolete),
            ResolveStatusId(statuses, TicketLookupCodes.Status.InProgress),
            ResolveStatusId(statuses, TicketLookupCodes.Status.OnHold),
            ResolvePriorityId(priorities, TicketLookupCodes.Priority.High),
            ResolvePriorityId(priorities, TicketLookupCodes.Priority.Critical));
    }

    private static int ResolveStatusId(IReadOnlyList<TicketStatus> statuses, string code)
    {
        var status = statuses.FirstOrDefault(status => string.Equals(status.Code, code, StringComparison.OrdinalIgnoreCase));
        if (status is null)
            throw new InvalidOperationException(
                $"PegasusApi ticket status '{code}' is required by Taurus but was not returned.");

        return status.Id;
    }

    private static int ResolvePriorityId(IReadOnlyList<TicketPriority> priorities, string code)
    {
        var priority = priorities.FirstOrDefault(priority => string.Equals(priority.Code, code, StringComparison.OrdinalIgnoreCase));
        if (priority is null)
            throw new InvalidOperationException(
                $"PegasusApi ticket priority '{code}' is required by Taurus but was not returned.");

        return priority.Id;
    }
}