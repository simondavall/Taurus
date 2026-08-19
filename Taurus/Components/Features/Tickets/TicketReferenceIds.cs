using Taurus.Application.Tickets;

namespace Taurus.Components.Features.Tickets;

public sealed record TicketReferenceIds(
    int BacklogStatusId,
    int CompletedStatusId,
    int ObsoleteStatusId,
    int InProgressStatusId,
    int OnHoldStatusId,
    int HighPriorityId,
    int CriticalPriorityId)
{
    public static TicketReferenceIds Resolve(IReadOnlyList<TicketStatus> statuses, IReadOnlyList<TicketPriority> priorities)
    {
        return new TicketReferenceIds(
            ResolveStatusId(statuses, TicketReferenceCodes.Status.Backlog),
            ResolveStatusId(statuses, TicketReferenceCodes.Status.Completed),
            ResolveStatusId(statuses, TicketReferenceCodes.Status.Obsolete),
            ResolveStatusId(statuses, TicketReferenceCodes.Status.InProgress),
            ResolveStatusId(statuses, TicketReferenceCodes.Status.OnHold),
            ResolvePriorityId(priorities, TicketReferenceCodes.Priority.High),
            ResolvePriorityId(priorities, TicketReferenceCodes.Priority.Critical));
    }

    private static int ResolveStatusId(IReadOnlyList<TicketStatus> statuses, string code)
    {
        var status = statuses.FirstOrDefault(status => string.Equals(status.Code, code, StringComparison.OrdinalIgnoreCase));
        if (status is null)
        {
            throw new InvalidOperationException(
                $"PegasusApi ticket status '{code}' is required by Taurus but was not returned.");
        }

        return status.Id;
    }

    private static int ResolvePriorityId(IReadOnlyList<TicketPriority> priorities, string code)
    {
        var priority = priorities.FirstOrDefault(priority => string.Equals(priority.Code, code, StringComparison.OrdinalIgnoreCase));
        if (priority is null)
        {
            throw new InvalidOperationException(
                $"PegasusApi ticket priority '{code}' is required by Taurus but was not returned.");
        }

        return priority.Id;
    }
}