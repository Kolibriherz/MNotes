using Microsoft.Extensions.Logging;
using MNoteProvider.BusinessCore.Mapping;
using MNoteProvider.Common.Abstractions;
using MNoteProvider.Common.Abstractions.Enums;
using MNoteProvider.Common.Abstractions.Resources;
using MNoteProvider.Common.DTOs;
using MNoteProvider.DataAccess.Repositories;
using MNoteProvider.Domain;
using Npgsql;
using OneOf;

namespace MNoteProvider.BusinessCore.Provider;

/// <summary>
/// Coordinates the assignment of tags to notes by mediating between the HTTP
/// request handlers and the note-tag assignment repository. Manages the
/// many-to-many relationship between notes and tags.
/// </summary>
public interface INoteTagAssignmentProvider
{
    /// <summary>Gets all note-tag assignments.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>All assignments as <see cref="NoteTagAssignmentDto"/> values, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<NoteTagAssignmentDto[], MNoteProcessFail>> GetAllNoteTagAssignments(CancellationToken ct = default);

    /// <summary>Assigns a tag to a note.</summary>
    /// <param name="assignmentDto">The data identifying the note and the tag to link.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The identifier of the created assignment, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<Guid, MNoteProcessFail>> AssignTag(AssignmentDto assignmentDto, CancellationToken ct = default);

    /// <summary>Removes the assignment between the specified note and tag.</summary>
    /// <param name="noteId">The identifier of the note.</param>
    /// <param name="tagId">The identifier of the tag.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the assignment was removed, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<bool, MNoteProcessFail>> UnassignTag(Guid noteId, Guid tagId, CancellationToken ct = default);
}

///<inheritdoc cref = "INoteTagAssignmentProvider" />
public class NoteTagAssignmentProvider : INoteTagAssignmentProvider
{
    private readonly INoteTagAssignmentRepository _noteTagAssignmentRepository;
    private readonly ILogger<NoteTagAssignmentProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteTagAssignmentProvider"/> class.
    /// </summary>
    /// <param name="noteTagAssignmentRepository">The repository used to persist and read note-tag assignments.</param>
    /// <param name="logger">The logger used to record database failures.</param>
    public NoteTagAssignmentProvider(
        INoteTagAssignmentRepository noteTagAssignmentRepository,
        ILogger<NoteTagAssignmentProvider> logger)
    {
        _noteTagAssignmentRepository = noteTagAssignmentRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<OneOf<NoteTagAssignmentDto[], MNoteProcessFail>> GetAllNoteTagAssignments(CancellationToken ct = default)
    {
        try
        {
            var assignments = await _noteTagAssignmentRepository.GetAllAsync(ct).ConfigureAwait(false);
            return assignments.ToDtos().ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while trying to get note-tag assignments");
            return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("get", "note-tag assignments"));
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> AssignTag(AssignmentDto assignmentDto, CancellationToken ct = default)
    {
        var newAssignment = new NoteTagAssignment
        {
            Id = Guid.NewGuid(),
            Doeom = DateTime.UtcNow,
            TagId = assignmentDto.TagId,
            NoteId = assignmentDto.NoteId
        };

        try
        {
            var saved = await _noteTagAssignmentRepository.CreateAsync(newAssignment, ct).ConfigureAwait(false);
            return saved
                ? newAssignment.Id
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("save", "note tag assignment"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while assigning tag {TagId} to note {NoteId}",
                assignmentDto.TagId, assignmentDto.NoteId);
            return DatabaseFailureMapper.Map(e, "create", "note-tag assignment");
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> UnassignTag(Guid noteId, Guid tagId, CancellationToken ct = default)
    {
        try
        {
            var deleted = await _noteTagAssignmentRepository.DeleteAsync(noteId, tagId, ct).ConfigureAwait(false);
            return deleted
                ? true
                : new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.AssignmentDoesNotExist(noteId, tagId));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while unassigning tag {TagId} from note {NoteId}", tagId, noteId);
            return DatabaseFailureMapper.Map(e, "delete", "note-tag assignment");
        }
    }
}
