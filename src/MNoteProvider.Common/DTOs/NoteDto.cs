using MNoteProvider.Common.Abstractions.DTOs;

namespace MNoteProvider.Common.DTOs;

/// <summary>
/// Data transfer record of a note.
/// </summary>
public record NoteDto : INoteDto
{
    /// <inheritdoc/>
    public Guid Id { get; init; }
    /// <inheritdoc/>
    public required string Name { get; set; }
    /// <inheritdoc/>
    public string Content { get; set; } = string.Empty;
    /// <inheritdoc/>
    public string Description { get; set; } = string.Empty;
    /// <inheritdoc/>
    public Guid FolderId { get; set; }
    /// <inheritdoc/>
    public DateTime CreationDate { get; init; }
    /// <inheritdoc/>
    public DateTime ChangeDate { get; set; }
}

/// <summary>
/// Data transfer record carrying the data required to create a new note.
/// </summary>
public record CreateNoteDto : ICreateNoteDto
{
    /// <inheritdoc/>
    public required string Name { get; set; }
    /// <inheritdoc/>
    public Guid FolderId { get; set; }
}