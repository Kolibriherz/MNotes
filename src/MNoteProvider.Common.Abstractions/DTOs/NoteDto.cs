namespace MNoteProvider.Common.Abstractions.DTOs;

/// <summary>
/// The data transfer contract of a note.
/// </summary>
public interface INoteDto
{
    /// <summary>The unique identifier of the note.</summary>
    Guid Id { get; init; }
    /// <summary>The display name of the note.</summary>
    string Name { get; set; }
    /// <summary>The main body text of the note.</summary>
    string Content { get; set; }
    /// <summary>A short description of the note.</summary>
    string Description { get; set; }
    /// <summary>The folder that owns this note.</summary>
    Guid FolderId { get; set; }
    /// <summary>The date the note was created.</summary>
    DateTime CreationDate { get; init; }
    /// <summary>The date of the last change.</summary>
    DateTime ChangeDate { get; set; }
}


/// <summary>
/// The data required to create a new note.
/// </summary>
public interface ICreateNoteDto
{
    /// <summary>The display name of the new note.</summary>
    string Name { get; set; }
    /// <summary>The folder the new note is created in.</summary>
    Guid FolderId { get; set; }
}

