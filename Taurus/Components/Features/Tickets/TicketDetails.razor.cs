using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Taurus.Application;
using Taurus.Application.Markdown;
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
    private ITicketCommentService TicketCommentService { get; set; } = default!;
    [Inject]
    private ITicketReferenceDataService TicketReferenceDataService { get; set; } = default!;
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;
    [Inject]
    private IMarkdownRenderer MarkdownRenderer { get; set; } = default!;
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
    private List<CommentEditorModel> Comments { get; set; } = [];

    private bool _loading;
    private bool _saving;
    private string? _loadError;
    private string? _updateError;
    private string? _titleError;
    private string? NewComment { get; set; }

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
        NewComment = null;

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
            Comments = [];
            _loadError = ticketResult.ErrorMessage ?? "The ticket could not be loaded.";
            return;
        }

        SetEditor(ticketResult.Value);
        await LoadCommentsAsync(ticketResult.Value.Id);
    }

    private async Task ReloadPageDataAsync()
    {
        var ticketResult = await TicketService.GetTicketByRefAsync(TicketRef);
        if (!ticketResult.Succeeded || ticketResult.Value is null)
        {
            Editor = null;
            Comments = [];
            _loadError = ticketResult.ErrorMessage ?? "The ticket could not be reloaded.";
            return;
        }

        SetEditor(ticketResult.Value);
        await LoadCommentsAsync(ticketResult.Value.Id);
        NewComment = null;
    }

    private async Task LoadCommentsAsync(Guid ticketId)
    {
        var comments = await TicketCommentService.GetCommentsAsync(ticketId);

        Comments = comments
            .OrderByDescending(comment => comment.LastModified)
            .Select(comment => new CommentEditorModel
            {
                Id = comment.Id,
                DisplayName = comment.DisplayName,
                LastModified = comment.LastModified,
                OriginalContent = comment.Content,
                Content = comment.Content,
                IsDeleted = comment.IsDeleted
            })
            .ToList();
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

    private void TitleChanged(string? value)
    {
        if (Editor is null)
        {
            return;
        }

        Editor.Title = value ?? string.Empty;
        _titleError = null;
    }

    private void EditComment(CommentEditorModel comment)
    {
        if (_saving || comment.IsDeleted)
        {
            return;
        }

        comment.IsEditing = true;
    }

    private void UndoEditComment(CommentEditorModel comment)
    {
        if (_saving)
        {
            return;
        }

        comment.Content = comment.OriginalContent;
        comment.IsEditing = false;
    }

    private void DeleteComment(CommentEditorModel comment)
    {
        if (_saving)
        {
            return;
        }

        comment.Content = comment.OriginalContent;
        comment.IsEditing = false;
        comment.IsDeleted = true;
    }

    private void UndoDeleteComment(CommentEditorModel comment)
    {
        if (_saving)
        {
            return;
        }

        comment.Content = comment.OriginalContent;
        comment.IsDeleted = false;
        comment.IsEditing = false;
    }

    private string RenderComment(string content)
    {
        return MarkdownRenderer.Render(content);
    }

    private static string GetCommentClass(CommentEditorModel comment)
    {
        return comment.IsDeleted
            ? "ticket-comment ticket-comment-deleted"
            : "ticket-comment";
    }

    private static string FormatCommentAge(DateTimeOffset lastModified)
    {
        var elapsed = DateTimeOffset.UtcNow - lastModified.ToUniversalTime();

        if (elapsed < TimeSpan.Zero || elapsed.TotalMinutes < 1)
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

        _saving = true;

        try
        {
            var ticketResult = await UpdateTicketAsync(userId);
            if (!ticketResult.Succeeded)
            {
                _updateError = ticketResult.ErrorMessage;
                return;
            }

            if (Comments.Count > 0)
            {
                var commentsResult = await UpdateCommentsAsync();
                if (!commentsResult.Succeeded)
                {
                    _updateError = commentsResult.ErrorMessage;
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(NewComment))
            {
                var createCommentResult = await CreateCommentAsync(userId);
                if (!createCommentResult.Succeeded)
                {
                    _updateError = createCommentResult.ErrorMessage;
                    return;
                }
            }

            await ReloadPageDataAsync();

            if (Editor is not null)
            {
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

    private Task<ApplicationResult> UpdateTicketAsync(Guid userId)
    {
        var request = new UpdateTicketRequest(
            Editor!.Id,
            Editor.Title.Trim(),
            Editor.Description,
            Editor.ProjectId,
            Editor.StatusId,
            Editor.TypeId,
            Editor.PriorityId,
            Editor.FixedInRelease,
            Editor.ParentTicketId,
            Editor.AssignedTo);

        return TicketService.UpdateTicketAsync(request, userId);
    }

    private Task<ApplicationResult> UpdateCommentsAsync()
    {
        var comments = Comments
            .Select(comment => new UpdateTicketComment(
                comment.Id,
                comment.Content,
                comment.IsDeleted))
            .ToArray();

        return TicketCommentService.UpdateCommentsAsync(comments);
    }

    private async Task<ApplicationResult<TicketComment>> CreateCommentAsync(Guid userId)
    {
        var request = new CreateTicketCommentRequest(
            Editor!.Id,
            NewComment!.Trim());

        return await TicketCommentService.CreateCommentAsync(request, userId);
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