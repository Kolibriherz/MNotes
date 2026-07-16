using MNoteProvider.Endpoints.EndpointDefinitions;
using MNoteProvider.RequestHandler;

namespace MNoteProvider.Endpoints;
/// <summary>
/// Provides extension methods for configuring the endpoint layer during application startup.
/// </summary>
/// <remarks>
/// <see cref="AddEndpoints(IServiceCollection)"/> registers the request handlers and endpoint definitions
/// with the dependency injection container. <see cref="MapEndpoints(WebApplication)"/> resolves the
/// registered <see cref="IEndpointDefinition"/> implementations and maps their routes to the application.
/// This keeps endpoint registration modular and avoids collecting resource-specific routes in
/// <c>Program.cs</c>.
/// </remarks>
public static class EndpointStartupExtension
{
    /// <summary>
    /// Registers request handlers and endpoint definitions with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the endpoint-related services to.</param>
    /// <returns>The service collection, enabling further configuration through method chaining.</returns>
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        services.AddSingleton<IIsAvailableRequestHandler, IsAvailableRequestHandler>();
        services.AddSingleton<INoteRequestHandler, NoteRequestHandler>();
        services.AddSingleton<IFolderRequestHandler, FolderRequestHandler>();
        services.AddSingleton<ICommentRequestHandler, CommentRequestHandler>();
        services.AddSingleton<ITagRequestHandler, TagRequestHandler>();
        services.AddSingleton<INoteTagAssignmentRequestHandler, NoteTagAssignmentRequestHandler>();

        services.AddSingleton<IEndpointDefinition, IsAvailableEndpointDefinition>();
        services.AddSingleton<IEndpointDefinition, NoteEndpointDefinition>();
        services.AddSingleton<IEndpointDefinition, FolderEndpointDefinition>();
        services.AddSingleton<IEndpointDefinition, CommentEndpointDefinition>();
        services.AddSingleton<IEndpointDefinition, TagEndpointDefinition>();
        services.AddSingleton<IEndpointDefinition, NoteTagAssignmentEndpointDefinition>();
        return services;
    }
    /// <summary>
    /// Maps all registered endpoint definitions to the web application.
    /// </summary>
    /// <param name="app">The web application to register the endpoints on.</param>
    /// <returns>The web application, enabling further configuration through method chaining.</returns>
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var definitions = app.Services.GetRequiredService<IEnumerable<IEndpointDefinition>>();
        foreach (var definition in definitions)
        {
            definition.AddEndpoints(app);
        }

        return app;
    }
}
