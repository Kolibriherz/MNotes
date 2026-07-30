using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MNoteProvider.ClientService.SignalRClient;

/// <summary>Starts and stops the note hub connection for the hosted application.</summary>
/// <param name="hubCon">The hub connection to start and stop.</param>
/// <param name="logger">The logger used to record connection lifecycle events.</param>
internal sealed class NoteHubConnectionStarter(NoteHubCon hubCon, ILogger<NoteHubConnectionStarter> logger) : IHostedService
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
        logger.LogInformation("Starting note hub connection.");

        logger.LogDebug("Note event relay subscribed to hub events.");

        await hubCon
            .EnsureStartedAsync(ct)
            .ConfigureAwait(false);

        logger.LogInformation("Note hub connection started.");
    }
    /// <summary>Stops the note hub connection.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    public async Task StopAsync(CancellationToken ct)
    {
        logger.LogInformation("Stopping note hub connection.");

        await hubCon
            .StopAsync(ct)
            .ConfigureAwait(false);

        logger.LogInformation("Note hub connection stopped.");
    }

}
