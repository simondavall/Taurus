using Microsoft.AspNetCore.Components;
using MudBlazor;
using Taurus.Application.Projects;

namespace Taurus.Components.Features.Projects;

public partial class ProjectDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    private readonly ProjectEditorValidator _validator = new();

    private MudForm _form = default!;

    private ProjectEditorModel Model { get; } = new();

    private string? _errorMessage;

    private bool _saving;

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
            var request = new CreateProjectRequest(
                Model.Title.Trim(),
                Model.Prefix.Trim());

            var result = await ProjectService.CreateProjectAsync(request);

            if (!result.Succeeded)
            {
                _errorMessage = result.ErrorMessage;
                return;
            }

            MudDialog.Close(DialogResult.Ok(result.Project));
        }
        finally
        {
            _saving = false;
        }
    }
}