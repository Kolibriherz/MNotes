using MNoteProvider.Common.Abstractions.DTOs;

namespace MNoteProvider.Common.DTOs;

/// <summary>
/// Data transfer record of a comment on a note.
/// </summary>
public record CommentDto : ICommentDto
{
    /// <inheritdoc/>
    public Guid Id { get; init; }
    /// <inheritdoc/>
    public string Content { get; set; } = string.Empty;
    /// <inheritdoc/>
    public Guid NoteId { get; set; }
    /// <inheritdoc/>
    public DateTime CreationDate { get; init; }
    /// <inheritdoc/>
    public DateTime ChangeDate { get; set; }
}

/// <summary>
/// Data transfer record carrying the data required to create a new comment.
/// </summary>
public record CreateCommentDto : ICreateCommentDto
{
    /// <inheritdoc/>
    public string Content { get; set; } = string.Empty;
    /// <inheritdoc/>
    public Guid NoteId { get; set; }
}
