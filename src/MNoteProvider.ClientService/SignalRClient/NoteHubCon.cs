using Microsoft.AspNetCore.SignalR.Client;
using MNoteProvider.Common;
using MNoteProvider.Common.DTOs;

namespace MNoteProvider.ClientService.SignalRClient;
/// <summary>Provides configuration options for the note hub connection.</summary>
public sealed class NoteHubConOptions
{
    /// <summary>Gets the SignalR hub address used for note notifications.</summary>
    public required Uri HubAddress { get; init; }
}

/// <summary>Manages the SignalR connection for receiving note events.</summary>
public sealed class NoteHubCon : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;

    /// <summary>Occurs when a note has been created.</summary>
    public event Action<NoteDto>? NoteCreated;

    /// <summary>Occurs when a note has been updated.</summary>
    public event Action<NoteDto>? NoteUpdated;

    /// <summary>Occurs when a note has been deleted.</summary>
    public event Action<Guid>? NoteDeleted;

    /// <summary>Initializes a new instance of the note hub connection.</summary>
    /// <param name="options">The options used to configure the hub connection.</param>
    public NoteHubCon(NoteHubConOptions options)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(options.HubAddress)
            .WithAutomaticReconnect()
            .Build();
        ConfigureListeners();
    }
    /// <summary>Starts the hub connection if it is currently disconnected.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    public async Task EnsureStartedAsync(CancellationToken ct = default)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync(ct).ConfigureAwait(false);
    }
    /// <summary>Stops the hub connection.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    public Task StopAsync(CancellationToken ct = default) => _hubConnection.StopAsync(ct);

    /// <summary>Stops and disposes the hub connection.</summary>
    public async ValueTask DisposeAsync()
    {
        await _hubConnection.StopAsync().ConfigureAwait(false);
        await _hubConnection.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private void ConfigureListeners()
    {
        _hubConnection.On<NoteDto>(MNotesRoutes.Hubs.MethodNames.NoteCreated, dto => NoteCreated?.Invoke(dto));
        _hubConnection.On<NoteDto>(MNotesRoutes.Hubs.MethodNames.NoteUpdated, dto => NoteUpdated?.Invoke(dto));
        _hubConnection.On<Guid>(MNotesRoutes.Hubs.MethodNames.NoteDeleted, id => NoteDeleted?.Invoke(id));
    }

}
