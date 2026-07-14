namespace MNoteProvider.Domain.Abstractions;

/// <summary>
/// The domain abstraction of a tag, a free-form label attachable to any number of notes.
/// </summary>
public interface ITag : IBaseDomainObject
{
    /// <summary>The display name of the tag. Names are globally unique.</summary>
    string Name { get; set; }
}
