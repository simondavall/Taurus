using FluentValidation;
using Taurus.Components.Features.Shared;

namespace Taurus.Components.Features.Tickets;

public sealed class TicketEditorValidator : AbstractValidator<TicketEditorModel>, IMudValidator<TicketEditorModel>
{
    public TicketEditorValidator()
    {
        RuleFor(ticket => ticket.Title)
            .NotEmpty()
            .WithMessage("Title is required.");
        
        RuleFor(ticket => ticket.Description)
            .NotEmpty()
            .WithMessage("Description is required.");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValueAsync => ValidatePropertyAsync;

    private async Task<IEnumerable<string>> ValidatePropertyAsync(object model, string propertyName)
    {
        var context = ValidationContext<TicketEditorModel>
            .CreateWithOptions(
                (TicketEditorModel)model,
                options => options.IncludeProperties(propertyName));

        var result = await ValidateAsync(context);

        return result.Errors.Select(error => error.ErrorMessage);
    }
}