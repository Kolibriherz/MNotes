using Microsoft.Extensions.Hosting;
using MNoteProvider.ClientService.Abstractions;

namespace MNoteProvider.ClientService.SignalRClient;

/// <summary>Starts and stops the note hub connection for the hosted application.</summary>
/// <param name="hubCon">The hub connection to start and stop.</param>
/// <param name="relay">The relay subscribed to hub events before the connection starts.</param>
internal sealed class NoteHubConnectionStarter(NoteHubCon hubCon,INoteEventRelay relay) : IHostedService
{
    /// <summary>
    /// Subscribes to note events and starts the hub connection.
    /// </summary>
    /// <param name="ct">A token used to cancel application startup.</param>
    /// <remarks>
    /// Automatic reconnect applies only after the initial connection has been
    /// established successfully. An initial connection failure is propagated
    /// so that the application does not continue with an unnoticed offline hub.
    /// </remarks>
    public async Task StartAsync(CancellationToken ct)
    {
        relay.Subscribe();

        await hubCon
            .EnsureStartedAsync(ct)
            .ConfigureAwait(false);
    }
    /// <summary>Stops the note hub connection.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    public Task StopAsync(CancellationToken ct) => hubCon.StopAsync(ct);
}
