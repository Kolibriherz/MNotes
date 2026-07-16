using MNoteProvider.Common.Abstractions.DTOs;

namespace MNoteProvider.Common.DTOs;

/// <summary>
/// Data transfer record of a tag.
/// </summary>
public record TagDto : ITagDto
{
    /// <inheritdoc/>
    public Guid Id { get; init; }
    /// <inheritdoc/>
    public required string Name { get; set; }
    /// <inheritdoc/>
    public DateTime CreationDate { get; init; }
}
