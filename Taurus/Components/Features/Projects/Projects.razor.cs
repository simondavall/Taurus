using Microsoft.AspNetCore.Components;
using Taurus.Application.Projects;

namespace Taurus.Components.Features.Projects;

public partial class Projects
{
    private const int DefaultPageSize = 10;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    private IReadOnlyList<Project> ProjectItems { get; set; } = [];

    private int PageSize { get; set; }

    protected override async Task OnInitializedAsync()
    {
        PageSize = Configuration.GetValue("Projects:PageSize", DefaultPageSize);

        if (PageSize <= 0)
        {
            PageSize = DefaultPageSize;
        }

        ProjectItems = await ProjectService.GetProjectsAsync();
    }

    private static string DisplayId(Guid id)
    {
        return id.ToString("N")[..6];
    }
}