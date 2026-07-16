using System.ComponentModel.DataAnnotations.Schema;
using MNoteProvider.Domain.Abstractions;
using MNoteProvider.Domain.Attributes;

namespace MNoteProvider.Domain;

/// <summary>
/// Persistence entity of a folder, mapped to the <c>folder</c> table. The folder
/// hierarchy is an adjacency list whose root folder is its own parent.
/// </summary>
[Table("folder")]
public class Folder : BaseDomainObject, IFolder
{
    /// <inheritdoc/>
    [Column("name")] public string Name { get; set; } = string.Empty;
    /// <inheritdoc/>
    [Column("parentid")] public Guid ParentId { get; set; }

    /// <inheritdoc/>
    [Column("creationdate")]
    [ImmutableColumnAttribute]
    public DateTime CreationDate { get; set; }
}
