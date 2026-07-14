using System.ComponentModel.DataAnnotations.Schema;

namespace MNoteProvider.Domain;

/// <summary>
/// Base type of all persistence entities. Carries the primary key and the
/// modification timestamp shared by every table.
/// </summary>
public abstract  class BaseDomainObject
{
    /// <summary>The primary key of the entity.</summary>
    [Column("id")]  public Guid Id { get; init; }
    /// <summary>Date of entry or modification. Maintained by the application layer, not by the database.</summary>
    [Column("doeom")] public DateTime Doeom { get; set; }
}
