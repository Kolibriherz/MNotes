using MNoteProvider.Common.DTOs;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.BusinessCore.Mapping;

/// <summary>
/// Provides extension methods for mapping tag domain entities to data transfer objects.
/// </summary>
/// <remarks>
/// The mapping creates DTOs for transferring tag data across application boundaries without
/// exposing the domain entities themselves.
/// </remarks>
public static class TagExtension
{
    /// <summary>
    /// Maps a tag domain entity to a data transfer object.
    /// </summary>
    /// <param name="tag">The tag to map.</param>
    /// <returns>A data transfer object containing the tag data.</returns>
    public static TagDto ToDto(this ITag tag) => new TagDto
    {
        Id = tag.Id,
        Name = tag.Name,
        CreationDate = tag.Doeom
    };

    /// <summary>
    /// Lazily maps a sequence of tag domain entities to data transfer objects.
    /// </summary>
    /// <param name="tags">The tags to map.</param>
    /// <returns>A lazily evaluated sequence of tags data transfer objects.</returns>
    public static IEnumerable<TagDto> ToDtos(this IEnumerable<ITag> tags) => MapToDtos(tags);
    private static IEnumerable<TagDto> MapToDtos(IEnumerable<ITag> tags)
    {
        foreach (var tag in tags)
            yield return tag.ToDto();
    }
}
