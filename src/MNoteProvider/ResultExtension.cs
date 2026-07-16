using MNoteProvider.Common;
using MNoteProvider.Common.Abstractions.Enums;

namespace MNoteProvider;

/// <summary>
/// Provides extension methods for translating process failures into HTTP responses.
/// </summary>
/// <remarks>
/// This class centralizes the mapping from <see cref="MNoteProcessFail"/> failure types to HTTP
/// status codes. Request handlers can therefore return domain-specific failure information without
/// depending on HTTP response construction.
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Converts the specified process failure into an HTTP result with the corresponding status code.
    /// </summary>
    /// <param name="fail">The process failure to translate into an HTTP response.</param>
    /// <returns>An HTTP result containing the failure details and the status code associated with its failure type.</returns>
    public static IResult ToIResult(this MNoteProcessFail fail)
        => fail.FailType switch
        {
            MNotesFailType.NOTFOUND => Results.Json(fail, statusCode: 404),
            MNotesFailType.BADREQUEST => Results.Json(fail, statusCode: 400),
            MNotesFailType.UNAUTHORIZED => Results.Json(fail, statusCode: 401),
            MNotesFailType.FORBID => Results.Json(fail, statusCode: 403),
            MNotesFailType.CONFLICT => Results.Json(fail, statusCode: 409),
            _ => Results.Json(fail, statusCode: 500)
        };
}
