using MNoteProvider.Common.Abstractions.DTOs;

namespace MNoteProvider.Common.Abstractions.Events;

/// <summary>
/// A domain event describing the update of a note, carrying the state before and after the change.
/// </summary>
/// <typeparam name="T">The concrete note DTO type carried by the event.</typeparam>
public interface IUpdateEvent<T> :IBaseEvent where T : INoteDto
{
    /// <summary>The state of the note immediately before the change.</summary>
    T OldNote { get;  set; }
    /// <summary>The state of the note after the change.</summary>
    T NewNote { get;  set; }
}