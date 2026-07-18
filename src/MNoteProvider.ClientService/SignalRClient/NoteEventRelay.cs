using MNoteProvider.ClientService.Abstractions;
using MNoteProvider.Common.DTOs;
namespace MNoteProvider.ClientService.SignalRClient;

/// <summary>Relays note hub events as client-side note notifications.</summary>
internal sealed class NoteEventRelay : INoteEventRelay, IDisposable
{
    private readonly NoteHubCon _hubCon;

    /// <inheritdoc/>
    public event Action<NoteDto>? NoteCreatedNotification;

    /// <inheritdoc/>
    public event Action<NoteDto>? NoteUpdatedNotification;

    /// <inheritdoc/>
    public event Action<Guid>? NoteDeletedNotification;

    /// <summary>Initializes a new instance of the note event relay.</summary>
    /// <param name="hubCon">The note hub connection that provides note events.</param>
    public NoteEventRelay(NoteHubCon hubCon) => _hubCon = hubCon;

    /// <summary>Subscribes to note events from the hub connection.</summary>
    internal void Subscribe()
    {
        _hubCon.NoteCreated += OnNoteCreated;
        _hubCon.NoteUpdated += OnNoteUpdated;
        _hubCon.NoteDeleted += OnNoteDeleted;
    }

    private void OnNoteCreated(NoteDto dto) => NoteCreatedNotification?.Invoke(dto);
    private void OnNoteUpdated(NoteDto dto) => NoteUpdatedNotification?.Invoke(dto);
    private void OnNoteDeleted(Guid id) => NoteDeletedNotification?.Invoke(id);

    /// <summary>Detaches all handlers from the underlying hub connection.</summary>
    public void Dispose()
    {
        _hubCon.NoteCreated -= OnNoteCreated;
        _hubCon.NoteUpdated -= OnNoteUpdated;
        _hubCon.NoteDeleted -= OnNoteDeleted;
        GC.SuppressFinalize(this);
    }
}

