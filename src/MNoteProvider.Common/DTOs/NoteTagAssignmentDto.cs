using MNoteProvider.Common.Abstractions.DTOs;

namespace MNoteProvider.Common.DTOs;

/// <summary>
/// Data transfer record of a note-tag assignment.
/// </summary>
public record NoteTagAssignmentDto : INoteTagAssignmentDto
{
    /// <inheritdoc/>
    public Guid Id { get; init; }
    /// <inheritdoc/>
    public Guid TagId { get; set; }
    /// <inheritdoc/>
    public Guid NoteId { get; set; }
    /// <inheritdoc/>
    public DateTime CreationDate { get; init; }
}


/// <summary>
/// Data transfer record used to assign a tag to a note or to remove such an assignment.
/// </summary>
public record AssignmentDto : IAssignmentDto
{
    /// <inheritdoc/>
    public Guid TagId { get; set; }
    /// <inheritdoc/>
    public Guid NoteId { get; set; }
}
