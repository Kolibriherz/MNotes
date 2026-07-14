using MNoteProvider.BusinessCore.Provider;
using MNoteProvider.Common.DTOs;

namespace MNoteProvider.RequestHandler;

/// <summary>
/// Handles incoming HTTP requests for note resources by delegating to the business layer
/// and translating its result types into HTTP responses.
/// </summary>
/// <remarks>
/// This is the boundary between transport and domain: it is the only place where a
/// business result is turned into an <see cref="IResult"/>. Everything below this layer
/// is unaware of HTTP.
/// </remarks>
public interface INoteRequestHandler
{
    /// <summary>Gets all notes.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>An HTTP result containing the notes or an error response.</returns>
    Task<IResult> GetAllNotes(CancellationToken ct = default);

    /// <summary>Creates a new note.</summary>
    /// <param name="createNoteDto">The note data used to create the note.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>
    /// <c>201 Created</c> with the identifier of the created note, or an error
    /// response derived from the failure returned by the business layer.
    /// </returns>
    Task<IResult> CreateNote(CreateNoteDto createNoteDto, CancellationToken ct = default);

    /// <summary>Updates an existing note.</summary>
    /// <param name="noteDto">The note data used to update the note.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>An HTTP result containing a success flag or an error response.</returns>
    Task<IResult> UpdateNote(NoteDto noteDto, CancellationToken ct = default);

    /// <summary>Deletes the note with the specified identifier.</summary>
    /// <param name="id">The identifier of the note to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>An HTTP result containing a success flag or an error response.</returns>
    Task<IResult> DeleteNote(Guid id, CancellationToken ct = default);

    /// <summary>Loads the previous version of the specified note.</summary>
    /// <param name="noteId">The identifier of the note whose previous version is loaded.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>An HTTP result containing the previous note version or an error response.</returns>
    Task<IResult> LoadPreviousVersion(Guid noteId, CancellationToken ct = default);

    /// <summary>Gets the update history of the specified note.</summary>
    /// <param name="noteId">The identifier of the note whose history is loaded.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>An HTTP result containing the note update history or an error response.</returns>
    Task<IResult> GetHistory(Guid noteId, CancellationToken ct = default);
}

///<inheritdoc cref = "INoteRequestHandler" />
public class NoteRequestHandler : INoteRequestHandler
{
    private readonly INoteProvider _noteProvider;

    /// <summary>Initializes a new instance of the note request handler.</summary>
    /// <param name="noteProvider">The note provider used to process note operations.</param>
    public NoteRequestHandler(INoteProvider noteProvider)
    {
        _noteProvider = noteProvider;
    }

    /// <inheritdoc/>
    public async Task<IResult> GetAllNotes(CancellationToken ct = default)
    {
        var mNotesResult = await _noteProvider.GetAllNotes(ct).ConfigureAwait(false);
        return mNotesResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }

    /// <inheritdoc/>
    public async Task<IResult> CreateNote(CreateNoteDto createNoteDto, CancellationToken ct = default)
    {
        var mNotesResult = await _noteProvider.CreateNote(createNoteDto, ct).ConfigureAwait(false);
        return mNotesResult.Match( id => Results.Created((string?)null, id), fail => fail.ToIResult());
    }

    /// <inheritdoc/>
    public async Task<IResult> UpdateNote(NoteDto noteDto, CancellationToken ct = default)
    {
        var mNotesResult = await _noteProvider.UpdateNote(noteDto, ct).ConfigureAwait(false);
        return mNotesResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }

    /// <inheritdoc/>
    public async Task<IResult> DeleteNote(Guid id, CancellationToken ct = default)
    {
        var mNotesResult = await _noteProvider.DeleteNote(id, ct).ConfigureAwait(false);
        return mNotesResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }

    /// <inheritdoc/>
    public async Task<IResult> LoadPreviousVersion(Guid noteId, CancellationToken ct = default)
    {
        var mNotesResult = await _noteProvider.LoadPreviousVersion(noteId, ct).ConfigureAwait(false);
        return mNotesResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }

    /// <inheritdoc/>
    public async Task<IResult> GetHistory(Guid noteId, CancellationToken ct = default)
    {
        var mNotesResult = await _noteProvider.GetHistory(noteId, ct).ConfigureAwait(false);
        return mNotesResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
}
