using System.ComponentModel.DataAnnotations.Schema;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.Domain;

/// <summary>
/// Persistence entity of a tag, mapped to the <c>tag</c> table. Tag names are globally unique.
/// </summary>
[Table("tag")]
public class Tag : BaseDomainObject, ITag
{
    /// <inheritdoc/>
    [Column("name")] public string Name { get; set; } = string.Empty;

}



