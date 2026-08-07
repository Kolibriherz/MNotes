using MNoteProvider.ClientService.Abstractions;
using MNoteProvider.Common.DTOs;
namespace MNoteProvider.ClientService.SignalRClient;

/// <summary>Relays note hub events as client-side note notifications.</summary>
internal sealed class NoteEventRelay : INoteEventRelay
{

    /// <inheritdoc/>
    public event Action<NoteDto>? NoteCreatedNotification;

    /// <inheritdoc/>
    public event Action<NoteDto>? NoteUpdatedNotification;

    /// <inheritdoc/>
    public event Action<Guid>? NoteDeletedNotification;

    /// <summary>Initializes a new instance of the note event relay.</summary>
    public NoteEventRelay()
    {

    }

    public void SendNoteCreated(NoteDto noteDto) => NoteCreatedNotification?.Invoke(noteDto);
    public void SendNoteUpdated(NoteDto noteDto) => NoteUpdatedNotification?.Invoke(noteDto);
    public void SendNoteDeleted(Guid noteId) => NoteDeletedNotification?.Invoke(noteId);
}

