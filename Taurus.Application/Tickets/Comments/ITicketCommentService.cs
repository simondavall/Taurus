namespace Taurus.Application.Tickets.Comments;

public interface ITicketCommentService
{
    Task<ApplicationResult<TicketComment>> CreateCommentAsync(CreateTicketCommentRequest request, Guid userId);
    Task<IReadOnlyList<TicketComment>> GetCommentsAsync(Guid ticketId);
    Task<ApplicationResult> UpdateCommentsAsync(IReadOnlyList<UpdateTicketComment> comments);
}