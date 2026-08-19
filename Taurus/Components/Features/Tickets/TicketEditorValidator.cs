using FluentValidation;
using Taurus.Components.Features.Shared;

namespace Taurus.Components.Features.Tickets;

public sealed class TicketEditorValidator : AbstractValidator<TicketEditorModel>, IMudValidator<TicketEditorModel>
{
    private readonly TicketReferenceIds _referenceIds;

    public TicketEditorValidator(TicketReferenceIds referenceIds)
    {
        _referenceIds = referenceIds;

        RuleFor(ticket => ticket.Title)
            .NotEmpty()
            .WithMessage("Title is required.");

        RuleFor(ticket => ticket.Description)
            .NotEmpty()
            .WithMessage("Description is required.");

        RuleFor(ticket => ticket)
            .Must(CanCloseTicket)
            .WithMessage("This ticket cannot be closed, it has active sub tasks.")
            .WithState(_ => TicketValidationPresentation.Banner);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValueAsync => ValidatePropertyAsync;

    private bool CanCloseTicket(TicketEditorModel ticket)
    {
        if (!ticket.HasActiveSubTasks)
        {
            return true;
        }

        return ticket.StatusId != _referenceIds.CompletedStatusId
               && ticket.StatusId != _referenceIds.ObsoleteStatusId;
    }

    private async Task<IEnumerable<string>> ValidatePropertyAsync(object model, string propertyName)
    {
        var context = ValidationContext<TicketEditorModel>
            .CreateWithOptions((TicketEditorModel)model, options => options.IncludeProperties(propertyName));

        var result = await ValidateAsync(context);
        return result.Errors.Select(error => error.ErrorMessage);
    }
}