namespace MNoteProvider.Common.Abstractions.DTOs;

/// <summary>
/// The data transfer contract of a comment on a note.
/// </summary>
public interface ICommentDto
{
    /// <summary>The unique identifier of the comment.</summary>
    Guid Id { get; init; }
    /// <summary>The body text of the comment.</summary>
    string Content { get; set; }
    /// <summary>The note this comment belongs to.</summary>
    Guid NoteId { get; set; }
    /// <summary>The date the comment was created.</summary>
    DateTime CreationDate { get; init; }
    /// <summary>The date of the last change.</summary>
    DateTime ChangeDate { get; set; }
}

/// <summary>
/// The data required to create a new comment.
/// </summary>
public interface ICreateCommentDto
{
    /// <summary>The body text of the new comment.</summary>
    string Content { get; set; }
    /// <summary>The note the new comment is attached to.</summary>
    Guid NoteId { get; set; }
}
