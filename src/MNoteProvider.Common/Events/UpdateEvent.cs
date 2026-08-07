using MNoteProvider.Common.Abstractions.Events;
using MNoteProvider.Common.DTOs;

namespace MNoteProvider.Common.Events;

/// <summary>
/// A note update event carrying the state of a note before and after a change.
/// Persisted to the event stream and broadcast to subscribers.
/// </summary>
public class UpdateEvent : BaseEvent, IUpdateEvent<NoteDto>
{
    /// <inheritdoc/>
    public NoteDto OldNote { get; set; }

    /// <inheritdoc/>
    public NoteDto NewNote { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateEvent"/> class. The owner is
    /// taken from the new note state and the publish date is set to the current UTC time.
    /// </summary>
    /// <param name="oldNote">The state of the note immediately before the change.</param>
    /// <param name="newNote">The state of the note after the change.</param>
    /// <param name="id">The event id; a new id is generated when omitted.</param>
    public UpdateEvent(NoteDto oldNote, NoteDto newNote, Guid id = default) : base(newNote.Id, id)
    {
        OldNote = oldNote;
        NewNote = newNote;
    }
}
