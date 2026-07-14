using Microsoft.AspNetCore.Mvc;
using MNoteProvider.Common;
using MNoteProvider.Common.DTOs;
using MNoteProvider.RequestHandler;

namespace MNoteProvider.Endpoints.EndpointDefinitions;

/// <summary>
/// Registers the HTTP endpoints for folder resources.
/// </summary>
/// <remarks>
/// This class contains routing only: it maps HTTP verbs and routes onto the methods of
/// <see cref="IFolderRequestHandler"/> and holds no logic of its own. Route templates come from
/// <see cref="MNotesRoutes"/>, so that server and client refer to the same constants rather than
/// to two copies of the same string.
/// </remarks>
public class FolderEndpointDefinition : IEndpointDefinition
{
    private readonly IFolderRequestHandler _folderRequestHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderEndpointDefinition"/> class.
    /// </summary>
    /// <param name="folderRequestHandler">Request handler the registered endpoints delegate to.</param>
    public FolderEndpointDefinition(IFolderRequestHandler folderRequestHandler)
    {
        _folderRequestHandler = folderRequestHandler;
    }
    /// <inheritdoc/>
    public void AddEndpoints(WebApplication app)
    {
         app.MapGet(MNotesRoutes.Endpoints.FolderEndpoints.GetAll, _folderRequestHandler.GetAllFolders);
         app.MapPost(MNotesRoutes.Endpoints.FolderEndpoints.Create,([FromBody] CreateFolderDto createFolderDto, CancellationToken ct) => _folderRequestHandler.CreateFolder(createFolderDto,ct));
         app.MapPut(MNotesRoutes.Endpoints.FolderEndpoints.Update, ([FromBody] FolderDto folderDto, CancellationToken ct) =>_folderRequestHandler.UpdateFolder(folderDto,ct));
         app.MapDelete(MNotesRoutes.Endpoints.FolderEndpoints.Delete+"/{id}", _folderRequestHandler.DeleteFolder);
    }
}
