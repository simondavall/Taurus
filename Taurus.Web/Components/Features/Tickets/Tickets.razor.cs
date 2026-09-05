using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Taurus.Application.Tickets.Lookups;
using Taurus.UserState;

namespace Taurus.Components.Features.Tickets;

public partial class Tickets
{
    private const int DefaultPageSize = 20;

    private static readonly Guid AllProjectsId = Guid.Empty;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;
    [Inject]
    private ITicketService TicketService { get; set; } = default!;
    [Inject]
    private ITicketLookupDataService TicketLookupDataService { get; set; } = default!;
    [Inject]
    private IUserStateService UserStateService { get; set; } = default!;
    [Inject]
    private IDialogService DialogService { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private IReadOnlyList<Project> ProjectItems { get; set; } = [];
    private IReadOnlyList<Ticket> TicketItems { get; set; } = [];
    private IReadOnlyList<TicketStatus> TicketStatuses { get; set; } = [];
    private IReadOnlyList<TicketPriority> TicketPriorities { get; set; } = [];
    private IReadOnlyList<TicketType> TicketTypes { get; set; } = [];
    private Guid? SelectedProjectId { get; set; }
    private Guid SelectedProjectListValue => SelectedProjectId ?? AllProjectsId;
    private TicketFilter SelectedTicketFilter { get; set; } = TicketFilter.Open;

    private TicketLookupIds LookupIds { get; set; } = default!;

    private string SelectedProjectTitle =>
        SelectedProjectId.HasValue
            ? ProjectItems.FirstOrDefault(project => project.Id == SelectedProjectId.Value)?.Title ?? "All"
            : "All";
    private Project? SelectedProject =>
        SelectedProjectId.HasValue
            ? ProjectItems.FirstOrDefault(project => project.Id == SelectedProjectId.Value)
            : null;
    private bool CanCreateTicket => SelectedProject is not null;
    
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
        await LoadTicketLookupDataAsync();
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

    private async Task LoadTicketLookupDataAsync()
    {
        TicketStatuses = await TicketLookupDataService.GetStatusesAsync();
        TicketPriorities = await TicketLookupDataService.GetPrioritiesAsync();
        TicketTypes = await TicketLookupDataService.GetTypesAsync();

        LookupIds = TicketLookupIds.Resolve(TicketStatuses, TicketPriorities);
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

    private async Task CreateTicketAsync()
    {
        var project = SelectedProject;
        if (project is null)
        {
            return;
        }

        var parameters = new DialogParameters
        {
            [nameof(TicketCreateDialog.Project)] = project,
            [nameof(TicketCreateDialog.TicketTypes)] = TicketTypes,
            [nameof(TicketCreateDialog.TicketPriorities)] = TicketPriorities,
            [nameof(TicketCreateDialog.TicketStatuses)] = TicketStatuses
        };

        var dialog = await DialogService.ShowAsync<TicketCreateDialog>(
            $"Create Ticket — {project.Title}",
            parameters,
            CreateTicketDialogOptions());

        var result = await dialog.Result;

        if (result is null ||
            result.Canceled ||
            result.Data is not Application.Tickets.TicketDetails ticket)
        {
            return;
        }

        Snackbar.Add($"Ticket {ticket.TicketRef} created successfully.", Severity.Success);
        NavigationManager.NavigateTo($"/tickets/{Uri.EscapeDataString(ticket.TicketRef)}");
    }

    private static DialogOptions CreateTicketDialogOptions()
    {
        return new DialogOptions
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Small,
            CloseOnEscapeKey = true
        };
    }
    
    private async Task LoadTicketsAsync()
    {
        TicketItems = await TicketService.GetTicketsAsync(SelectedProjectId);

        if (CurrentPage > PageCount)
        {
            CurrentPage = PageCount;
        }
    }

    private void OpenTicket(Ticket ticket)
    {
        NavigationManager.NavigateTo(
            $"/tickets/{Uri.EscapeDataString(ticket.TicketRef)}");
    }

    private void TicketKeyDown(KeyboardEventArgs args, Ticket ticket)
    {
        if (args.Key is "Enter" or " ")
        {
            OpenTicket(ticket);
        }
    }
    
    private IEnumerable<Ticket> ApplyTicketFilter(IEnumerable<Ticket> tickets)
    {
        return SelectedTicketFilter switch
        {
            TicketFilter.Open => tickets.Where(ticket =>
                ticket.StatusId != LookupIds.CompletedStatusId &&
                ticket.StatusId != LookupIds.ObsoleteStatusId),

            TicketFilter.Backlog => tickets.Where(ticket =>
                ticket.StatusId == LookupIds.BacklogStatusId),

            TicketFilter.HighPriority => tickets.Where(ticket =>
                ticket.PriorityId == LookupIds.HighPriorityId ||
                ticket.PriorityId == LookupIds.CriticalPriorityId),

            TicketFilter.Obsolete => tickets.Where(ticket =>
                ticket.StatusId == LookupIds.ObsoleteStatusId),

            _ => tickets
        };
    }

    private string GetTicketClass(Ticket ticket)
    {
        return TicketPresentation.IsInactive(ticket, LookupIds)
            ? "ticket-row ticket-inactive"
            : "ticket-row";
    }

    private string GetMobileTicketClass(Ticket ticket)
    {
        return TicketPresentation.IsInactive(ticket, LookupIds)
            ? "mobile-ticket ticket-inactive"
            : "mobile-ticket";
    }

    private static string FormatLastUpdated(DateTimeOffset lastModified)
    {
        return TicketPresentation.FormatAge(lastModified);
    }
}