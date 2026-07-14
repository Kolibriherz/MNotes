using MNoteProvider.BusinessCore.Provider;
using MNoteProvider.Common.DTOs;

namespace MNoteProvider.RequestHandler;
/// <summary>
/// Handles incoming HTTP requests for comment resources by delegating to the business layer
/// and translating its result types into HTTP responses.
/// </summary>
/// <remarks>
/// This is the boundary between transport and domain: it is the only place where a
/// business result is turned into an <see cref="IResult"/>. Everything below this layer
/// is unaware of HTTP.
/// </remarks>
public interface ICommentRequestHandler
{
    /// <summary>
    /// Retrieves all comments belonging to a single note.
    /// </summary>
    /// <param name="id">Id of the note whose comments are requested.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the list of comments, or an error response derived from the failure
    /// returned by the business layer. A note without comments yields an empty list, not a failure.
    /// </returns>
    Task<IResult> GetAllCommentsByNote(Guid id,CancellationToken ct = default);

    /// <summary>
    /// Creates a new comment on a note.
    /// </summary>
    /// <param name="createCommentDto">Content of the comment and the id of the note it belongs to.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>201 Created</c> with the identifier of the created comment, or an error
    /// response derived from the failure returned by the business layer.
    /// </returns>
    Task<IResult> CreateComment(CreateCommentDto createCommentDto,CancellationToken ct = default);

    /// <summary>
    /// Updates an existing comment.
    /// </summary>
    /// <param name="commentDto">Comment to update, identified by its id.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the updated comment, or an error response derived from the failure
    /// returned by the business layer — for example when the comment does not exist.
    /// </returns>
    Task<IResult> UpdateComment(CommentDto commentDto, CancellationToken ct = default);

    /// <summary>
    /// Deletes the comment with the given id.
    /// </summary>
    /// <param name="id">Id of the comment to delete.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> on success, or an error response derived from the failure returned by the
    /// business layer. Only the comment is removed; the note it belongs to is left untouched.
    /// </returns>
    Task<IResult> DeleteComment(Guid id, CancellationToken ct = default);
}

///<inheritdoc cref = "ICommentRequestHandler" />
public class CommentRequestHandler : ICommentRequestHandler
{
    private readonly ICommentProvider _commentProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentRequestHandler"/> class.
    /// </summary>
    /// <param name="commentProvider">Business layer component this handler delegates all comment operations to.</param>
    public CommentRequestHandler(ICommentProvider commentProvider)
    {
        _commentProvider = commentProvider;
    }
    /// <inheritdoc/>
    public async Task<IResult> GetAllCommentsByNote(Guid id, CancellationToken ct = default)
    {
        var mCommentsResult = await _commentProvider.GetAllCommentsByNote(id, ct).ConfigureAwait(false);
        return mCommentsResult.Match(id => Results.Created((string?)null, id), fail => fail.ToIResult());

    }
    /// <inheritdoc/>
    public async Task<IResult> CreateComment(CreateCommentDto createCommentDto, CancellationToken ct = default)
    {
        var mCommentsResult = await _commentProvider.CreateComment(createCommentDto,ct).ConfigureAwait(false);
        return   mCommentsResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
    /// <inheritdoc/>
    public async Task<IResult> UpdateComment(CommentDto commentDto, CancellationToken ct = default)
    {
        var mCommentsResult = await _commentProvider.UpdateComment(commentDto,ct).ConfigureAwait(false);
        return   mCommentsResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
    /// <inheritdoc/>
    public async Task<IResult> DeleteComment(Guid id, CancellationToken ct = default)
    {
        var mCommentsResult = await _commentProvider.DeleteComment(id, ct).ConfigureAwait(false);
        return   mCommentsResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
}
