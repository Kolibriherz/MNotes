using MNoteProvider.Common;
using MNoteProvider.RequestHandler;

namespace MNoteProvider.Endpoints.EndpointDefinitions;

/// <summary>
/// Registers the HTTP endpoint used to check whether the MNote API is available.
/// </summary>
/// <remarks>
/// This class contains routing only: it maps the availability route onto
/// <see cref="IIsAvailableRequestHandler"/> and holds no logic of its own. The route template comes from
/// <see cref="MNotesRoutes"/>, so that server and client refer to the same constant rather than
/// to two copies of the same string.
/// </remarks>
public class IsAvailableEndpointDefinition : IEndpointDefinition
{
    private readonly IIsAvailableRequestHandler _isAvailableRequestHandler;
    /// <summary>
    /// Initializes a new instance of the <see cref="IsAvailableEndpointDefinition"/> class.
    /// </summary>
    /// <param name="isAvailableRequestHandler">Request handler the registered endpoint delegates to.</param>
    public IsAvailableEndpointDefinition(IIsAvailableRequestHandler isAvailableRequestHandler)
    {
        _isAvailableRequestHandler = isAvailableRequestHandler;
    }
    /// <inheritdoc/>
    public void AddEndpoints(WebApplication app)
    {
        app.MapGet(MNotesRoutes.Endpoints.IsAvailable, _isAvailableRequestHandler.IsAvailable);
    }
}
