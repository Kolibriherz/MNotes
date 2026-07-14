namespace MNoteProvider.Common.Abstractions.DTOs;

/// <summary>
/// The data transfer contract of a folder.
/// </summary>
public interface IFolderDto
{
    /// <summary>The unique identifier of the folder.</summary>
    Guid Id { get; init; }
    /// <summary>The display name of the folder.</summary>
    string Name { get; set; }
    /// <summary>The parent folder. Equals the own id only for the root folder.</summary>
    Guid ParentId { get; set; }
    /// <summary>The date the folder was created.</summary>
    DateTime CreationDate { get; init; }
    /// <summary>The date of the last change.</summary>
    DateTime ChangeDate { get; set; }
}

/// <summary>
/// The data required to create a new folder.
/// </summary>
public interface ICreateFolderDto
{
    /// <summary>The display name of the new folder.</summary>
    string Name { get; set; }
    /// <summary>The folder the new folder is created under.</summary>
    Guid ParentId { get; set; }
}