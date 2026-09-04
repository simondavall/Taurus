namespace Taurus.Application.Tickets.Lookups;

public static class TicketLookupCodes
{
    public static class Status
    {
        public const string Backlog = "backlog";
        public const string Completed = "completed";
        public const string Obsolete = "obsolete";
        public const string InProgress = "in_progress";
        public const string OnHold = "on_hold";
        public const string Submitted = "submitted";
    }

    public static class Priority
    {
        public const string None = "none";
        public const string Low = "low";
        public const string Normal = "normal";
        public const string High = "high";
        public const string Critical = "critical";
    }

    public static class Type
    {
        public const string Task = "task";
        public const string Bug = "bug";
        public const string Feature = "feature";
    }
}