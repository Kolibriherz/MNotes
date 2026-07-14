using MNoteProvider.Common.DTOs;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.BusinessCore.Mapping;

/// <summary>
/// Provides extension methods for mapping note-tag assignment domain entities to data transfer objects.
/// </summary>
/// <remarks>
/// The mapping creates DTOs for transferring note-tag assignment data across application boundaries
/// without exposing the domain entities themselves.
/// </remarks>
public static class NoteTagAssignmentExtension
{
    /// <summary>
    /// Provides extension methods for mapping note-tag assignment domain entities to data transfer objects.
    /// </summary>
    /// <remarks>
    /// The mapping creates DTOs for transferring note-tag assignment data across application boundaries
    /// without exposing the domain entities themselves.
    /// </remarks>
    public static NoteTagAssignmentDto ToDto(this INoteTagAssignment noteTagAssignment) => new NoteTagAssignmentDto
    {
        Id = noteTagAssignment.Id,
        TagId = noteTagAssignment.TagId,
        NoteId = noteTagAssignment.NoteId,
        CreationDate = noteTagAssignment.Doeom
    };

    /// <summary>
    /// Lazily maps a sequence of note-tag assignment domain entities to data transfer objects.
    /// </summary>
    /// <param name="noteTagAssignments">The note-tag assignments to map.</param>
    /// <returns>A lazily evaluated sequence of note-tag assignment data transfer objects.</returns>
    public static IEnumerable<NoteTagAssignmentDto> ToDtos(this IEnumerable<INoteTagAssignment> noteTagAssignments) => MapToDtos(noteTagAssignments);
    private static IEnumerable<NoteTagAssignmentDto> MapToDtos(IEnumerable<INoteTagAssignment> noteTagAssignments)
    {
        foreach (var noteTagAssignment in noteTagAssignments)
            yield return noteTagAssignment.ToDto();
    }
}
