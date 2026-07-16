using MNoteProvider.Common;

namespace MNoteProvider.Hubs;

/// <summary>
/// Provides extension methods for mapping SignalR hubs to the web application.
/// </summary>
/// <remarks>
/// Hub routes are defined centrally in <see cref="MNotesRoutes"/>, so that the server and its
/// clients refer to the same route constants.
/// </remarks>
public static class HubRegistrationExtension
{
    /// <summary>
    /// Maps the application's SignalR hubs to the web application.
    /// </summary>
    /// <param name="app">The web application to register the hubs on.</param>
    /// <returns>The web application, enabling further configuration through method chaining.</returns>
    public static WebApplication MapHubs(this WebApplication app)
    {
        app.MapHub<NoteHub>(MNotesRoutes.Hubs.Name);

        return app;
    }
}
