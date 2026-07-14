namespace MNoteProvider.Endpoints;
/// <summary>
/// Registers a group of related HTTP endpoints on the application.
/// </summary>
/// <remarks>
/// Each resource defines its routes in a dedicated implementation, keeping
/// resource-specific endpoint mappings out of <c>Program.cs</c>.
/// Implementations are registered explicitly and resolved during startup.
/// </remarks>
public interface IEndpointDefinition
{
    /// <summary>
    /// Maps this group's routes onto the request handler it delegates to.
    /// </summary>
    /// <param name="app">Application the routes are registered on.</param>
    /// <remarks>
    /// Called once during startup, before the application begins accepting requests. Implementations
    /// perform routing only and hold no logic of their own.
    /// </remarks>
    void AddEndpoints(WebApplication app);
}
