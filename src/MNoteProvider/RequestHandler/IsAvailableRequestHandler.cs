namespace MNoteProvider.RequestHandler;


/// <summary>
/// Handles the liveness probe of the service.
/// </summary>
/// <remarks>
/// Answers the question "is this process running and able to serve requests?" — nothing more.
/// It deliberately performs no work: it touches neither the database nor any other dependency,
/// so a positive answer says only that the HTTP pipeline is up.
/// </remarks>
public interface IIsAvailableRequestHandler
{
    /// <summary>
    /// Reports that the service is running.
    /// </summary>
    /// <param name="ct">Token used to cancel the operation. Accepted for signature consistency with the other request handlers; the operation completes synchronously and never observes it.</param>
    /// <returns><c>200 OK</c>. This method has no failure path — if the process is unable to answer, the caller sees a connection error instead.</returns>
    Task<IResult> IsAvailable(CancellationToken ct = default);
}
///<inheritdoc cref = "IIsAvailableRequestHandler" />
public class IsAvailableRequestHandler : IIsAvailableRequestHandler
{
    /// <inheritdoc/>
    public Task<IResult> IsAvailable(CancellationToken cts = default) => Task.FromResult(Results.Ok(true));
}
