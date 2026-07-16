namespace MNoteProvider.Domain.Abstractions;

/// <summary>
/// The domain abstraction of a folder in the hierarchical folder tree.
/// </summary>
public interface IFolder : IBaseDomainObject
{
    /// <summary>The display name of the folder.</summary>
    string Name { get; set; }
    /// <summary>The parent folder. Equals the own id only for the root folder.</summary>
    Guid ParentId { get; set; }
    /// <summary>The date the folder was created. Immutable after insert.</summary>
    DateTime CreationDate { get; set; }
}
