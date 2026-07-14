namespace MNoteProvider.Domain.Abstractions;

/// <summary>
/// The domain abstraction of a note, the core aggregate of the domain.
/// </summary>
public interface INote : IBaseDomainObject
{
    /// <summary>The display name of the note.</summary>
    string Name { get; set; }
    /// <summary>The main body text of the note.</summary>
    string Content { get; set; }
    /// <summary>A short description of the note.</summary>
    string Description { get; set; }
    /// <summary>The folder that owns this note.</summary>
    Guid FolderId { get; set; }
    /// <summary>The date the note was created. Immutable after insert.</summary>
    DateTime CreationDate { get; init; }
}
