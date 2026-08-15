using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Severity = MudBlazor.Severity;

namespace Taurus.Components.Features.Tickets;

public partial class TicketDetails
{
    [Parameter]
    public string TicketRef { get; set; } = string.Empty;

    [Inject]
    private ITicketService TicketService { get; set; } = default!;
    [Inject]
    private ITicketReferenceDataService TicketReferenceDataService { get; set; } = default!;
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private readonly TicketEditorValidator _validator = new();

    private MudForm? _form;
    private TicketEditorModel? Editor { get; set; }

    private IReadOnlyList<Project> Projects { get; set; } = [];
    private IReadOnlyList<TicketStatus> TicketStatuses { get; set; } = [];
    private IReadOnlyList<TicketPriority> TicketPriorities { get; set; } = [];
    private IReadOnlyList<TicketType> TicketTypes { get; set; } = [];

    private bool _loading;
    private bool _saving;
    private bool _editingDescription;
    private string? _loadError;
    private string? _updateError;
    private string? _titleError;
    
    private string ProjectTitle =>
        Editor is null
            ? string.Empty
            : Projects.FirstOrDefault(project => project.Id == Editor.ProjectId)?.Title
              ?? "Unknown project";

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        _loadError = null;
        _updateError = null;
        _titleError = null;
        _editingDescription = false;

        try
        {
            await LoadPageDataAsync();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task LoadPageDataAsync()
    {
        var projectsTask = ProjectService.GetProjectsAsync();
        var statusesTask = TicketReferenceDataService.GetStatusesAsync();
        var prioritiesTask = TicketReferenceDataService.GetPrioritiesAsync();
        var typesTask = TicketReferenceDataService.GetTypesAsync();
        var ticketTask = TicketService.GetTicketByRefAsync(TicketRef);

        await Task.WhenAll(
            projectsTask,
            statusesTask,
            prioritiesTask,
            typesTask,
            ticketTask);

        Projects = await projectsTask;
        TicketStatuses = await statusesTask;
        TicketPriorities = await prioritiesTask;
        TicketTypes = await typesTask;

        var ticketResult = await ticketTask;
        if (!ticketResult.Succeeded || ticketResult.Value is null)
        {
            Editor = null;
            _loadError = ticketResult.ErrorMessage ?? "The ticket could not be loaded.";
            return;
        }

        SetEditor(ticketResult.Value);
    }

    private async Task ReloadTicketAsync()
    {
        var ticketResult = await TicketService.GetTicketByRefAsync(TicketRef);
        if (!ticketResult.Succeeded || ticketResult.Value is null)
        {
            Editor = null;
            _loadError = ticketResult.ErrorMessage ?? "The ticket could not be reloaded.";
            return;
        }

        SetEditor(ticketResult.Value);
    }

    private void SetEditor(Application.Tickets.TicketDetails ticket)
    {
        Editor = new TicketEditorModel
        {
            Id = ticket.Id,
            TicketRef = ticket.TicketRef,
            Title = ticket.Title,
            Description = ticket.Description,
            ProjectId = ticket.ProjectId,
            StatusId = ticket.StatusId,
            TypeId = ticket.TypeId,
            PriorityId = ticket.PriorityId,
            FixedInRelease = ticket.FixedInRelease,
            ParentTicketId = ticket.ParentTicketId,
            AssignedTo = ticket.AssignedTo
        };
    }
    
    private void BeginDescriptionEdit()
    {
        if (_saving)
        {
            return;
        }

        _editingDescription = true;
    }
    
    private void TitleChanged(string? value)
    {
        if (Editor is null)
        {
            return;
        }

        Editor.Title = value ?? string.Empty;
        _titleError = null;
    }

    private async Task UpdateAsync()
    {
        if (Editor is null || _saving)
        {
            return;
        }

        _updateError = null;

        if (!await ValidateAsync())
        {
            return;
        }

        var userId = await GetCurrentUserIdAsync();

        var request = new UpdateTicketRequest(
            Editor.Id,
            Editor.Title.Trim(),
            Editor.Description,
            Editor.ProjectId,
            Editor.StatusId,
            Editor.TypeId,
            Editor.PriorityId,
            Editor.FixedInRelease,
            Editor.ParentTicketId,
            Editor.AssignedTo);

        _saving = true;

        try
        {
            var result = await TicketService.UpdateTicketAsync(request, userId);

            if (!result.Succeeded)
            {
                _updateError = result.ErrorMessage;
                return;
            }

            await ReloadTicketAsync();

            if (Editor is not null)
            {
                _editingDescription = false;

                Snackbar.Add(
                    $"Ticket {Editor.TicketRef} updated successfully.",
                    Severity.Success);
            }
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task<bool> ValidateAsync()
    {
        if (Editor is null)
        {
            return false;
        }

        var validationResult = await _validator.ValidateAsync(Editor);

        _titleError = validationResult.Errors
            .FirstOrDefault(error => error.PropertyName == nameof(TicketEditorModel.Title))
            ?.ErrorMessage;

        return validationResult.IsValid;
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        var subject = authenticationState.User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out var userId))
        {
            throw new InvalidOperationException(
                "The authenticated Soteria principal does not contain a valid 'sub' user identifier.");
        }

        return userId;
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo("/tickets");
    }
}