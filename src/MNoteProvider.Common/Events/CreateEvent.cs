using MNoteProvider.Common.Abstractions.Events;
using MNoteProvider.Common.DTOs;

namespace MNoteProvider.Common.Events;

/// <summary>
/// A note creation event carrying the state of the new note.
/// Persisted to the event stream and broadcast to subscribers.
/// </summary>
public class CreateEvent : BaseEvent, ICreateEvent<NoteDto>
{
    /// <inheritdoc/>
    public NoteDto NewNote { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEvent"/> class. The owner is
    /// taken from the new note state and the publish date is set to the current UTC time.
    /// </summary>
    /// <param name="newNote">The state of the new note.</param>
    /// <param name="id">The event id; a new id is generated when omitted.</param>
    public CreateEvent(NoteDto newNote, Guid id = default) : base(newNote.Id, id)
    {
        NewNote = newNote;
    }
}
