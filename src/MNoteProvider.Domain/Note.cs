using System.ComponentModel.DataAnnotations.Schema;
using MNoteProvider.Domain.Abstractions;
using MNoteProvider.Domain.Attributes;

namespace MNoteProvider.Domain;


/// <summary>
/// Persistence entity of a note, mapped to the <c>note</c> table.
/// </summary>
[Table("note")]
public class Note : BaseDomainObject, INote
{
    /// <inheritdoc/>
    [Column("name")] public string Name { get; set; } = string.Empty;
    /// <inheritdoc/>
    [Column("content")] public string Content { get; set; } = string.Empty;
    /// <inheritdoc/>
    [Column("description")] public string Description { get; set; } = string.Empty;
    /// <inheritdoc/>
    [Column("folderid")] public Guid FolderId { get; set; }

    /// <inheritdoc/>
    [Column("creationdate")]
    [ImmutableColumnAttribute]
    public DateTime CreationDate { get; init; }
}
