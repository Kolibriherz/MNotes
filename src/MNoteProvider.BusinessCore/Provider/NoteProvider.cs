using Microsoft.Extensions.Logging;
using MNoteProvider.BusinessCore.Mapping;
using MNoteProvider.Common;
using MNoteProvider.Common.Abstractions.Enums;
using MNoteProvider.Common.Abstractions.Events;
using MNoteProvider.Common.Abstractions.Resources;
using MNoteProvider.Common.DTOs;
using MNoteProvider.Common.Events;
using MNoteProvider.DataAccess.Repositories;
using MNoteProvider.Domain;
using Npgsql;
using OneOf;

namespace MNoteProvider.BusinessCore.Provider;

/// <summary>
/// Coordinates note operations by combining note persistence, event publishing
/// and the note event stream. Acts as the business layer between the HTTP request
/// handlers and the underlying repositories.
/// </summary>
public interface INoteProvider
{
    /// <summary>Gets all notes.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>All notes as <see cref="NoteDto"/> values, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<NoteDto[], MNoteProcessFail>> GetAllNotes(CancellationToken ct = default);

    /// <summary>Creates a new note and publishes a note-created event.</summary>
    /// <param name="createNoteDto">The data used to create the note.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The identifier of the created note, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<Guid, MNoteProcessFail>> CreateNote(CreateNoteDto createNoteDto, CancellationToken ct = default);

    /// <summary>Updates an existing note and publishes a note-updated event containing the previous and new state.</summary>
    /// <param name="noteDto">The data used to update the note.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the note was updated, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<bool, MNoteProcessFail>> UpdateNote(NoteDto noteDto, CancellationToken ct = default);

    /// <summary>Deletes the note with the specified identifier and publishes a note-deleted event.</summary>
    /// <param name="id">The identifier of the note to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the note was deleted, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<bool, MNoteProcessFail>> DeleteNote(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Loads the previous version of a note by reading its update history.
    /// </summary>
    /// <remarks>
    /// The history is ordered by change date descending, so the first event is the most
    /// recent update; its <c>OldNote</c> therefore represents the state immediately before
    /// the last change. If no update events exist, the note has never been changed and its
    /// current state is returned instead.
    /// </remarks>
    /// <param name="noteId">The identifier of the note whose previous version is loaded.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>
    /// The previous <see cref="NoteDto"/> version, the current note if no history exists,
    /// or an <see cref="MNoteProcessFail"/> if the note cannot be found.
    /// </returns>
    Task<OneOf<NoteDto, MNoteProcessFail>> LoadPreviousVersion(Guid noteId, CancellationToken ct = default);

    /// <summary>Gets the full update history of the specified note, ordered from newest to oldest.</summary>
    /// <param name="noteId">The identifier of the note whose history is loaded.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The note's update events, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<UpdateEvent[], MNoteProcessFail>> GetHistory(Guid noteId, CancellationToken ct = default);
}
///<inheritdoc cref = "INoteProvider" />
public class NoteProvider : INoteProvider
{
    private readonly INoteEventPublisher _eventPublisher;
    private readonly INoteRepository _noteRepository;
    private readonly IEventstreamRepository _eventRepository;
    private readonly ILogger<NoteProvider> _logger;
    private const int MaxNameLength = 255;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteProvider"/> class.
    /// </summary>
    /// <param name="noteRepository">The repository used to persist and read notes.</param>
    /// <param name="eventPublisher">The publisher used to broadcast note events and persist update events.</param>
    /// <param name="eventRepository">The event stream repository used to read a note's update history.</param>
    /// <param name="logger">The logger used to record database and publishing failures.</param>
    public NoteProvider(
        INoteRepository noteRepository,
        INoteEventPublisher eventPublisher,
        IEventstreamRepository eventRepository,
        ILogger<NoteProvider> logger)
    {
        _noteRepository = noteRepository;
        _eventPublisher = eventPublisher;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<OneOf<NoteDto[], MNoteProcessFail>> GetAllNotes(CancellationToken ct = default)
    {
        try
        {
            var notes = await _noteRepository.GetAllAsync(ct).ConfigureAwait(false);
            return notes.ToDtos().ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while trying to get notes");
            return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("get", "notes"));
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> CreateNote(CreateNoteDto createNoteDto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(createNoteDto.Name))
            return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameRequired("note"));

        if (createNoteDto.Name.Length > MaxNameLength)
            return new MNoteProcessFail(MNotesFailType.BADREQUEST,ErrorMessages.NameTooLong("note", MaxNameLength));

        var newNote = new Note
        {
            Id = Guid.NewGuid(),
            Name = createNoteDto.Name,
            FolderId = createNoteDto.FolderId,
            Content = string.Empty,
            Description = string.Empty,
            CreationDate = DateTime.UtcNow,
            Doeom = DateTime.UtcNow
        };

        try
        {
            var saved = await _noteRepository.CreateAsync(newNote, ct).ConfigureAwait(false);
            if (!saved)
                return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("save", "note"));

            await _eventPublisher.PublishCreatedAsync(newNote.ToDto(), ct).ConfigureAwait(false);
            return newNote.Id;
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while creating note {NoteId}", newNote.Id);
            return DatabaseFailureMapper.Map(e, "create", "note");
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> UpdateNote(NoteDto noteDto, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(noteDto.Name))
                return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameRequired("note"));

