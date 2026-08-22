using Taurus.Application.Tickets;

namespace Taurus.Components.Features.Tickets;

public static class TicketPresentation
{
    public static bool IsInactive(Ticket ticket, TicketReferenceIds referenceIds)
    {
        return ticket.StatusId == referenceIds.CompletedStatusId
               || ticket.StatusId == referenceIds.ObsoleteStatusId;
    }

    public static string FormatAge(DateTimeOffset lastModified)
    {
        var elapsed = DateTimeOffset.UtcNow - lastModified.ToUniversalTime();

        if (elapsed < TimeSpan.Zero || elapsed.TotalMinutes < 1)
        {
            return "just now";
        }

        if (elapsed.TotalHours < 1)
        {
            var minutes = Math.Max(1, (int)elapsed.TotalMinutes);
            return $"{minutes} min{(minutes == 1 ? string.Empty : "s")} ago";
        }

        if (elapsed.TotalDays < 1)
        {
            var hours = Math.Max(1, (int)elapsed.TotalHours);
            return $"{hours} hr{(hours == 1 ? string.Empty : "s")} ago";
        }

        if (elapsed.TotalDays < 7)
        {
            var days = Math.Max(1, (int)elapsed.TotalDays);
            return $"{days} day{(days == 1 ? string.Empty : "s")} ago";
        }

        return lastModified.ToLocalTime().ToString("dd MMM yyyy");
    }
}