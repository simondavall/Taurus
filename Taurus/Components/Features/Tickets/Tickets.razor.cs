using Microsoft.AspNetCore.Components;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Taurus.Application.UserState;

namespace Taurus.Components.Features.Tickets;

public partial class Tickets
{
    private const int DefaultPageSize = 20;

    // TODO TAU: Replace this temporary hard-coded status rule when
    // "Support status filtering" establishes the Taurus ticket status representation.
    private const int CompletedStatusId = 3;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;
    [Inject]
    private ITicketService TicketService { get; set; } = default!;
    [Inject]
    private IUserStateService UserStateService { get; set; } = default!;

    private IReadOnlyList<Project> ProjectItems { get; set; } = [];
    private IReadOnlyList<Ticket> TicketItems { get; set; } = [];
    private Guid? SelectedProjectId { get; set; }
    private static readonly Guid AllProjectsId = Guid.Empty;
    private Guid SelectedProjectListValue => SelectedProjectId ?? AllProjectsId;

    private string SelectedProjectTitle =>
        SelectedProjectId.HasValue
            ? ProjectItems.FirstOrDefault(project => project.Id == SelectedProjectId.Value)?.Title ?? "All"
            : "All";

    private int PageSize { get; set; }

    private int CurrentPage { get; set; } = 1;

    private int PageCount => Math.Max(1, (int)Math.Ceiling(TicketItems.Count / (double)PageSize));

    private IEnumerable<Ticket> PagedTicketItems =>
        TicketItems
            .OrderByDescending(ticket => ticket.LastModified)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);

    protected override void OnInitialized()
    {
        PageSize = Configuration.GetValue("Tickets:PageSize", DefaultPageSize);

        if (PageSize <= 0)
        {
            PageSize = DefaultPageSize;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await LoadProjectsAsync();
        await RestoreSelectedProjectAsync();
        await LoadTicketsAsync();

        StateHasChanged();
    }

    private async Task LoadProjectsAsync()
    {
        var projects = await ProjectService.GetProjectsAsync();

        ProjectItems = projects
            .Where(project => project.IsActive)
            .OrderBy(project => project.Title)
            .ToArray();
    }

    private async Task RestoreSelectedProjectAsync()
    {
        var storedProjectId = await UserStateService.GetSelectedProjectIdAsync();

        if (!storedProjectId.HasValue)
        {
            SelectedProjectId = null;
            return;
        }

        if (ProjectItems.Any(project => project.Id == storedProjectId.Value))
        {
            SelectedProjectId = storedProjectId;
            return;
        }

        SelectedProjectId = null;
        await UserStateService.SetSelectedProjectIdAsync(null);
    }

    private async Task SelectedProjectChangedAsync(Guid projectId)
    {
        SelectedProjectId = projectId == AllProjectsId ? null : projectId;

        CurrentPage = 1;

        await UserStateService.SetSelectedProjectIdAsync(SelectedProjectId);
        await LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        TicketItems = await TicketService.GetTicketsAsync(SelectedProjectId);

        if (CurrentPage > PageCount)
        {
            CurrentPage = PageCount;
        }
    }

    private static string GetTicketClass(Ticket ticket)
    {
        return ticket.StatusId == CompletedStatusId
            ? "ticket-row ticket-completed"
            : "ticket-row";
    }

    private static string GetMobileTicketClass(Ticket ticket)
    {
        return ticket.StatusId == CompletedStatusId
            ? "mobile-ticket ticket-completed"
            : "mobile-ticket";
    }

    private static string FormatLastUpdated(DateTimeOffset lastModified)
    {
        var elapsed = DateTimeOffset.Now - lastModified.ToLocalTime();

        if (elapsed < TimeSpan.Zero)
        {
            return "just now";
        }

        if (elapsed.TotalMinutes < 1)
        {
            return "just now";
        }

        if (elapsed.TotalHours < 1)
        {
            var minutes = Math.Max(1, (int)elapsed.TotalMinutes);
            return $"{minutes} min{(minutes == 1 ? string.Empty : "s")} ago";
        }

        if (elapsed.TotalDays < 1)
        {
            var hours = Math.Max(1, (int)elapsed.TotalHours);
            return $"{hours} hr{(hours == 1 ? string.Empty : "s")} ago";
        }

        if (elapsed.TotalDays < 7)
        {
            var days = Math.Max(1, (int)elapsed.TotalDays);
            return $"{days} day{(days == 1 ? string.Empty : "s")} ago";
        }

        return lastModified.ToLocalTime().ToString("dd MMM yyyy");
    }
}