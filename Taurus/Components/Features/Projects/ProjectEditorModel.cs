namespace Taurus.Components.Features.Projects;

public sealed class ProjectEditorModel
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}