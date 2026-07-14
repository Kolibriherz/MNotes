using MNoteProvider.Common.Abstractions.DTOs;

namespace MNoteProvider.Common.DTOs;

/// <summary>
/// Data transfer record of a folder.
/// </summary>
public record FolderDto : IFolderDto
{
    /// <inheritdoc/>
    public Guid Id { get; init; }
    /// <inheritdoc/>
    public required string Name { get; set; }
    /// <inheritdoc/>
    public Guid ParentId { get; set; }
    /// <inheritdoc/>
    public DateTime CreationDate { get; init; }
    /// <inheritdoc/>
    public DateTime ChangeDate { get; set; }
}

/// <summary>
/// Data transfer record carrying the data required to create a new folder.
/// </summary>
public record CreateFolderDto : ICreateFolderDto
{
    /// <inheritdoc/>
    public required string Name { get; set; }
    /// <inheritdoc/>
    public Guid ParentId { get; set; }
}