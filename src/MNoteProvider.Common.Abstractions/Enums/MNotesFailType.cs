namespace MNoteProvider.Common.Abstractions.Enums;

/// <summary>
/// Defines the categories of failures that can occur while processing an MNotes operation.
/// </summary>
/// <remarks>
/// Each value represents a distinct failure condition so that callers can handle failures
/// consistently at the application boundary.
/// </remarks>
public enum MNotesFailType
{
    /// <summary>Default value; no failure type has been assigned.</summary>
    UNSET,
    /// <summary>The request was malformed or contained invalid data. Maps to HTTP 400.</summary>
    BADREQUEST,
    /// <summary>An unexpected processing or infrastructure error, e.g. a database failure. Maps to HTTP 500.</summary>
    PROBLEM,
    /// <summary>The caller is not authenticated. Maps to HTTP 401.</summary>
    UNAUTHORIZED,
    /// <summary>The content cannot be served for legal reasons.</summary>
    UNAVAILABLE_FOR_LEGAL_REASONS,
    /// <summary>The requested entry does not exist. Maps to HTTP 404.</summary>
    NOTFOUND,
    /// <summary>The caller is authenticated but not allowed to perform the operation. Maps to HTTP 403.</summary>
    FORBID,
    /// <summary>The operation yielded no result to report.</summary>
    NOTHING,
    /// <summary>The operation conflicts with existing data, e.g. a duplicate unique value. Maps to HTTP 409.</summary>
    CONFLICT
}
