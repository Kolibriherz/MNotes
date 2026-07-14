using Microsoft.AspNetCore.SignalR;
using MNoteProvider.Common;
using MNoteProvider.Common.Abstractions.DTOs;
using MNoteProvider.Common.Abstractions.Events;
using MNoteProvider.DataAccess.Repositories;

namespace MNoteProvider.Hubs;

/// <summary>
/// Publishes note lifecycle events to connected SignalR clients and stores note update events
/// in the event stream.
/// </summary>
/// <param name="hub">The SignalR hub context used to publish events to connected clients.</param>
/// <param name="eventRepository">The repository used to persist note update events.</param>
/// <remarks>
/// This implementation bridges the business layer and SignalR without requiring the business
/// layer to depend on SignalR directly. It is accessed through <see cref="INoteEventPublisher"/>.
/// </remarks>
public sealed class SignalRNoteEventPublisher(IHubContext<NoteHub> hub, IEventstreamRepository eventRepository) : INoteEventPublisher
{
    /// <summary>
    /// Publishes a notification that a note has been created.
    /// </summary>
    /// <param name="note">The newly created note.</param>
    /// <param name="ct">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    public async Task PublishCreatedAsync(INoteDto note, CancellationToken ct = default)
        => await hub.Clients.All.SendAsync(MNotesRoutes.Hubs.MethodNames.NoteCreated, note, ct).ConfigureAwait(false);

    /// <summary>
    /// Publishes a notification that a note has been deleted.
    /// </summary>
    /// <param name="noteId">The identifier of the deleted note.</param>
    /// <param name="ct">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    public async Task PublishDeletedAsync(Guid noteId, CancellationToken ct = default)
        => await hub.Clients.All.SendAsync(MNotesRoutes.Hubs.MethodNames.NoteDeleted, noteId, ct).ConfigureAwait(false);

    /// <summary>
    /// Stores a note update event and publishes a notification containing the updated note.
    /// </summary>
    /// <typeparam name="T">The type of the updated note.</typeparam>
    /// <param name="noteEvent">The event containing the previous and updated state of the note.</param>
    /// <param name="ct">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous persistence and publishing operation.</returns>
    public async Task PublishUpdatedAsync<T>(IUpdateEvent<T> noteEvent, CancellationToken ct = default) where T : INoteDto
    {
        await eventRepository.CreateAsync(noteEvent, "note", ct).ConfigureAwait(false);
        await hub.Clients.All.SendAsync(MNotesRoutes.Hubs.MethodNames.NoteUpdated, noteEvent.NewNote, ct);
    }
}
