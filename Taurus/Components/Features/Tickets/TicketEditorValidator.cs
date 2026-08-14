using FluentValidation;

namespace Taurus.Components.Features.Tickets;

public sealed class TicketEditorValidator : AbstractValidator<TicketEditorModel>
{
    public TicketEditorValidator()
    {
        RuleFor(ticket => ticket.Title)
            .NotEmpty()
            .WithMessage("Title is required.");
    }
}