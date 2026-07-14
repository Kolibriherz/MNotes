
namespace MNoteProvider.Domain.Attributes;

/// <summary>
/// Marks a property as write-once: it is included in INSERT statements but excluded
/// from the SET clause of UPDATE statements.
/// </summary>
/// <remarks>
/// Use this for values that are assigned when the row is created and must never change
/// afterwards, such as a creation timestamp. Without this marker, a caller supplying a
/// partially populated entity would silently overwrite the stored value with a default.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ImmutableColumnAttribute : Attribute;
