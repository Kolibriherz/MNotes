namespace MNoteProvider.Domain.Abstractions;

/// <summary>
/// Base contract shared by all domain objects. Consumers depend on the domain
/// abstractions, never on the concrete persistence entities.
/// </summary>
public interface IBaseDomainObject
{
    /// <summary>The primary key of the domain object.</summary>
    Guid Id { get; init; }
    /// <summary>Date of entry or modification. Maintained by the application layer, not by the database.</summary>
    DateTime Doeom { get; set; }
}
