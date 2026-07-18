using MNoteProvider.Common.Abstractions.Enums;

namespace MNoteProvider.Common.Abstractions;
/// <summary>
/// Represents a failure that occurred while processing an MNotes operation.
/// </summary>
/// <param name="FailType">The category that describes the failure.</param>
/// <param name="Message">A human-readable description of the failure.</param>
public record MNoteProcessFail(MNotesFailType FailType, string Message);
