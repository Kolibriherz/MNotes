using MNoteProvider.Common.Abstractions.Events;

namespace MNoteProvider.Common.Events;

/// <inheritdoc cref="IBaseEvent"/>
public class BaseEvent : IBaseEvent
{
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <inheritdoc/>
    public Guid OwnerId { get; set; }

    /// <inheritdoc/>
    public DateTime PublishDate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEvent"/> class. The publish date is set to the current UTC time.
    /// </summary>
    /// <param name="ownerId">The event owner id.</param>
    /// <param name="id">The event id; a new id is generated when omitted.</param>
    public BaseEvent(Guid ownerId, Guid id = default)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        OwnerId = ownerId;
        PublishDate = DateTime.UtcNow;

    }
}
