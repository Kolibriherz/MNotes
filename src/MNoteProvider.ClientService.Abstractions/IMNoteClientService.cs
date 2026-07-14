using MNoteProvider.Common;
using MNoteProvider.Common.Abstractions.DTOs;
using MNoteProvider.Common.Abstractions.Events;
using MNoteProvider.Common.DTOs;
using OneOf;

namespace MNoteProvider.ClientService.Abstractions;
/// <summary>
/// Provides HTTP-based access to the MNote API.
/// </summary>
public interface IMNoteClientService
{
    /// <summary>
    /// Checks whether the MNote API is available.
    /// </summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> IsAvailable(CancellationToken ct = default);
    /// <summary>
    /// Gets all notes from the MNote API.
    /// </summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The available notes or a process failure.</returns>
    Task<OneOf<INoteDto[], MNoteProcessFail>> GetAllNotes(CancellationToken ct = default);
    /// <summary>
    /// Creates a new note and returns its identifier.
    /// </summary>
    /// <param name="createNoteDto">The note data used to create the note.</param>
    /// <param name="ct">A token used to cancel the operation.</param>
    /// <returns>The note identifier or a process failure.</returns>
    Task<OneOf<Guid,MNoteProcessFail>> CreateNote(CreateNoteDto createNoteDto, CancellationToken ct = default);
    /// <summary>
    /// Updates an existing note.
    /// </summary>
    /// <param name="noteDto">The note data used to update the note.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> UpdateNote(NoteDto noteDto, CancellationToken ct = default);
    /// <summary>
    /// Deletes the note with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the note to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> DeleteNote(Guid id, CancellationToken ct = default);
    /// <summary>
    /// Loads the previous version of the specified note.
    /// </summary>
    /// <param name="noteId">The identifier of the note whose previous version is loaded.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The previous note version or a process failure.</returns>
    Task<OneOf<INoteDto,MNoteProcessFail>> LoadPreviousVersion(Guid noteId, CancellationToken ct = default);
    /// <summary>
    /// Gets the update history of the specified note.
    /// </summary>
    /// <param name="noteId">The identifier of the note whose history is loaded.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The note update history or a process failure.</returns>
    Task<OneOf<IUpdateEvent<NoteDto>[],MNoteProcessFail>> GetHistory(Guid noteId, CancellationToken ct = default);
   
    /// <summary>
    /// Gets all folders from the MNote API.
    /// </summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The available folders or a process failure.</returns>
    Task<OneOf<IFolderDto[],MNoteProcessFail>> GetAllFolders(CancellationToken ct = default);
    /// <summary>
    /// Creates a new folder and returns its identifier.
    /// </summary>
    /// <param name="createFolderDto">The folder data used to create the folder.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The created folder identifier or a process failure.</returns>
    Task<OneOf<Guid,MNoteProcessFail>> CreateFolder(CreateFolderDto createFolderDto, CancellationToken ct = default);
    /// <summary>
    /// Updates an existing folder.
    /// </summary>
    /// <param name="folderDto">The folder data used to update the folder.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> UpdateFolder(FolderDto folderDto, CancellationToken ct = default);
    /// <summary>
    /// Deletes the folder with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the folder to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> DeleteFolder(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all comments assigned to the specified note.
    /// </summary>
    /// <param name="id">The identifier of the note whose comments are loaded.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The comments assigned to the note or a process failure.</returns>
    Task<OneOf<ICommentDto[],MNoteProcessFail>> GetAllCommentsByNote(Guid id,CancellationToken ct = default);
    /// <summary>
    /// Creates a new comment and returns its identifier.
    /// </summary>
    /// <param name="createCommentDto">The comment data used to create the comment.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The created comment identifier or a process failure.</returns>
    Task<OneOf<Guid,MNoteProcessFail>> CreateComment(CreateCommentDto createCommentDto, CancellationToken ct = default);
    /// <summary>
    /// Updates an existing comment.
    /// </summary>
    /// <param name="commentDto">The comment data used to update the comment.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> UpdateComment(CommentDto commentDto, CancellationToken ct = default);
    /// <summary>
    /// Deletes the comment with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the comment to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> DeleteComment(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all tags from the MNote API.
    /// </summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The available tags or a process failure.</returns>
    Task<OneOf<ITagDto[],MNoteProcessFail>> GetAllTags(CancellationToken ct = default);
    /// <summary>
    /// Creates a new tag and returns its identifier.
    /// </summary>
    /// <param name="name">The name of the tag to create.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The created tag identifier or a process failure.</returns>
    Task<OneOf<Guid,MNoteProcessFail>> CreateTag(string name, CancellationToken ct = default);
    /// <summary>
    /// Deletes the tag with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the tag to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> DeleteTag(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all note-tag assignments from the MNote API.
    /// </summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The available note-tag assignments or a process failure.</returns>
    Task<OneOf<INoteTagAssignmentDto[],MNoteProcessFail>> GetAllNoteTagAssignments(CancellationToken ct = default);
    /// <summary>
    /// Assigns a tag to a note and returns the assignment identifier.
    /// </summary>
    /// <param name="assignmentDto">The assignment data containing the note and tag identifiers.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The created assignment identifier or a process failure.</returns>
    Task<OneOf<Guid,MNoteProcessFail>> AssignTag(AssignmentDto assignmentDto, CancellationToken ct = default);
    /// <summary>
    /// Removes a tag assignment from a note.
    /// </summary>
    /// <param name="assignmentDto">The assignment data containing the note and tag identifiers.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>A success flag or a process failure.</returns>
    Task<OneOf<bool,MNoteProcessFail>> UnassignTag(AssignmentDto assignmentDto, CancellationToken ct = default);

}
