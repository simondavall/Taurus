namespace Taurus.Application.Tickets;

public static class TicketReferenceCodes
{
    public static class Status
    {
        public const string Backlog = "backlog";
        public const string Completed = "completed";
        public const string Obsolete = "obsolete";
        public const string InProgress = "in_progress";
        public const string OnHold = "on_hold";
    }

    public static class Priority
    {
        public const string High = "high";
        public const string Critical = "critical";
    }

    public static class Type
    {
    }
}