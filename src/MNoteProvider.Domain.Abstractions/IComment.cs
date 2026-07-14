namespace MNoteProvider.Domain.Abstractions;

/// <summary>
/// The domain abstraction of a comment on a note. A comment cannot exist without its note.
/// </summary>
public interface IComment : IBaseDomainObject
{

    /// <summary>The body text of the comment.</summary>
    string Content { get; set; }
    /// <summary>The note this comment belongs to.</summary>
    Guid NoteId { get; set; }
    /// <summary>The date the comment was created. Immutable after insert.</summary>
    DateTime CreationDate { get; set; }

}
