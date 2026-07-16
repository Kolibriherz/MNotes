using MNoteProvider.BusinessCore.Provider;
using MNoteProvider.Common.DTOs;

namespace MNoteProvider.RequestHandler;
/// <summary>
/// Handles incoming HTTP requests for folder resources by delegating to the business layer
/// and translating its result types into HTTP responses.
/// </summary>
/// <remarks>
/// This is the boundary between transport and domain: it is the only place where a
/// business result is turned into an <see cref="IResult"/>. Everything below this layer
/// is unaware of HTTP.
/// </remarks>
public interface IFolderRequestHandler
{
    /// <summary>
    /// Retrieves all folders.
    /// </summary>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the flat list of folders, or an error response derived from the failure
    /// returned by the business layer.
    /// </returns>
    Task<IResult> GetAllFolders(CancellationToken ct = default);

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    /// <param name="createFolderDto">Name of the new folder and the id of its parent folder.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>201 Created</c> with the identifier of the created folder, or an error
    /// response derived from the failure returned by the business layer.
    /// </returns>
    Task<IResult> CreateFolder(CreateFolderDto createFolderDto, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing folder.
    /// </summary>
    /// <param name="folderDto">Folder to update, identified by its id.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the updated folder, or an error response derived from the failure
    /// returned by the business layer.
    /// </returns>
    Task<IResult> UpdateFolder(FolderDto folderDto, CancellationToken ct = default);

    /// <summary>
    /// Deletes the folder with the given id.
    /// </summary>
    /// <param name="id">Id of the folder to delete.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> on success, or an error response derived from the failure returned by the
    /// business layer. Deleting a folder deletes the notes and folders it contains.
    /// </returns>
    Task<IResult> DeleteFolder(Guid id, CancellationToken ct = default);
}
///<inheritdoc cref = "IFolderRequestHandler" />
public class FolderRequestHandler : IFolderRequestHandler
{
    private readonly IFolderProvider _folderProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderRequestHandler"/> class.
    /// </summary>
    /// <param name="folderProvider">Business layer component this handler delegates all folder operations to.</param>
    public FolderRequestHandler(IFolderProvider folderProvider)
    {
        _folderProvider = folderProvider;
    }
    /// <inheritdoc/>
    public async Task<IResult> GetAllFolders(CancellationToken ct = default)
    {
        var mFoldersResult = await _folderProvider.GetAllFolders(ct).ConfigureAwait(false);
        return mFoldersResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
    /// <inheritdoc/>
    public async Task<IResult> CreateFolder(CreateFolderDto createFolderDto, CancellationToken ct = default)
    {
        var mFoldersResult = await _folderProvider.CreateFolder(createFolderDto, ct).ConfigureAwait(false);
        return mFoldersResult.Match(id => Results.Created((string?)null, id), fail => fail.ToIResult());
    }
    /// <inheritdoc/>
    public async Task<IResult> UpdateFolder(FolderDto folderDto, CancellationToken ct = default)
    {
        var mFoldersResult = await _folderProvider.UpdateFolder(folderDto, ct).ConfigureAwait(false);
        return mFoldersResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
    /// <inheritdoc/>
    public async Task<IResult> DeleteFolder(Guid id, CancellationToken ct = default)
    {
        var mFoldersResult = await _folderProvider.DeleteFolder(id, ct).ConfigureAwait(false);
        return mFoldersResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }
}
