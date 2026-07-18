using MNoteProvider.Common.Abstractions;
using MNoteProvider.Common.Abstractions.Enums;
using MNoteProvider.Common.Abstractions.Resources;
using Npgsql;

namespace MNoteProvider.BusinessCore;

/// <summary>
/// Translates PostgreSQL errors into application-level failures with appropriate
/// failure categories and user-facing error messages.
/// </summary>
/// <remarks>
/// Known constraint violations are mapped to conflict or bad-request failures.
/// All other PostgreSQL errors are mapped to a generic processing failure.
/// </remarks>
public static class DatabaseFailureMapper
{
    /// <summary>
    /// Maps a PostgreSQL exception to the corresponding application-level failure.
    /// </summary>
    /// <param name="exception">The PostgreSQL exception to translate.</param>
    /// <param name="action">The database operation that was attempted.</param>
    /// <param name="entity">The type of entity involved in the operation.</param>
    /// <returns>A failure describing the database error without exposing internal details.</returns>
    public static MNoteProcessFail Map(PostgresException exception, string action, string entity)
    {
        return exception.SqlState switch
        {

            PostgresErrorCodes.UniqueViolation =>
                new MNoteProcessFail(MNotesFailType.CONFLICT, ErrorMessages.EntryAlreadyExists(entity)),

            PostgresErrorCodes.ForeignKeyViolation =>
                new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.InvalidReference(entity)),

            PostgresErrorCodes.CheckViolation =>
                new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.InvalidData(entity)),

            PostgresErrorCodes.NotNullViolation or PostgresErrorCodes.StringDataRightTruncation =>
                new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.InvalidData(entity)),

            _ =>
                new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail(action, entity))
        };
    }
}
