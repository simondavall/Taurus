using Microsoft.AspNetCore.Components;
using Taurus.Application.Projects;

namespace Taurus.Components.Features.Projects;

public partial class Projects
{
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    private IReadOnlyList<Project> ProjectItems { get; set; } = [];

    private bool ActiveOnly { get; set; }
    private bool IncludeDeleted { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadProjectsAsync();
    }

    private async Task LoadProjectsAsync()
    {
        ProjectItems = await ProjectService.GetProjectsAsync(ActiveOnly, IncludeDeleted);
    }

    private async Task ActiveOnlyChangedAsync(bool value)
    {
        ActiveOnly = value;
        await LoadProjectsAsync();
    }

    private async Task IncludeDeletedChangedAsync(bool value)
    {
        IncludeDeleted = value;
        await LoadProjectsAsync();
    }
}