using FluentValidation;

namespace Taurus.Components.Features.Projects;

public sealed class ProjectEditorValidator : AbstractValidator<ProjectEditorModel>
{
    public ProjectEditorValidator()
    {
        RuleFor(project => project.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title is required.");

        RuleFor(project => project.Prefix)
            .Must(prefix => !string.IsNullOrWhiteSpace(prefix))
            .WithMessage("Prefix is required.");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var context = ValidationContext<ProjectEditorModel>.CreateWithOptions(
            (ProjectEditorModel)model,
            options => options.IncludeProperties(propertyName));

        var result = await ValidateAsync(context);

        return result.IsValid ? [] : result.Errors.Select(error => error.ErrorMessage);
    };
}