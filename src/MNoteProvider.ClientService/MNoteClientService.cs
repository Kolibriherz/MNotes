using System.Net.Http.Json;
using MNoteProvider.ClientService.Abstractions;
using MNoteProvider.Common;
using MNoteProvider.Common.Abstractions.DTOs;
using MNoteProvider.Common.Abstractions.Enums;
using MNoteProvider.Common.Abstractions.Events;
using MNoteProvider.Common.DTOs;
using MNoteProvider.Common.Events;
using OneOf;

namespace MNoteProvider.ClientService;

/// <inheritdoc/>
public class MNoteClientService : IMNoteClientService
{
    private readonly HttpClient _httpClient;
    /// <summary>
    /// Initializes a new instance of the MNote client service.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests to the MNote API.</param>
    public MNoteClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    /// <inheritdoc/>
    public async Task<OneOf<bool,MNoteProcessFail>> IsAvailable(CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.IsAvailable;
        try
        {
            using var result = await _httpClient.GetAsync(endpoint, ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(IsAvailable), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(IsAvailable)}: {e.Message}");
        }
    }
    
#region notes
    /// <inheritdoc/>
    public async Task<OneOf<INoteDto[], MNoteProcessFail>> GetAllNotes(CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteEndpoints.GetAll;
        try
        {
            using var result = await _httpClient.GetAsync(endpoint, ct).ConfigureAwait(false);
            var response = await ReadResponseAsync<NoteDto[]>(result, endpoint, nameof(GetAllNotes), ct).ConfigureAwait(false);

            return response.Match<OneOf<INoteDto[], MNoteProcessFail>>(notes => notes, fail => fail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(GetAllNotes)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> CreateNote(CreateNoteDto createNoteDto, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteEndpoints.Create;
        try
        {
            using var result = await _httpClient.PostAsJsonAsync(endpoint,createNoteDto, ct).ConfigureAwait(false);
            return await ReadResponseAsync<Guid>(result, endpoint, nameof(CreateNote), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(CreateNote)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> UpdateNote(NoteDto noteDto, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteEndpoints.Update;
        try
        {
            using var result = await _httpClient.PutAsJsonAsync(endpoint,noteDto,ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(UpdateNote), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(UpdateNote)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> DeleteNote(Guid id, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteEndpoints.Delete + $"/{id}";
        try
        {
            using var result = await _httpClient.DeleteAsync(endpoint,ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(DeleteNote), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(DeleteNote)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<INoteDto, MNoteProcessFail>> LoadPreviousVersion(Guid noteId, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteEndpoints.LoadPreviousVersion + $"/{noteId}";
        try
        {
            using var result = await _httpClient.GetAsync(endpoint, ct).ConfigureAwait(false);
            var response = await ReadResponseAsync<NoteDto>(result, endpoint, nameof(LoadPreviousVersion), ct).ConfigureAwait(false);

            return response.Match<OneOf<INoteDto, MNoteProcessFail>>(note => note, fail => fail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(LoadPreviousVersion)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<IUpdateEvent<NoteDto>[], MNoteProcessFail>> GetHistory(Guid noteId, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteEndpoints.GetHistory + $"/{noteId}";
        try
        {
            using var result = await _httpClient.GetAsync(endpoint, ct).ConfigureAwait(false);
            var response = await ReadResponseAsync<UpdateEvent[]>(result, endpoint, nameof(GetHistory), ct).ConfigureAwait(false);

            return response.Match<OneOf<IUpdateEvent<NoteDto>[], MNoteProcessFail>>(updates => updates, fail => fail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(GetHistory)}: {e.Message}");
        }
    }

#endregion

#region folder
    /// <inheritdoc/>
   public async Task<OneOf<IFolderDto[], MNoteProcessFail>> GetAllFolders(CancellationToken ct = default)
   {
       string endpoint = MNotesRoutes.Endpoints.FolderEndpoints.GetAll;
        try
        {
            using var result = await _httpClient.GetAsync(endpoint, ct).ConfigureAwait(false);
            var response = await ReadResponseAsync<FolderDto[]>(result, endpoint, nameof(GetAllFolders), ct).ConfigureAwait(false);

            return response.Match<OneOf<IFolderDto[], MNoteProcessFail>>(folders => folders, fail => fail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(GetAllFolders)}: {e.Message}");
        }
        
    }
    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> CreateFolder(CreateFolderDto createFolderDto, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.FolderEndpoints.Create;
        try
        {
            using var result = await _httpClient.PostAsJsonAsync(endpoint,createFolderDto, ct).ConfigureAwait(false);
            return await ReadResponseAsync<Guid>(result, endpoint, nameof(CreateFolder), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(CreateFolder)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> UpdateFolder(FolderDto folderDto, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.FolderEndpoints.Update;
        try
        {
            using var result = await _httpClient.PutAsJsonAsync(endpoint,folderDto,ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(UpdateFolder), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(UpdateFolder)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> DeleteFolder(Guid id, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.FolderEndpoints.Delete + $"/{id}";
        try
        {
            using var result = await _httpClient.DeleteAsync(endpoint,ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(DeleteFolder), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(DeleteFolder)}: {e.Message}");
        }
    }

#endregion

#region comments
    /// <inheritdoc/>
  public async Task<OneOf<ICommentDto[], MNoteProcessFail>> GetAllCommentsByNote(Guid id, CancellationToken ct = default)
  {
      string endpoint = MNotesRoutes.Endpoints.CommentEndpoints.GetAllByNote + $"/{id}";
        try
        {
            using var result = await _httpClient.GetAsync(endpoint, ct).ConfigureAwait(false);
            var response = await ReadResponseAsync<CommentDto[]>(result, endpoint, nameof(GetAllCommentsByNote), ct).ConfigureAwait(false);

            return response.Match<OneOf<ICommentDto[], MNoteProcessFail>>(comments => comments, fail => fail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(GetAllCommentsByNote)}: {e.Message}");
        }
        
    }
    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> CreateComment(CreateCommentDto createCommentDto, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.CommentEndpoints.Create;
        try
        {
            using var result = await _httpClient.PostAsJsonAsync(endpoint,createCommentDto, ct).ConfigureAwait(false);
            return await ReadResponseAsync<Guid>(result, endpoint, nameof(CreateComment), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(CreateComment)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> UpdateComment(CommentDto commentDto, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.CommentEndpoints.Update;
        try
        {
            using var result = await _httpClient.PutAsJsonAsync(endpoint,commentDto,ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(UpdateComment), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(UpdateComment)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> DeleteComment(Guid id, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.CommentEndpoints.Delete + $"/{id}";
        try
        {
            using var result = await _httpClient.DeleteAsync(endpoint,ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(DeleteComment), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(DeleteComment)}: {e.Message}");
        }
    }

#endregion

#region tags
    /// <inheritdoc/>
  public async Task<OneOf<ITagDto[], MNoteProcessFail>> GetAllTags(CancellationToken ct = default)
  {
      string endpoint = MNotesRoutes.Endpoints.TagEndpoints.GetAll;
        try
        {
            using var result = await _httpClient.GetAsync(endpoint, ct).ConfigureAwait(false);
            var response = await ReadResponseAsync<TagDto[]>(result, endpoint, nameof(GetAllTags), ct).ConfigureAwait(false);

            return response.Match<OneOf<ITagDto[], MNoteProcessFail>>(tags => tags, fail => fail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(GetAllTags)}: {e.Message}");
        }
    }
    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> CreateTag(string name, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.TagEndpoints.Create;
        try
        {
            using var result = await _httpClient.PostAsJsonAsync(endpoint,name, ct).ConfigureAwait(false);
            return await ReadResponseAsync<Guid>(result, endpoint, nameof(CreateTag), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(CreateTag)}: {e.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> DeleteTag(Guid id, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.TagEndpoints.Delete + $"/{id}";
        try
        {
            using var result = await _httpClient.DeleteAsync(endpoint,ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(DeleteTag), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(DeleteTag)}: {e.Message}");
        }
    }
#endregion

#region notetagassignments
/// <inheritdoc/>
 public async Task<OneOf<INoteTagAssignmentDto[], MNoteProcessFail>> GetAllNoteTagAssignments(CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteTagAssignmentEndpoints.GetAll;
        try
        {
            using var result = await _httpClient.GetAsync(endpoint, ct).ConfigureAwait(false);
            var response = await ReadResponseAsync<NoteTagAssignmentDto[]>(result, endpoint, nameof(GetAllNoteTagAssignments), ct).ConfigureAwait(false);

            return response.Match<OneOf<INoteTagAssignmentDto[], MNoteProcessFail>>(assignments => assignments, fail => fail);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(GetAllNoteTagAssignments)}: {e.Message}");
        }
        
    }
    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> AssignTag(AssignmentDto assignmentDto, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteTagAssignmentEndpoints.Assign;
        try
        {
            using var result = await _httpClient.PostAsJsonAsync(endpoint,assignmentDto, ct).ConfigureAwait(false);
            return await ReadResponseAsync<Guid>(result, endpoint, nameof(AssignTag), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM, $"Error while calling {endpoint} in {nameof(AssignTag)}: {e.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> UnassignTag(AssignmentDto assignmentDto, CancellationToken ct = default)
    {
        string endpoint = MNotesRoutes.Endpoints.NoteTagAssignmentEndpoints.Unassign + $"/{assignmentDto.NoteId}/{assignmentDto.TagId}";
        try
        {
            using var result = await _httpClient.DeleteAsync(endpoint, ct).ConfigureAwait(false);
            return await ReadResponseAsync<bool>(result, endpoint, nameof(UnassignTag), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            return new MNoteProcessFail(MNotesFailType.PROBLEM,
                $"Error while calling {endpoint} in {nameof(UnassignTag)}: {e.Message}");
        }
    }

#endregion

    private async Task<OneOf<T, MNoteProcessFail>> ReadResponseAsync<T>( HttpResponseMessage response, string endpoint, string callerName, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<T>(ct).ConfigureAwait(false);

            return value is not null ? value
                : new MNoteProcessFail( MNotesFailType.PROBLEM, $"Server returned an empty response while calling {endpoint} in {callerName}");
        }

        var fail = await response.Content.ReadFromJsonAsync<MNoteProcessFail>(ct).ConfigureAwait(false);
        return fail ?? new MNoteProcessFail( MNotesFailType.PROBLEM, response.ReasonPhrase ?? $"Error while calling {endpoint} in {callerName}");
    }
}

