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
    /// Stores a note create event and publishes a notification containing the new note.
    /// </summary>
    /// <typeparam name="T">The type of the created note.</typeparam>
    /// <param name="noteEvent">The event containing the state of the new note.</param>
    /// <param name="ct">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous persistence and publishing operation.</returns>
    public async Task PublishCreatedAsync<T>(ICreateEvent<T> noteEvent, CancellationToken ct = default) where T : INoteDto
    {
        await eventRepository.CreateAsync(noteEvent, "note", ct).ConfigureAwait(false);
        await hub.Clients.All.SendAsync(MNotesRoutes.Hubs.MethodNames.NoteCreated, noteEvent.NewNote, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a notification that a note has been deleted.
    /// </summary>
    /// <param name="noteEvent">The event containing the identifier of the deleted note.</param>
    /// <param name="ct">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    public async Task PublishDeletedAsync(IDeleteEvent noteEvent, CancellationToken ct = default)
    {
        await eventRepository.CreateAsync(noteEvent, "note", ct).ConfigureAwait(false);
        await hub.Clients.All.SendAsync(MNotesRoutes.Hubs.MethodNames.NoteDeleted, noteEvent.OwnerId, ct).ConfigureAwait(false);
    }

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
        await hub.Clients.All.SendAsync(MNotesRoutes.Hubs.MethodNames.NoteUpdated, noteEvent.NewNote, ct).ConfigureAwait(false);
    }



}
