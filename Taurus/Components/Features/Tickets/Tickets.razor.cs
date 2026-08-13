using Microsoft.AspNetCore.Components;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Taurus.Application.UserState;

namespace Taurus.Components.Features.Tickets;

public partial class Tickets
{
    private const int DefaultPageSize = 20;

    private const string CompletedStatusCode = "completed";
    private const string ObsoleteStatusCode = "obsolete";
    private const string BacklogStatusCode = "backlog";
    private const string HighPriorityCode = "high";
    private const string CriticalPriorityCode = "critical";

    private static readonly Guid AllProjectsId = Guid.Empty;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;
    [Inject]
    private ITicketService TicketService { get; set; } = default!;
    [Inject]
    private ITicketReferenceDataService TicketReferenceDataService { get; set; } = default!;
    [Inject]
    private IUserStateService UserStateService { get; set; } = default!;

    private IReadOnlyList<Project> ProjectItems { get; set; } = [];
    private IReadOnlyList<Ticket> TicketItems { get; set; } = [];
    private IReadOnlyList<TicketStatus> TicketStatuses { get; set; } = [];
    private IReadOnlyList<TicketPriority> TicketPriorities { get; set; } = [];
    private IReadOnlyList<TicketType> TicketTypes { get; set; } = [];
    private Guid? SelectedProjectId { get; set; }
    private Guid SelectedProjectListValue => SelectedProjectId ?? AllProjectsId;
    private TicketFilter SelectedTicketFilter { get; set; } = TicketFilter.Open;

    private int CompletedStatusId { get; set; }
    private int ObsoleteStatusId { get; set; }
    private int BacklogStatusId { get; set; }
    private int HighPriorityId { get; set; }
    private int CriticalPriorityId { get; set; }

    private string SelectedProjectTitle =>
        SelectedProjectId.HasValue
            ? ProjectItems.FirstOrDefault(project => project.Id == SelectedProjectId.Value)?.Title ?? "All"
            : "All";

    private int PageSize { get; set; }
    private int CurrentPage { get; set; } = 1;
    private IEnumerable<Ticket> FilteredTicketItems => ApplyTicketFilter(TicketItems);
    private int FilteredTicketCount => FilteredTicketItems.Count();
    private int PageCount => Math.Max(1, (int)Math.Ceiling(FilteredTicketCount / (double)PageSize));
    private IEnumerable<Ticket> PagedTicketItems =>
        FilteredTicketItems
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
        await LoadTicketReferenceDataAsync();
        await RestoreSelectedProjectAsync();
        await RestoreSelectedTicketFilterAsync();
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

    private async Task LoadTicketReferenceDataAsync()
    {
        TicketStatuses = await TicketReferenceDataService.GetStatusesAsync();
        TicketPriorities = await TicketReferenceDataService.GetPrioritiesAsync();
        TicketTypes = await TicketReferenceDataService.GetTypesAsync();

        CompletedStatusId = ResolveRequiredStatusId(CompletedStatusCode);
        ObsoleteStatusId = ResolveRequiredStatusId(ObsoleteStatusCode);
        BacklogStatusId = ResolveRequiredStatusId(BacklogStatusCode);
        HighPriorityId = ResolveRequiredPriorityId(HighPriorityCode);
        CriticalPriorityId = ResolveRequiredPriorityId(CriticalPriorityCode);
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

    private async Task RestoreSelectedTicketFilterAsync()
    {
        var storedFilter = await UserStateService.GetSelectedTicketFilterAsync();
        SelectedTicketFilter = storedFilter ?? TicketFilter.Open;
    }

    private async Task SelectedProjectChangedAsync(Guid projectId)
    {
        SelectedProjectId = projectId == AllProjectsId ? null : projectId;
        CurrentPage = 1;

        await UserStateService.SetSelectedProjectIdAsync(SelectedProjectId);
        await LoadTicketsAsync();
    }

    private async Task SelectedTicketFilterChangedAsync(TicketFilter filter)
    {
        SelectedTicketFilter = filter;
        CurrentPage = 1;

        await UserStateService.SetSelectedTicketFilterAsync(filter);
    }

    private async Task LoadTicketsAsync()
    {
        TicketItems = await TicketService.GetTicketsAsync(SelectedProjectId);

        if (CurrentPage > PageCount)
        {
            CurrentPage = PageCount;
        }
    }

    private IEnumerable<Ticket> ApplyTicketFilter(IEnumerable<Ticket> tickets)
    {
        return SelectedTicketFilter switch
        {
            TicketFilter.Open => tickets.Where(ticket =>
                ticket.StatusId != CompletedStatusId &&
                ticket.StatusId != ObsoleteStatusId),

            TicketFilter.Backlog => tickets.Where(ticket =>
                ticket.StatusId == BacklogStatusId),

            TicketFilter.HighPriority => tickets.Where(ticket =>
                ticket.PriorityId == HighPriorityId ||
                ticket.PriorityId == CriticalPriorityId),

            TicketFilter.Obsolete => tickets.Where(ticket =>
                ticket.StatusId == ObsoleteStatusId),

            _ => tickets
        };
    }

    private int ResolveRequiredStatusId(string code)
    {
        var status = TicketStatuses.FirstOrDefault(status =>
            string.Equals(status.Code, code, StringComparison.OrdinalIgnoreCase));

        if (status is null)
        {
            throw new InvalidOperationException(
                $"PegasusApi ticket status '{code}' is required by Taurus but was not returned.");
        }

        return status.Id;
    }

    private int ResolveRequiredPriorityId(string code)
    {
        var priority = TicketPriorities.FirstOrDefault(priority =>
            string.Equals(priority.Code, code, StringComparison.OrdinalIgnoreCase));

        if (priority is null)
        {
            throw new InvalidOperationException(
                $"PegasusApi ticket priority '{code}' is required by Taurus but was not returned.");
        }

        return priority.Id;
    }

    private string GetTicketClass(Ticket ticket)
    {
        return IsInactiveTicket(ticket)
            ? "ticket-row ticket-inactive"
            : "ticket-row";
    }

    private string GetMobileTicketClass(Ticket ticket)
    {
        return IsInactiveTicket(ticket)
            ? "mobile-ticket ticket-inactive"
            : "mobile-ticket";
    }

    private bool IsInactiveTicket(Ticket ticket)
    {
        return ticket.StatusId == CompletedStatusId
               || ticket.StatusId == ObsoleteStatusId;
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