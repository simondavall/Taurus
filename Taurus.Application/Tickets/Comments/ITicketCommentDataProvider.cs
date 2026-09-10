namespace Taurus.Application.Tickets.Comments;

public interface ITicketCommentDataProvider
{
    Task<ApplicationResult<TicketComment>> CreateCommentAsync(CreateTicketComment createComment, Guid userId);
    Task<IReadOnlyList<TicketComment>> GetCommentsAsync(Guid ticketId);
    Task<ApplicationResult> UpdateCommentsAsync(IReadOnlyList<UpdateTicketComment> comments);
}