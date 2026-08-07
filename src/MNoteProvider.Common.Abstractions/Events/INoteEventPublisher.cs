using MNoteProvider.Common.Abstractions.DTOs;

namespace MNoteProvider.Common.Abstractions.Events;

/// <summary>
/// Publishes note lifecycle events to interested subscribers. The business layer depends
/// on this abstraction only; the concrete transport (e.g. SignalR) is supplied by the host.
/// </summary>
public interface INoteEventPublisher
{
    /// <summary>Publishes a note-created event for the given note.</summary>
    /// <typeparam name="T">The concrete note DTO type carried by the event.</typeparam>
    /// <param name="noteEvent">The creation event holding the new note state.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    Task PublishCreatedAsync<T>(ICreateEvent<T> noteEvent, CancellationToken ct = default) where T : INoteDto;

    /// <summary>Publishes a note-deleted event for the given note id.</summary>
    /// <param name="noteEvent">The deletion event containing the identifier of the note that was deleted.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    Task PublishDeletedAsync(IDeleteEvent noteEvent, CancellationToken ct = default);

    /// <summary>Publishes a note-updated event carrying the previous and the new state of the note.</summary>
    /// <typeparam name="T">The concrete note DTO type carried by the event.</typeparam>
    /// <param name="noteEvent">The update event holding the old and the new note state.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    Task PublishUpdatedAsync<T>(IUpdateEvent<T> noteEvent, CancellationToken ct = default) where T : INoteDto;
}

