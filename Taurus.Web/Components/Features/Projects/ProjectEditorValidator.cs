using FluentValidation;
using Taurus.Components.Features.Shared;

namespace Taurus.Components.Features.Projects;

public sealed class ProjectEditorValidator : AbstractValidator<ProjectEditorModel>, IMudValidator<ProjectEditorModel>
{
    public ProjectEditorValidator()
    {
        RuleFor(project => project.Title)
            .NotEmpty()
            .WithMessage("Title is required.");

        RuleFor(project => project.Prefix)
            .NotEmpty()
            .WithMessage("Prefix is required.");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValueAsync => ValidatePropertyAsync;

    private async Task<IEnumerable<string>> ValidatePropertyAsync(object model, string propertyName)
    {
        var context = ValidationContext<ProjectEditorModel>
            .CreateWithOptions(
                (ProjectEditorModel)model,
                options => options.IncludeProperties(propertyName));

        var result = await ValidateAsync(context);

        return result.Errors.Select(error => error.ErrorMessage);
    }
}