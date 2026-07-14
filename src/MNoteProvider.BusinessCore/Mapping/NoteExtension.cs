using MNoteProvider.Common.DTOs;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.BusinessCore.Mapping;

/// <summary>
/// Provides extension methods for mapping note domain entities to data transfer objects.
/// </summary>
/// <remarks>
/// The mapping creates DTOs for transferring note data across application boundaries without
/// exposing the domain entities themselves.
/// </remarks>
public static class NoteExtension
{
    /// <summary>
    /// Maps a note domain entity to a data transfer object.
    /// </summary>
    /// <param name="note">The note to map.</param>
    /// <returns>A data transfer object containing the note data.</returns>
    public static NoteDto ToDto(this INote note) => new NoteDto()
    {
        Id = note.Id,
        Name = note.Name,
        Content = note.Content,
        Description = note.Description,
        CreationDate = note.CreationDate,
        FolderId = note.FolderId,
        ChangeDate = note.Doeom,
    };
    /// <summary>
    /// Lazily maps a sequence of notes domain entities to data transfer objects.
    /// </summary>
    /// <param name="notes">The notes to map.</param>
    /// <returns>A lazily evaluated sequence of notes data transfer objects.</returns>
    public static IEnumerable<NoteDto> ToDtos(this IEnumerable<INote> notes) => MapToDtos(notes);
    private static IEnumerable<NoteDto> MapToDtos(IEnumerable<INote> notes)
    {
        foreach (var note in notes)
            yield return note.ToDto();
    }
}
