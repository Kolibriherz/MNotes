namespace MNoteProvider.Common.Abstractions.Events;

/// <summary>
/// Base contract of all domain events stored in the event stream.
/// </summary>
public interface IBaseEvent
{
    /// <summary>The unique identifier of the event.</summary>
    Guid Id { get;  set; }
    /// <summary>The id of the entity the event belongs to. Events outlive their owner, so this is deliberately no foreign key.</summary>
    Guid OwnerId { get;  set; }
    /// <summary>The date the event was published.</summary>
    DateTime PublishDate { get; set; }

}