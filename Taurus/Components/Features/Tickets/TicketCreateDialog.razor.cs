using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;

namespace Taurus.Components.Features.Tickets;

public partial class TicketCreateDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private ITicketService TicketService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Parameter, EditorRequired]
    public Project Project { get; set; } = default!;

    [Parameter, EditorRequired]
    public IReadOnlyList<TicketType> TicketTypes { get; set; } = [];

    [Parameter, EditorRequired]
    public IReadOnlyList<TicketPriority> TicketPriorities { get; set; } = [];

    [Parameter, EditorRequired]
    public IReadOnlyList<TicketStatus> TicketStatuses { get; set; } = [];

    private readonly TicketCreateEditorValidator _validator = new();

    private MudForm _form = default!;

    private TicketCreateEditorModel Model { get; } = new();

    private bool _saving;

    private string? _errorMessage;

    protected override void OnInitialized()
    {
        if (TicketTypes.Count == 0)
        {
            throw new InvalidOperationException(
                "Ticket type reference data is required when creating a ticket.");
        }

        if (TicketPriorities.Count == 0)
        {
            throw new InvalidOperationException(
                "Ticket priority reference data is required when creating a ticket.");
        }

        if (TicketStatuses.Count == 0)
        {
            throw new InvalidOperationException(
                "Ticket status reference data is required when creating a ticket.");
        }

        Model.TypeId = TicketTypes[0].Id;
        Model.PriorityId = TicketPriorities[0].Id;
        Model.StatusId = TicketStatuses[0].Id;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task CreateAsync()
    {
        _errorMessage = null;

        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            return;
        }

        var userId = await GetCurrentUserIdAsync();

        _saving = true;

        try
        {
            var request = new CreateTicketRequest(
                Model.Title.Trim(),
                Model.Description,
                Project.Id,
                Model.StatusId,
                Model.TypeId,
                Model.PriorityId);

            var result = await TicketService.CreateTicketAsync(request, userId);

            if (!result.Succeeded || result.Value is null)
            {
                _errorMessage = result.ErrorMessage;
                return;
            }

            MudDialog.Close(DialogResult.Ok(result.Value));
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        var authenticationState =
            await AuthenticationStateProvider.GetAuthenticationStateAsync();

        var subject = authenticationState.User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out var userId))
        {
            throw new InvalidOperationException(
                "The authenticated Soteria principal does not contain a valid 'sub' user identifier.");
        }

        return userId;
    }
}