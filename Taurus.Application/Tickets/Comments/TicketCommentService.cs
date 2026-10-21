namespace Taurus.Application.Tickets.Comments;

public interface ITicketCommentService
{
    Task<ApplicationResult<TicketComment>> CreateCommentAsync(CreateTicketComment createComment, Guid userId);
    Task<IReadOnlyList<TicketComment>> GetCommentsAsync(Guid ticketId);
    Task<ApplicationResult> UpdateCommentsAsync(IReadOnlyList<UpdateTicketComment> comments);
}

public sealed class TicketCommentService(ITicketCommentDataProvider ticketCommentDataProvider) : ITicketCommentService
{
    public Task<ApplicationResult<TicketComment>> CreateCommentAsync(CreateTicketComment createComment, Guid userId)
    {
        return ticketCommentDataProvider.CreateCommentAsync(createComment, userId);
    }

    public Task<IReadOnlyList<TicketComment>> GetCommentsAsync(Guid ticketId)
    {
        return ticketCommentDataProvider.GetCommentsAsync(ticketId);
    }

    public Task<ApplicationResult> UpdateCommentsAsync(IReadOnlyList<UpdateTicketComment> comments)
    {
        return ticketCommentDataProvider.UpdateCommentsAsync(comments);
    }
}
