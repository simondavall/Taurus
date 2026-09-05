using FluentValidation;
using Taurus.Components.Features.Shared;

namespace Taurus.Components.Features.Tickets;

public sealed class TicketCreateEditorValidator : AbstractValidator<TicketCreateEditorModel>, IMudValidator<TicketCreateEditorModel>
{
    public TicketCreateEditorValidator(int completedStatusId, bool requireFixedInRelease)
    {
        RuleFor(ticket => ticket.Title)
            .NotEmpty()
            .WithMessage("Title is required.");

        RuleFor(ticket => ticket.Description)
            .NotEmpty()
            .WithMessage("Description is required.");

        RuleFor(ticket => ticket.FixedInRelease)
            .NotEmpty()
            .When(ticket => requireFixedInRelease && ticket.StatusId == completedStatusId)
            .WithMessage("This ticket cannot be completed, Fixed In Release is required.");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValueAsync => ValidatePropertyAsync;

    private async Task<IEnumerable<string>> ValidatePropertyAsync(object model, string propertyName)
    {
        var context = ValidationContext<TicketCreateEditorModel>
            .CreateWithOptions(
                (TicketCreateEditorModel)model,
                options => options.IncludeProperties(propertyName));

        var result = await ValidateAsync(context);

        return result.Errors.Select(error => error.ErrorMessage);
    }
}