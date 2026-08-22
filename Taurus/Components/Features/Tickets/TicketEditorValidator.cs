using FluentValidation;
using Taurus.Components.Features.Shared;

namespace Taurus.Components.Features.Tickets;

public sealed class TicketEditorValidator : AbstractValidator<TicketEditorModel>, IMudValidator<TicketEditorModel>
{
    private readonly TicketLookupIds _lookupIds;
    private readonly bool _requireFixedInReleaseForCompletion;

    public TicketEditorValidator(
        TicketLookupIds lookupIds,
        bool requireFixedInReleaseForCompletion)
    {
        _lookupIds = lookupIds;
        _requireFixedInReleaseForCompletion = requireFixedInReleaseForCompletion;

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

        RuleFor(ticket => ticket)
            .Must(CanCompleteTicket)
            .WithMessage("This ticket cannot be completed, Fixed In Release is required.")
            .WithState(_ => TicketValidationPresentation.Banner);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValueAsync => ValidatePropertyAsync;

    private bool CanCloseTicket(TicketEditorModel ticket)
    {
        if (!ticket.HasActiveSubTasks)
        {
            return true;
        }

        return ticket.StatusId != _lookupIds.CompletedStatusId
               && ticket.StatusId != _lookupIds.ObsoleteStatusId;
    }

    private bool CanCompleteTicket(TicketEditorModel ticket)
    {
        if (!_requireFixedInReleaseForCompletion)
        {
            return true;
        }

        if (ticket.StatusId != _lookupIds.CompletedStatusId)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(ticket.FixedInRelease);
    }

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