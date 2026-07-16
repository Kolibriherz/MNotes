using Microsoft.Extensions.Logging;
using MNoteProvider.BusinessCore.Mapping;
using MNoteProvider.Common;
using MNoteProvider.Common.Abstractions.Enums;
using MNoteProvider.Common.Abstractions.Resources;
using MNoteProvider.Common.DTOs;
using MNoteProvider.DataAccess.Repositories;
using MNoteProvider.Domain;
using Npgsql;
using OneOf;

namespace MNoteProvider.BusinessCore.Provider;

/// <summary>
/// Coordinates comment operations by mediating between the HTTP request handlers
/// and the comment repository. Handles creation, updating, deletion and retrieval
/// of comments belonging to a note.
/// </summary>
public interface ICommentProvider
{
    /// <summary>Gets all comments that belong to the specified note.</summary>
    /// <param name="id">The identifier of the note whose comments are loaded.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The comments of the note as <see cref="CommentDto"/> values, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<CommentDto[], MNoteProcessFail>> GetAllCommentsByNote(Guid id, CancellationToken ct = default);

    /// <summary>Creates a new comment.</summary>
    /// <param name="createCommentDto">The data used to create the comment.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The identifier of the created comment, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<Guid, MNoteProcessFail>> CreateComment(CreateCommentDto createCommentDto, CancellationToken ct = default);

    /// <summary>Updates the content of an existing comment.</summary>
    /// <param name="commentDto">The data used to update the comment.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the comment was updated, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<bool, MNoteProcessFail>> UpdateComment(CommentDto commentDto, CancellationToken ct = default);

    /// <summary>Deletes the comment with the specified identifier.</summary>
    /// <param name="id">The identifier of the comment to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the comment was deleted, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<bool, MNoteProcessFail>> DeleteComment(Guid id, CancellationToken ct = default);
}

///<inheritdoc cref = "ICommentProvider" />
public class CommentProvider : ICommentProvider
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<CommentProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentProvider"/> class.
    /// </summary>
    /// <param name="commentRepository">The repository used to persist and read comments.</param>
    /// <param name="logger">The logger used to record database failures.</param>
    public CommentProvider(ICommentRepository commentRepository, ILogger<CommentProvider> logger)
    {
        _commentRepository = commentRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<OneOf<CommentDto[], MNoteProcessFail>> GetAllCommentsByNote(Guid id, CancellationToken ct = default)
    {
        try
        {
            var comments = await _commentRepository.GetAllByNoteAsync(id, ct).ConfigureAwait(false);
            return comments.ToDtos().ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while trying to get comments for note {id}.", id);
            return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("get", "comments"));
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> CreateComment(CreateCommentDto createCommentDto, CancellationToken ct = default)
    {
        var newComment = new Comment
        {
            Id = Guid.NewGuid(),
            Doeom = DateTime.UtcNow,
            Content = createCommentDto.Content,
            NoteId = createCommentDto.NoteId,
            CreationDate = DateTime.UtcNow
        };

        try
        {
            var saved = await _commentRepository.CreateAsync(newComment, ct).ConfigureAwait(false);
            return saved
                ? newComment.Id
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("save", "comment"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while creating comment {CommentId} for note {NoteId}", newComment.Id, newComment.NoteId);
            return DatabaseFailureMapper.Map(e, "save", "comment");
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> UpdateComment(CommentDto commentDto, CancellationToken ct = default)
    {
        try
        {
            var updateComment = await _commentRepository.GetByIdAsync(commentDto.Id, ct).ConfigureAwait(false);
            if (updateComment is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(commentDto.Id));

            // NoteId is intentionally immutable — a comment stays on its note; only the content changes.
            updateComment.Content = commentDto.Content;
            updateComment.Doeom = DateTime.UtcNow;

            var updated = await _commentRepository.UpdateAsync(updateComment, ct).ConfigureAwait(false);
            return updated
                ? true
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("update", "comment"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while updating comment {CommentId}", commentDto.Id);
            return DatabaseFailureMapper.Map(e, "update", "comment");
        }

    }

    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> DeleteComment(Guid id, CancellationToken ct = default)
    {
        try
        {
            if (await _commentRepository.GetByIdAsync(id, ct).ConfigureAwait(false) is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(id));

            var deleted = await _commentRepository.DeleteAsync(id, ct).ConfigureAwait(false);
            return deleted
                ? true
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("delete", "comment"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while deleting comment {CommentId}", id);
            return DatabaseFailureMapper.Map(e, "delete", "comment");
        }
    }
}
