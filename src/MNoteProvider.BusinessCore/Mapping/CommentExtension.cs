using MNoteProvider.Common.DTOs;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.BusinessCore.Mapping;

/// <summary>
/// Provides extension methods for mapping comment domain entities to data transfer objects.
/// </summary>
/// <remarks>
/// The mapping creates DTOs for transferring comment data across application boundaries without
/// exposing the domain entities themselves.
/// </remarks>
public static class CommentExtension
{
    /// <summary>
    /// Maps a comment domain entity to a data transfer object.
    /// </summary>
    /// <param name="comment">The comment to map.</param>
    /// <returns>A data transfer object containing the comment data.</returns>
    public static CommentDto ToDto(this IComment comment) => new CommentDto
    {
        Id = comment.Id,
        Content = comment.Content,
        NoteId = comment.NoteId,
        CreationDate = comment.CreationDate,
        ChangeDate = comment.Doeom
    };

    /// <summary>
    /// Lazily maps a sequence of comments domain entities to data transfer objects.
    /// </summary>
    /// <param name="comments">The comments to map.</param>
    /// <returns>A lazily evaluated sequence of comments data transfer objects.</returns>
    public static IEnumerable<CommentDto> ToDtos(this IEnumerable<IComment> comments) => MapToDtos(comments);
    private static IEnumerable<CommentDto> MapToDtos(IEnumerable<IComment> comments)
    {
        foreach (var comment in comments)
            yield return comment.ToDto();
    }
}
