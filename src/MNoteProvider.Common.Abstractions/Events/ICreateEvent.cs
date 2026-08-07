using MNoteProvider.Common.Abstractions.DTOs;

namespace MNoteProvider.Common.Abstractions.Events;

/// <summary>A domain event describing the creation of a note, carrying the state after creation.</summary>
/// <typeparam name="T">The concrete note DTO type carried by the event.</typeparam>
public interface ICreateEvent<T> : IBaseEvent where T : INoteDto
{
    /// <summary>The state of the new note.</summary>
    T NewNote { get; set; }
}
