using MNoteProvider.Common.DTOs;

namespace MNoteProvider.ClientService.Abstractions;

/// <summary>Relays note hub events as client-side note notifications.</summary>
public interface INoteEventRelay
{
    /// <summary>Occurs when a note has been created.</summary>
    event Action<NoteDto>? NoteCreatedNotification;

    /// <summary>Occurs when a note has been updated.</summary>
    event Action<NoteDto>? NoteUpdatedNotification;

    /// <summary>Occurs when a note has been deleted.</summary>
    event Action<Guid>? NoteDeletedNotification;

    /// <summary>Subscribes to note events from the hub connection.</summary>
    void Subscribe();
}
