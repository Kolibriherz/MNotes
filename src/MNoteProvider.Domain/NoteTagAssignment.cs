using System.ComponentModel.DataAnnotations.Schema;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.Domain;

/// <summary>
/// Persistence entity of a note-tag assignment, mapped to the <c>notetagassignment</c> table.
/// </summary>
[Table("notetagassignment")]
public class NoteTagAssignment : BaseDomainObject, INoteTagAssignment
{
    /// <inheritdoc/>
    [Column("tagid")] public Guid TagId { get; set; }
    /// <inheritdoc/>
    [Column("noteid")] public Guid NoteId { get; set; }
}
