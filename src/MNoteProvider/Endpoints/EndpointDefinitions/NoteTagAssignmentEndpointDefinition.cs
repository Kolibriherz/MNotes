using Microsoft.AspNetCore.Mvc;
using MNoteProvider.Common;
using MNoteProvider.Common.DTOs;
using MNoteProvider.RequestHandler;

namespace MNoteProvider.Endpoints.EndpointDefinitions;
/// <summary>
/// Registers the HTTP endpoints for note tag assignment resources.
/// </summary>
/// <remarks>
/// This class contains routing only: it maps HTTP verbs and routes onto the methods of
/// <see cref="INoteTagAssignmentRequestHandler"/> and holds no logic of its own. Route templates
/// come from <see cref="MNotesRoutes"/>, so that server and client refer to the same constants
/// rather than to two copies of the same string.
/// </remarks>
public class NoteTagAssignmentEndpointDefinition : IEndpointDefinition
{
    private readonly INoteTagAssignmentRequestHandler _noteTagAssignmentRequestHandler;
    /// <summary>
    /// Initializes a new instance of the <see cref="NoteTagAssignmentEndpointDefinition"/> class.
    /// </summary>
    /// <param name="noteTagAssignmentRequestHandler">Request handler the registered endpoints delegate to.</param>
    public NoteTagAssignmentEndpointDefinition(INoteTagAssignmentRequestHandler noteTagAssignmentRequestHandler)
    {
        _noteTagAssignmentRequestHandler = noteTagAssignmentRequestHandler;
    }
    /// <inheritdoc/>
    public void AddEndpoints(WebApplication app)
    {
        app.MapGet(MNotesRoutes.Endpoints.NoteTagAssignmentEndpoints.GetAll, _noteTagAssignmentRequestHandler.GetAllNoteTagAssignments);
        app.MapPost(MNotesRoutes.Endpoints.NoteTagAssignmentEndpoints.Assign,([FromBody] AssignmentDto assignmentDto, CancellationToken ct) => _noteTagAssignmentRequestHandler.AssignTag(assignmentDto,ct));
        app.MapDelete(MNotesRoutes.Endpoints.NoteTagAssignmentEndpoints.Unassign+"/{noteId}/{tagId}", _noteTagAssignmentRequestHandler.UnassignTag);
    }
}
