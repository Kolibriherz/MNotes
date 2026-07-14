using Microsoft.AspNetCore.Mvc;
using MNoteProvider.Common;
using MNoteProvider.RequestHandler;

namespace MNoteProvider.Endpoints.EndpointDefinitions;
/// <summary>
/// Registers the HTTP endpoints for tag resources.
/// </summary>
/// <remarks>
/// This class contains routing only: it maps HTTP verbs and routes onto the methods of
/// <see cref="ITagRequestHandler"/> and holds no logic of its own. Route templates come from
/// <see cref="MNotesRoutes"/>, so that server and client refer to the same constants rather than
/// to two copies of the same string.
/// </remarks>
public class TagEndpointDefinition : IEndpointDefinition
{
    private readonly ITagRequestHandler _tagRequestHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagEndpointDefinition"/> class.
    /// </summary>
    /// <param name="tagRequestHandler">Request handler the registered endpoints delegate to.</param>
    public TagEndpointDefinition(ITagRequestHandler tagRequestHandler)
    {
        _tagRequestHandler = tagRequestHandler;
    }
    /// <inheritdoc/>
    public void AddEndpoints(WebApplication app)
    {
        app.MapGet(MNotesRoutes.Endpoints.TagEndpoints.GetAll, _tagRequestHandler.GetAllTags);
        app.MapPost(MNotesRoutes.Endpoints.TagEndpoints.Create,([FromBody] string name, CancellationToken ct) => _tagRequestHandler.CreateTag(name,ct));
        app.MapDelete(MNotesRoutes.Endpoints.TagEndpoints.Delete+"/{id}", _tagRequestHandler.DeleteTag);
    }
  
}
