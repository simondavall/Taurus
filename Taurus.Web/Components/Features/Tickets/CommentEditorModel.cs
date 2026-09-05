namespace Taurus.Components.Features.Tickets;

public sealed class CommentEditorModel
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public DateTimeOffset LastModified { get; init; }

    public string OriginalContent { get; init; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public bool IsEditing { get; set; }
}