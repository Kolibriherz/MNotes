using System.ComponentModel.DataAnnotations.Schema;
using MNoteProvider.Domain.Abstractions;
using MNoteProvider.Domain.Attributes;
namespace MNoteProvider.Domain;

/// <summary>
/// Persistence entity of a comment, mapped to the <c>comment</c> table.
/// Deleting the owning note deletes its comments.
/// </summary>
[TableAttribute( "comment")]
public class Comment :BaseDomainObject, IComment
{
    /// <inheritdoc/>
    [Column("content")] public string Content { get; set; } = string.Empty;
    /// <inheritdoc/>
    [Column("noteid")] public Guid NoteId { get; set; }

    /// <inheritdoc/>
    [Column("creationdate")]
    [ImmutableColumnAttribute]
    public DateTime CreationDate { get; set; }

}
