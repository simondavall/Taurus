using Microsoft.AspNetCore.Components;
using MudBlazor;
using Taurus.Application.Projects;

namespace Taurus.Components.Features.Projects;

public partial class ProjectDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    [Parameter]
    public Project? ProjectToEdit { get; set; }

    private readonly ProjectEditorValidator _validator = new();

    private MudForm _form = default!;

    private ProjectEditorModel Model { get; } = new();

    private bool IsEdit => ProjectToEdit is not null;

    private bool IsBusy => _saving || _deleting;

    private string? _errorMessage;

    private bool _saving;

    private bool _deleting;

    protected override void OnInitialized()
    {
        if (ProjectToEdit is null)
        {
            return;
        }

        Model.Id = ProjectToEdit.Id;
        Model.Title = ProjectToEdit.Title;
        Model.Prefix = ProjectToEdit.Prefix;
        Model.LatestVersion = ProjectToEdit.LatestVersion;
        Model.IsActive = ProjectToEdit.IsActive;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task SaveAsync()
    {
        _errorMessage = null;

        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            return;
        }

        _saving = true;

        try
        {
            if (IsEdit)
            {
                await UpdateProjectAsync();
            }
            else
            {
                await CreateProjectAsync();
            }
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task DeleteAsync()
    {
        if (Model.Id is null)
        {
            throw new InvalidOperationException("A project identifier is required when deleting a project.");
        }

        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete Project",
            $"Are you sure you want to delete '{Model.Title}'?",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        _errorMessage = null;
        _deleting = true;

        try
        {
            var result = await ProjectService.DeleteProjectAsync(Model.Id.Value);

            if (!result.Succeeded)
            {
                _errorMessage = result.ErrorMessage;
                return;
            }

            MudDialog.Close(DialogResult.Ok(ProjectDialogResult.Deleted));
        }
        finally
        {
            _deleting = false;
        }
    }

    private async Task CreateProjectAsync()
    {
        var request = new CreateProjectRequest(Model.Title.Trim(), Model.Prefix.Trim());
        var result = await ProjectService.CreateProjectAsync(request);

        if (!result.Succeeded)
        {
            _errorMessage = result.ErrorMessage;
            return;
        }

        MudDialog.Close(DialogResult.Ok(result.Value));
    }

    private async Task UpdateProjectAsync()
    {
        if (Model.Id is null)
        {
            throw new InvalidOperationException("A project identifier is required when editing a project.");
        }

        var request = new UpdateProjectRequest(
            Model.Id.Value,
            Model.Title.Trim(),
            Model.Prefix.Trim(),
            string.IsNullOrWhiteSpace(Model.LatestVersion) ? null : Model.LatestVersion.Trim(),
            Model.IsActive);

        var result = await ProjectService.UpdateProjectAsync(request);

        if (!result.Succeeded)
        {
            _errorMessage = result.ErrorMessage;
            return;
        }

        MudDialog.Close(DialogResult.Ok(ProjectDialogResult.Updated));
    }
}