            if (noteDto.Name.Length > MaxNameLength)
                return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameTooLong("note", MaxNameLength));

            var oldNote = await _noteRepository.GetByIdAsync(noteDto.Id, ct).ConfigureAwait(false);

            if (oldNote is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND,ErrorMessages.EntryDoesNotExist(noteDto.Id));
            
            var updateNote = new Note
            {
                Id = oldNote.Id,
                Name = noteDto.Name,
                Content = noteDto.Content,
                Description = noteDto.Description,
                FolderId = noteDto.FolderId,
                CreationDate = oldNote.CreationDate,
                Doeom = DateTime.UtcNow
            };

            var saved = await _noteRepository.UpdateAsync(updateNote, ct).ConfigureAwait(false);
            if (!saved)
                return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("update", "note"));

            await _eventPublisher
                .PublishUpdatedAsync(new UpdateEvent(oldNote.ToDto(), updateNote.ToDto()), ct)
                .ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while updating note {NoteId}", noteDto.Id);
            return DatabaseFailureMapper.Map(e, "update", "note");
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> DeleteNote(Guid id, CancellationToken ct = default)
    {
        try
        {
            if (await _noteRepository.GetByIdAsync(id, ct) is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(id));

            var saved = await _noteRepository.DeleteAsync(id, ct).ConfigureAwait(false);
            if (!saved)
                return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("delete", "note"));

            await _eventPublisher.PublishDeletedAsync(id, ct).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while deleting note {NoteId}", id);
            return DatabaseFailureMapper.Map(e, "delete", "note");
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<NoteDto, MNoteProcessFail>> LoadPreviousVersion(Guid noteId, CancellationToken ct = default)
    {
        var historyResult = await GetHistory(noteId, ct).ConfigureAwait(false);

        if (historyResult.TryPickT1(out var fail, out var history))
            return fail;

        if (history.Length == 0)
            return await LoadCurrentVersion(noteId, ct).ConfigureAwait(false);

        // History is DESC-ordered → [0] is the newest event → its OldNote is the version before the last change.
        return history[0].OldNote;
    }

    /// <inheritdoc/>
    public async Task<OneOf<UpdateEvent[], MNoteProcessFail>> GetHistory(Guid noteId, CancellationToken ct = default)
    {
        try
        {
            var events = await _eventRepository.GetAllAsync<UpdateEvent>(noteId, ct).ConfigureAwait(false);
            return events.ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while trying to get history");
            return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("get", "history"));
        }
    }

    private async Task<OneOf<NoteDto, MNoteProcessFail>> LoadCurrentVersion(Guid noteId, CancellationToken ct)
    {
        var note = await _noteRepository.GetByIdAsync(noteId, ct).ConfigureAwait(false);
        if (note is null)
            return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(noteId));

        return note.ToDto();
    }
}
