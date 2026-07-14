namespace MNoteProvider.Domain.Abstractions;

/// <summary>
/// The domain abstraction of a note-tag assignment, resolving the many-to-many
/// relationship between notes and tags. A (tag, note) pair is unique.
/// </summary>
public interface INoteTagAssignment : IBaseDomainObject
{
    /// <summary>The assigned tag.</summary>
    Guid TagId { get; set; }
    /// <summary>The note the tag is assigned to.</summary>
    Guid NoteId { get; set; }
}
