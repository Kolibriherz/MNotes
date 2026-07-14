using Microsoft.AspNetCore.Mvc;
using MNoteProvider.Common;
using MNoteProvider.Common.DTOs;
using MNoteProvider.RequestHandler;

namespace MNoteProvider.Endpoints.EndpointDefinitions;

/// <summary>
/// Registers the HTTP endpoints for comment resources.
/// </summary>
/// <remarks>
/// This class contains routing only: it maps HTTP verbs and routes onto the methods of
/// <see cref="ICommentRequestHandler"/> and holds no logic of its own. Route templates come from
/// <see cref="MNotesRoutes"/>, so that server and client refer to the same constants rather than
/// to two copies of the same string.
/// </remarks>
public class CommentEndpointDefinition : IEndpointDefinition
{
    private readonly ICommentRequestHandler _commentRequestHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentEndpointDefinition"/> class.
    /// </summary>
    /// <param name="commentRequestHandler">Request handler the registered endpoints delegate to.</param>
    public CommentEndpointDefinition(ICommentRequestHandler commentRequestHandler)
    {
        _commentRequestHandler = commentRequestHandler;
    }
    /// <inheritdoc/>
    public void AddEndpoints(WebApplication app)
    {
        app.MapGet(MNotesRoutes.Endpoints.CommentEndpoints.GetAllByNote+"/{id}", _commentRequestHandler.GetAllCommentsByNote);
        app.MapPost(MNotesRoutes.Endpoints.CommentEndpoints.Create,([FromBody] CreateCommentDto createCommentDto, CancellationToken ct) => _commentRequestHandler.CreateComment(createCommentDto,ct));
        app.MapPut(MNotesRoutes.Endpoints.CommentEndpoints.Update, ([FromBody] CommentDto commentDto, CancellationToken ct) =>_commentRequestHandler.UpdateComment(commentDto,ct));
        app.MapDelete(MNotesRoutes.Endpoints.CommentEndpoints.Delete+"/{id}", _commentRequestHandler.DeleteComment);
    }
}
