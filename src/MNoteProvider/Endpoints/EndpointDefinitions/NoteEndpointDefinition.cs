using Microsoft.AspNetCore.Mvc;
using MNoteProvider.Common;
using MNoteProvider.Common.DTOs;
using MNoteProvider.RequestHandler;

namespace MNoteProvider.Endpoints.EndpointDefinitions;
/// <summary>
/// Registers the HTTP endpoints for note resources.
/// </summary>
/// <remarks>
/// This class contains routing only: it maps HTTP verbs and routes onto the methods of
/// <see cref="INoteRequestHandler"/> and holds no logic of its own. Route templates come from
/// <see cref="MNotesRoutes"/>, so that server and client refer to the same constants rather than
/// to two copies of the same string.
/// </remarks>
public class NoteEndpointDefinition : IEndpointDefinition
{
    private readonly INoteRequestHandler _noteRequestHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteEndpointDefinition"/> class.
    /// </summary>
    /// <param name="noteRequestHandler">Request handler the registered endpoints delegate to.</param>
    public NoteEndpointDefinition(INoteRequestHandler noteRequestHandler)
    {
        _noteRequestHandler = noteRequestHandler;
    }
    /// <inheritdoc/>
    public void AddEndpoints(WebApplication app)
    {
         app.MapGet(MNotesRoutes.Endpoints.NoteEndpoints.GetAll, _noteRequestHandler.GetAllNotes);
         app.MapPost(MNotesRoutes.Endpoints.NoteEndpoints.Create,([FromBody] CreateNoteDto createNoteDto, CancellationToken ct) => _noteRequestHandler.CreateNote(createNoteDto,ct));
         app.MapPut(MNotesRoutes.Endpoints.NoteEndpoints.Update, ([FromBody] NoteDto noteDto, CancellationToken ct)=>_noteRequestHandler.UpdateNote(noteDto,ct));
         app.MapDelete(MNotesRoutes.Endpoints.NoteEndpoints.Delete+"/{id}", _noteRequestHandler.DeleteNote);
         app.MapGet(MNotesRoutes.Endpoints.NoteEndpoints.LoadPreviousVersion+"/{noteId}", _noteRequestHandler.LoadPreviousVersion);
         app.MapGet(MNotesRoutes.Endpoints.NoteEndpoints.GetHistory+"/{noteId}", _noteRequestHandler.GetHistory);
    }
}
