namespace Taurus.Components.Features.Tickets;

public sealed class TicketCreateEditorModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TypeId { get; set; }
    public int PriorityId { get; set; }
    public int StatusId { get; set; }
}