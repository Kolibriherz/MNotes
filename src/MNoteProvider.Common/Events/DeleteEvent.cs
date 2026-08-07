using MNoteProvider.Common.Abstractions.Events;

namespace MNoteProvider.Common.Events;

/// <summary>
/// A note deletion event.
/// Persisted to the event stream and broadcast to subscribers.
/// </summary>
public class DeleteEvent : BaseEvent, IDeleteEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEvent"/> class. The publish date is set to the current UTC time.
    /// </summary>
    /// <param name="ownerId">The event owner id.</param>
    /// <param name="id">The event id; a new id is generated when omitted.</param>
    public DeleteEvent(Guid ownerId, Guid id = default) : base(ownerId, id) { }

}
