namespace MNoteProvider.Common.Abstractions.DTOs;

/// <summary>
/// The data transfer contract of a tag.
/// </summary>
public interface ITagDto
{
    /// <summary>The unique identifier of the tag.</summary>
    Guid Id { get; init; }
    /// <summary>The display name of the tag. Names are globally unique.</summary>
    string Name { get; set; }
    /// <summary>The date the tag was created.</summary>
    DateTime CreationDate { get; init; }
}