namespace MNoteProvider.Common.Abstractions.DTOs;

/// <summary>
/// The data transfer contract of a note-tag assignment.
/// </summary>
public interface INoteTagAssignmentDto
{
    /// <summary>The unique identifier of the assignment.</summary>
    Guid Id { get; init; }
    /// <summary>The assigned tag.</summary>
    Guid TagId { get; set; }
    /// <summary>The note the tag is assigned to.</summary>
    Guid NoteId { get; set; }
    /// <summary>The date the assignment was created.</summary>
    DateTime CreationDate { get; init; }
}


/// <summary>
/// The data required to assign a tag to a note or to remove such an assignment.
/// </summary>
public interface IAssignmentDto
{
    /// <summary>The tag to assign or unassign.</summary>
    Guid TagId { get; set; }
    /// <summary>The note the tag is assigned to or removed from.</summary>
    Guid NoteId { get; set; }
}