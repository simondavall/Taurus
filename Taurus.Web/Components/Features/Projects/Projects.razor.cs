using Microsoft.AspNetCore.Components;
using MudBlazor;
using Taurus.Application.Configuration;
using Taurus.Application.Projects;
using Taurus.UserState;

namespace Taurus.Components.Features.Projects;

public partial class Projects
{
    [Inject]
    private ProjectSettings Settings { get; set; } = default!;
    [Inject]
    private IDialogService DialogService { get; set; } = default!;
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;
    [Inject]
    private IUserStateService UserStateService { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private IReadOnlyList<Project> ProjectItems { get; set; } = [];
    private bool ShowActiveOnly { get; set; } = true;

    private IEnumerable<Project> FilteredProjectItems => ShowActiveOnly
        ? ProjectItems.Where(project => project.IsActive)
        : ProjectItems;

    private string SortField { get; set; } = nameof(Project.Title);
    private bool SortDescending { get; set; }
    private int PageSize { get; set; }
    private int CurrentPage { get; set; } = 1;
    private int PageCount => Math.Max(1, (int)Math.Ceiling(FilteredProjectItems.Count() / (double)PageSize));

    private IEnumerable<Project> PagedProjectItems {
        get {
            var projects = FilteredProjectItems;

            var sortedProjects = SortField switch {
                nameof(Project.Id) => Sort(projects, project => project.Id),
                nameof(Project.Prefix) => Sort(projects, project => project.Prefix),
                nameof(Project.LatestVersion) => Sort(projects, project => project.LatestVersion),
                nameof(Project.RequireFixedInRelease) => Sort(projects, project => project.RequireFixedInRelease),
                nameof(Project.IsActive) => Sort(projects, project => project.IsActive),
                _ => Sort(projects, project => project.Title)
            };

            return sortedProjects
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        PageSize = Settings.PageSize;
        await LoadProjectsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        ShowActiveOnly = await UserStateService.GetShowActiveProjectsOnlyAsync() ?? true;
        CurrentPage = 1;

        StateHasChanged();
    }

    private async Task CreateProjectAsync()
    {
        var dialog = await DialogService.ShowAsync<ProjectDialog>("Create Project", CreateDialogOptions());
        var result = await dialog.Result;
        if (result is null || result.Canceled || result.Data is not Project)
            return;

        await LoadProjectsAsync();

        Snackbar.Add("Project created successfully.", Severity.Success);
    }

    private async Task EditProjectAsync(Project project)
    {
        var parameters = new DialogParameters { [nameof(ProjectDialog.ProjectToEdit)] = project };

        var dialog = await DialogService.ShowAsync<ProjectDialog>("Edit Project", parameters, CreateDialogOptions());
        var result = await dialog.Result;
        if (result is null || result.Canceled || result.Data is not ProjectDialogResult dialogResult)
            return;

        await LoadProjectsAsync();

        var message = dialogResult switch {
            ProjectDialogResult.Deleted => "Project deleted successfully.",
            _ => "Project updated successfully."
        };

        Snackbar.Add(message, Severity.Success);
    }

    private static DialogOptions CreateDialogOptions()
    {
        return new DialogOptions { FullWidth = true, MaxWidth = MaxWidth.Small, BackdropClick = false, CloseOnEscapeKey = true };
    }

    private async Task LoadProjectsAsync()
    {
        ProjectItems = await ProjectService.GetProjectsAsync();

        if (CurrentPage > PageCount)
            CurrentPage = PageCount;
    }

    private Task ProjectRowClickedAsync(TableRowClickEventArgs<Project> args)
    {
        return EditProjectAsync(args.Item!);
    }

    private async Task ShowActiveOnlyChangedAsync(bool value)
    {
        ShowActiveOnly = value;
        CurrentPage = 1;

        await UserStateService.SetShowActiveProjectsOnlyAsync(value);
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
            SortDescending = !SortDescending;
        else {
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