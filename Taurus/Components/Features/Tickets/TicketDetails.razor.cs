using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Taurus.Application;
using Taurus.Application.Markdown;
using Taurus.Application.Projects;
using Taurus.Application.Tickets;
using Taurus.Application.Tickets.Comments;
using Taurus.Application.Tickets.Lookups;
using Taurus.Application.Users;
using Taurus.Components.Features.Shared;
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
    private ITicketLookupDataService TicketLookupDataService { get; set; } = default!;
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;
    [Inject]
    private IMarkdownRenderer MarkdownRenderer { get; set; } = default!;
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private INavigationHistoryService NavigationHistoryService { get; set; } = default!;
    [Inject]
    private IDialogService DialogService { get; set; } = default!;
    [Inject]
    private IUserService UserService { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private TicketEditorValidator _validator = default!;
    private IReadOnlyList<string> ValidationBannerMessages { get; set; } = [];
    
    private MudForm? _form;
    private TicketEditorModel? Editor { get; set; }

    private IReadOnlyList<Project> Projects { get; set; } = [];
    private IReadOnlyList<TicketStatus> TicketStatuses { get; set; } = [];
    private IReadOnlyList<TicketPriority> TicketPriorities { get; set; } = [];
    private IReadOnlyList<TicketType> TicketTypes { get; set; } = [];
    private IReadOnlyList<Ticket> SubTasks { get; set; } = [];
    private List<CommentEditorModel> Comments { get; set; } = [];
    private Application.Tickets.TicketDetails? ParentTicket { get; set; }
    private IReadOnlyList<User> Users { get; set; } = [];
    private Application.Tickets.TicketDetails? LoadedTicket { get; set; }

    private TicketLookupIds LookupIds { get; set; } = default!;
    
    private bool _loading;
    private bool _saving;
    private string? _loadError;
    private string? _updateError;
    private bool _descriptionEditing;
    private string? NewComment { get; set; }


    private string ProjectTitle
    {
        get
        {
            if (CurrentProject is null)
            {
                return "Unknown project";
            }

            return string.IsNullOrWhiteSpace(CurrentProject.LatestVersion)
                ? CurrentProject.Title
                : $"{CurrentProject.Title} ({CurrentProject.LatestVersion})";
        }
    }

    private Project? CurrentProject =>
        Editor is null
            ? null
            : Projects.FirstOrDefault(project => project.Id == Editor.ProjectId);

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        _loadError = null;
        _updateError = null;
        _descriptionEditing = false;
        ValidationBannerMessages = [];
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
        var statusesTask = TicketLookupDataService.GetStatusesAsync();
        var prioritiesTask = TicketLookupDataService.GetPrioritiesAsync();
        var typesTask = TicketLookupDataService.GetTypesAsync();
        var ticketTask = TicketService.GetTicketByRefAsync(TicketRef);
        var usersTask = UserService.GetUsersAsync();

        await Task.WhenAll(
            projectsTask,
            statusesTask,
            prioritiesTask,
            typesTask,
            usersTask,
            ticketTask);

        Projects = await projectsTask;
        TicketStatuses = await statusesTask;
        TicketPriorities = await prioritiesTask;
        TicketTypes = await typesTask;
        Users = await usersTask;

        LookupIds = TicketLookupIds.Resolve(TicketStatuses, TicketPriorities);
        
        var ticketResult = await ticketTask;
        if (!ticketResult.Succeeded || ticketResult.Value is null)
        {
            ClearTicketData();
            _loadError = ticketResult.ErrorMessage ?? "The ticket could not be loaded.";
            return;
        }

        SetEditor(ticketResult.Value);
        
        _validator = new TicketEditorValidator(LookupIds, CurrentProject?.RequireFixedInRelease ?? false);
        
        await LoadRelatedTicketDataAsync(ticketResult.Value);
    }

    private async Task ReloadPageDataAsync()
    {
        var ticketResult = await TicketService.GetTicketByRefAsync(TicketRef);
        if (!ticketResult.Succeeded || ticketResult.Value is null)
        {
            ClearTicketData();
            _loadError = ticketResult.ErrorMessage ?? "The ticket could not be reloaded.";
            return;
        }

        SetEditor(ticketResult.Value);
        await LoadRelatedTicketDataAsync(ticketResult.Value);

        NewComment = null;
        ValidationBannerMessages = [];
    }

    private async Task LoadRelatedTicketDataAsync(Application.Tickets.TicketDetails ticket)
    {
        var commentsTask = LoadCommentsAsync(ticket.Id);
        var subTasksTask = TicketService.GetSubTasksAsync(ticket.TicketRef);

        Task<ApplicationResult<Application.Tickets.TicketDetails>>? parentTask = null;

        if (!string.IsNullOrWhiteSpace(ticket.ParentTicketRef))
        {
            parentTask = TicketService.GetTicketByRefAsync(ticket.ParentTicketRef);
        }

        if (parentTask is null)
        {
            await Task.WhenAll(commentsTask, subTasksTask);
        }
        else
        {
            await Task.WhenAll(commentsTask, subTasksTask, parentTask);
        }

        SubTasks = (await subTasksTask)
            .OrderByDescending(subTask => subTask.LastModified)
            .ToArray();

        Editor?.HasActiveSubTasks = SubTasks.Any(subTask => !TicketPresentation.IsInactive(subTask, LookupIds));

        ParentTicket = null;

        if (parentTask is not null)
        {
            var parentResult = await parentTask;

            if (parentResult.Succeeded && parentResult.Value is not null)
            {
                ParentTicket = parentResult.Value;
            }
        }
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

    private void ClearTicketData()
    {
        Editor = null;
        LoadedTicket = null;
        ParentTicket = null;
        SubTasks = [];
        Comments = [];
        ValidationBannerMessages = [];
    }

    private void SetEditor(Application.Tickets.TicketDetails ticket)
    {
        LoadedTicket = ticket;

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
            ParentTicketRef = ticket.ParentTicketRef,
            AssignedTo = ticket.AssignedTo
        };

        _descriptionEditing = string.IsNullOrWhiteSpace(ticket.Description);
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

    private string RenderDescription()
    {
        return MarkdownRenderer.Render(Editor?.Description);
    }

    private void EditDescription()
    {
        if (_saving)
        {
            return;
        }

        _descriptionEditing = true;
    }

    private void DescriptionKeyDown(KeyboardEventArgs args)
    {
        if (args.Key is "Enter" or " ")
        {
            EditDescription();
        }
    }
    
    private static string GetCommentClass(CommentEditorModel comment)
    {
        return comment.IsDeleted
            ? "ticket-comment ticket-comment-deleted"
            : "ticket-comment";
    }

    private static string FormatAge(DateTimeOffset lastModified)
    {
        return TicketPresentation.FormatAge(lastModified);
    }

    private async Task AddSubTaskAsync()
    {
        if (Editor is null || CurrentProject is null || _saving)
        {
            return;
        }

        var parameters = new DialogParameters
        {
            [nameof(TicketCreateDialog.Project)] = CurrentProject,
            [nameof(TicketCreateDialog.TicketTypes)] = TicketTypes,
            [nameof(TicketCreateDialog.TicketPriorities)] = TicketPriorities,
            [nameof(TicketCreateDialog.TicketStatuses)] = TicketStatuses,
            [nameof(TicketCreateDialog.ParentTicketRef)] = Editor.TicketRef
        };

        var dialog = await DialogService.ShowAsync<TicketCreateDialog>(
            $"Create Sub Task — {CurrentProject.Title}",
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
        NavigateToTicket(ticket.TicketRef);
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

    private void OpenSubTask(Ticket ticket)
    {
        NavigateToTicket(ticket.TicketRef);
    }

    private void SubTaskKeyDown(KeyboardEventArgs args, Ticket ticket)
    {
        if (args.Key is "Enter" or " ")
        {
            OpenSubTask(ticket);
        }
    }

    private string ResolveUserDisplayName(Guid userId)
    {
        return Users.FirstOrDefault(user => user.Id == userId)?.DisplayName
               ?? "Unknown user";
    }

    private bool HasResolvedAssignedUser =>
        Editor?.AssignedTo is not null &&
        Users.Any(user => user.Id == Editor.AssignedTo.Value);

    private static string FormatAuditDate(DateTimeOffset date)
    {
        return date
            .ToLocalTime()
            .ToString("dd/MM/yyyy HH:mm");
    }
    
    private void NavigateToParent()
    {
        if (ParentTicket is not null)
        {
            NavigateToTicket(ParentTicket.TicketRef);
        }
    }

    private void NavigateToTicket(string ticketRef)
    {
        NavigationManager.NavigateTo(
            $"/tickets/{Uri.EscapeDataString(ticketRef)}");
    }

    private async Task UpdateAsync()
    {
        if (Editor is null || _saving)
        {
            return;
        }

        _updateError = null;
        ValidationBannerMessages = [];

        if (!await ValidateEditorAsync())
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

            if (IsClosedTicket() && NavigationHistoryService.TryNavigateBack())
            {
                return;
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
            Editor.ParentTicketRef,
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

    private async Task<bool> ValidateEditorAsync()
    {
        await _form!.ValidateAsync();

        var validationResult = await _validator.ValidateAsync(Editor!);

        ValidationBannerMessages = validationResult.Errors
            .Where(error => error.CustomState is TicketValidationPresentation.Banner)
            .Select(error => error.ErrorMessage)
            .Distinct()
            .ToArray();

        return _form.IsValid && validationResult.IsValid;
    }
    
    private void DismissValidationBanner()
    {
        ValidationBannerMessages = [];
    }
    
    private async Task<ApplicationResult<TicketComment>> CreateCommentAsync(Guid userId)
    {
        var request = new CreateTicketCommentRequest(
            Editor!.Id,
            NewComment!.Trim());

        return await TicketCommentService.CreateCommentAsync(request, userId);
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

    private bool IsClosedTicket()
    {
        return Editor is not null &&
               (Editor.StatusId == LookupIds.CompletedStatusId ||
                Editor.StatusId == LookupIds.ObsoleteStatusId);
    }
    
    private string GetSubTaskClass(Ticket ticket)
    {
        return TicketPresentation.IsInactive(ticket, LookupIds)
            ? "ticket-subtask-row ticket-inactive"
            : "ticket-subtask-row";
    }
    
    private async Task CancelAsync()
    {
        if (_saving)
        {
            return;
        }

        _updateError = null;
        ValidationBannerMessages = [];

        await ReloadPageDataAsync();
    }
}