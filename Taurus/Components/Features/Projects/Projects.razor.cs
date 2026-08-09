using Microsoft.AspNetCore.Components;
using Taurus.Application.Projects;

namespace Taurus.Components.Features.Projects;

public partial class Projects
{
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    private IReadOnlyList<Project> ProjectItems { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        ProjectItems = await ProjectService.GetProjectsAsync();
    }
}