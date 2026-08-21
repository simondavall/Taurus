using System.Net;
using PegasusApi.Abstractions.Comments;
using Taurus.Application.PegasusApi;
using PegasusCreateCommentRequest = PegasusApi.Abstractions.Comments.CreateCommentRequest;
using PegasusUpdateCommentRequest = PegasusApi.Abstractions.Comments.UpdateCommentRequest;
using PegasusUpdateCommentsRequest = PegasusApi.Abstractions.Comments.UpdateCommentsRequest;

namespace Taurus.Application.Tickets;

public interface ITicketCommentService
{
    Task<IReadOnlyList<TicketComment>> GetCommentsAsync(Guid ticketId);
    Task<ApplicationResult> UpdateCommentsAsync(IReadOnlyList<UpdateTicketComment> comments);
    Task<ApplicationResult<TicketComment>> CreateCommentAsync(CreateTicketCommentRequest request, Guid userId);
}

public sealed class TicketCommentService(
    HttpClient httpClient,
    ILogger<TicketCommentService> logger,
    ITicketReferenceLinker ticketReferenceLinker) : ITicketCommentService
{
    public async Task<IReadOnlyList<TicketComment>> GetCommentsAsync(Guid ticketId)
    {
        logger.LogInformation("Retrieving comments from PegasusApi for ticket {TicketId}", ticketId);

        try
        {
            var requestUri = $"api/comments?TicketId={Uri.EscapeDataString(ticketId.ToString())}";

            var response = await httpClient.GetFromJsonAsync<CommentsResponse>(requestUri);
            if (response is null)
            {
                throw new InvalidOperationException("PegasusApi returned an empty comments response.");
            }

            var comments = response.Items
                .Select(MapComment)
                .ToArray();

            logger.LogInformation("Retrieved {CommentCount} comments from PegasusApi for ticket {TicketId}", comments.Length, ticketId);

            return comments;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to retrieve comments from PegasusApi for ticket {TicketId}", ticketId);

            throw;
        }
    }

    public async Task<ApplicationResult> UpdateCommentsAsync(IReadOnlyList<UpdateTicketComment> comments)
    {
        logger.LogInformation("Updating {CommentCount} comments in PegasusApi", comments.Count);

        try
        {
            var apiComments = new List<PegasusUpdateCommentRequest>(comments.Count);

            foreach (var comment in comments)
            {
                var content = await ticketReferenceLinker.LinkTicketReferencesAsync(comment.Content);

                apiComments.Add(new PegasusUpdateCommentRequest
                {
                    Id = comment.Id,
                    Content = content!,
                    IsDeleted = comment.IsDeleted
                });
            }

            var apiRequest = new PegasusUpdateCommentsRequest
            {
                Comments = apiComments
            };

            using var response = await httpClient.PutAsJsonAsync("api/comments", apiRequest);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Updated {CommentCount} comments in PegasusApi", comments.Count);
                return ApplicationResult.Success();
            }

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            {
                var errorMessage = await PegasusApiFailureReader.ReadAsync(
                    response,
                    "The comments could not be updated because PegasusApi rejected the supplied details.");

                logger.LogWarning("PegasusApi rejected comment update with status code {StatusCode}", (int)response.StatusCode);

                return ApplicationResult.Failure(errorMessage);
            }

            response.EnsureSuccessStatusCode();

            throw new InvalidOperationException("PegasusApi comment update failed unexpectedly.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to update comments in PegasusApi");
            throw;
        }
    }

    public async Task<ApplicationResult<TicketComment>> CreateCommentAsync(CreateTicketCommentRequest request, Guid userId)
    {
        logger.LogInformation("Creating comment in PegasusApi for ticket {TicketId}", request.TicketId);

        try
        {
            var content = await ticketReferenceLinker.LinkTicketReferencesAsync(request.Content);
            
            var apiRequest = new PegasusCreateCommentRequest
            {
                TicketId = request.TicketId,
                Content = content!,
                UserId = userId
            };

            using var response = await httpClient.PostAsJsonAsync("api/comments", apiRequest);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var commentResponse = await response.Content.ReadFromJsonAsync<CommentResponse>();
                if (commentResponse is null)
                {
                    throw new InvalidOperationException(
                        "PegasusApi returned an empty comment response after comment creation.");
                }

                var comment = MapComment(commentResponse);

                logger.LogInformation("Created comment {CommentId} in PegasusApi for ticket {TicketId}", comment.Id, request.TicketId);

                return ApplicationResult<TicketComment>.Success(comment);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorMessage = await PegasusApiFailureReader.ReadAsync(
                    response,
                    "The comment could not be created because PegasusApi rejected the supplied details.");

                logger.LogWarning(
                    "PegasusApi rejected comment creation for ticket {TicketId} with status code {StatusCode}",
                    request.TicketId,
                    (int)response.StatusCode);

                return ApplicationResult<TicketComment>.Failure(errorMessage);
            }

            response.EnsureSuccessStatusCode();

            throw new InvalidOperationException("PegasusApi comment creation failed unexpectedly.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to create comment in PegasusApi for ticket {TicketId}", request.TicketId);
            throw;
        }
    }

    private static TicketComment MapComment(CommentResponse comment)
    {
        return new TicketComment(
            comment.Id,
            comment.TicketId,
            comment.Content,
            comment.IsDeleted,
            comment.DisplayName,
            comment.LastModifiedBy,
            AsUtc(comment.LastModified),
            comment.CreatedBy,
            AsUtc(comment.CreatedDate));
    }

    private static DateTimeOffset AsUtc(DateTimeOffset value)
    {
        return value.Offset == TimeSpan.Zero
            ? value
            : new DateTimeOffset(value.DateTime, TimeSpan.Zero);
    }
}