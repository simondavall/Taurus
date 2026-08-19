namespace Taurus.Components.Features.Tickets;

public sealed class TicketEditorModel
{
    public Guid Id { get; set; }
    public string TicketRef { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public int StatusId { get; set; }
    public int TypeId { get; set; }
    public int PriorityId { get; set; }
    public string? FixedInRelease { get; set; }
    public string? ParentTicketRef { get; set; }
    public Guid? AssignedTo { get; set; }
}