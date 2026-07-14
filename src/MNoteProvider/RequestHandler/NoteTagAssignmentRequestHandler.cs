using MNoteProvider.BusinessCore.Provider;
using MNoteProvider.Common.DTOs;

namespace MNoteProvider.RequestHandler;
/// <summary>
/// Handles incoming HTTP requests for note tag assignment resources by delegating to the business layer
/// and translating its result types into HTTP responses.
/// </summary>
/// <remarks>
/// This is the boundary between transport and domain: it is the only place where a
/// business result is turned into an <see cref="IResult"/>. Everything below this layer
/// is unaware of HTTP.
/// </remarks>
public interface INoteTagAssignmentRequestHandler
{
    /// <summary>
    /// Retrieves all assignments between notes and tags.
    /// </summary>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the list of assignments, or an error response derived from the failure
    /// returned by the business layer.
    /// </returns>
    Task<IResult> GetAllNoteTagAssignments(CancellationToken ct = default);

    /// <summary>
    /// Assigns a tag to a note.
    /// </summary>
    /// <param name="assignmentDto">Identifies the note and the tag to be linked.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>201 Created</c> with the identifier of the created assignment, or an error
    /// response derived from the failure returned by the business layer.
    /// </returns>
    Task<IResult> AssignTag(AssignmentDto assignmentDto,CancellationToken ct = default);

    /// <summary>
    /// Removes the assignment between a note and a tag.
    /// </summary>
    /// <param name="noteId">Id of the note the tag is assigned to.</param>
    /// <param name="tagId">Id of the tag to remove from the note.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> on success, or an error response derived from the failure returned by the
    /// business layer. Only the assignment is removed; neither the note nor the tag is deleted.
    /// </returns>
    Task<IResult> UnassignTag(Guid noteId, Guid tagId, CancellationToken ct = default);
}
///<inheritdoc cref = "INoteTagAssignmentRequestHandler" />
public class NoteTagAssignmentRequestHandler : INoteTagAssignmentRequestHandler
{
    private readonly INoteTagAssignmentProvider _tagProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteTagAssignmentRequestHandler"/> class.
    /// </summary>
    /// <param name="noteTagAssignmentProvider">
    /// Business layer component this handler delegates all assignment operations to.
    /// </param>
    public NoteTagAssignmentRequestHandler(INoteTagAssignmentProvider noteTagAssignmentProvider)
    {
        _tagProvider = noteTagAssignmentProvider;
    }

    /// <inheritdoc/>
    public async Task<IResult> GetAllNoteTagAssignments(CancellationToken ctx = default)
    {
        var mNoteTagAssignmentsResult = await _tagProvider.GetAllNoteTagAssignments(ctx).ConfigureAwait(false);
        return   mNoteTagAssignmentsResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
    /// <inheritdoc/>
    public async Task<IResult> AssignTag(AssignmentDto assignmentDto, CancellationToken ctx = default)
    {
        var mNoteTagAssignmentsResult = await _tagProvider.AssignTag(assignmentDto,ctx).ConfigureAwait(false);
        return mNoteTagAssignmentsResult.Match(id => Results.Created((string?)null, id), fail => fail.ToIResult());
    }
    /// <inheritdoc/>
    public async Task<IResult> UnassignTag(Guid noteId, Guid tagId, CancellationToken ctx = default)
    {
        var mNoteTagAssignmentsResult = await _tagProvider.UnassignTag(noteId, tagId, ctx).ConfigureAwait(false);
        return   mNoteTagAssignmentsResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
}
