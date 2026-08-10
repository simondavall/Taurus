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

    private string SortField { get; set; } = nameof(Project.Title);

    private bool SortDescending { get; set; }

    private int PageSize { get; set; }

    private int CurrentPage { get; set; } = 1;

    private int PageCount => Math.Max(1, (int)Math.Ceiling(ProjectItems.Count / (double)PageSize));

    private IEnumerable<Project> PagedProjectItems
    {
        get
        {
            var sortedProjects = SortField switch
            {
                nameof(Project.Id) => Sort(ProjectItems, project => project.Id),
                nameof(Project.Prefix) => Sort(ProjectItems, project => project.Prefix),
                nameof(Project.IsActive) => Sort(ProjectItems, project => project.IsActive),
                _ => Sort(ProjectItems, project => project.Title)
            };

            return sortedProjects
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        PageSize = Configuration.GetValue("Projects:PageSize", DefaultPageSize);

        if (PageSize <= 0)
        {
            PageSize = DefaultPageSize;
        }

        ProjectItems = await ProjectService.GetProjectsAsync();
    }

    private IEnumerable<Project> Sort<TKey>(IEnumerable<Project> projects, Func<Project, TKey> selector)
    {
        return SortDescending
            ? projects.OrderByDescending(selector)
            : projects.OrderBy(selector);
    }

    private void SortBy(string field)
    {
        if (SortField == field)
        {
            SortDescending = !SortDescending;
        }
        else
        {
            SortField = field;
            SortDescending = false;
        }

        CurrentPage = 1;
    }

    private void SortFieldChanged(string field)
    {
        SortField = field;
        SortDescending = false;
        CurrentPage = 1;
    }

    private void ToggleSortDirection()
    {
        SortDescending = !SortDescending;
        CurrentPage = 1;
    }

    private static string DisplayId(Guid id)
    {
        return id.ToString("N")[..6];
    }
}