using Microsoft.Extensions.Logging;
using MNoteProvider.BusinessCore.Mapping;
using MNoteProvider.Common;
using MNoteProvider.Common.Abstractions.Enums;
using MNoteProvider.Common.Abstractions.Resources;
using MNoteProvider.Common.DTOs;
using MNoteProvider.DataAccess.Repositories;
using MNoteProvider.Domain;
using Npgsql;
using OneOf;

namespace MNoteProvider.BusinessCore.Provider;

/// <summary>
/// Coordinates folder operations by mediating between the HTTP request handlers
/// and the folder repository. Handles creation, updating, deletion and retrieval
/// of folders used to organize notes.
/// </summary>
public interface IFolderProvider
{
    /// <summary>Gets all folders.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>All folders as <see cref="FolderDto"/> values, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<FolderDto[], MNoteProcessFail>> GetAllFolders(CancellationToken ct = default);

    /// <summary>Creates a new folder.</summary>
    /// <param name="createFolderDto">The data used to create the folder.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The identifier of the created folder, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<Guid, MNoteProcessFail>> CreateFolder(CreateFolderDto createFolderDto, CancellationToken ct = default);

    /// <summary>Updates the name and parent of an existing folder.</summary>
    /// <param name="folderDto">The data used to update the folder.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the folder was updated, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<bool, MNoteProcessFail>> UpdateFolder(FolderDto folderDto, CancellationToken ct = default);

    /// <summary>Deletes the folder with the specified identifier.</summary>
    /// <param name="id">The identifier of the folder to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the folder was deleted, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<bool, MNoteProcessFail>> DeleteFolder(Guid id, CancellationToken ct = default);
}

///<inheritdoc cref = "IFolderProvider" />
public class FolderProvider : IFolderProvider
{
    private readonly IFolderRepository _folderRepository;
    private readonly ILogger<FolderProvider> _logger;
    private static readonly Guid _rootFolderId = Guid.Empty;
    private const int MaxNameLength = 255;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderProvider"/> class.
    /// </summary>
    /// <param name="folderRepository">The repository used to persist and read folders.</param>
    /// <param name="logger">The logger used to record database failures.</param>
    public FolderProvider(IFolderRepository folderRepository, ILogger<FolderProvider> logger)
    {
        _folderRepository = folderRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<OneOf<FolderDto[], MNoteProcessFail>> GetAllFolders(CancellationToken ct = default)
    {
        try
        {
            var folders = await _folderRepository.GetAllAsync(ct).ConfigureAwait(false);
            return folders.ToDtos().ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while trying to get folders");
            return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("get", "folders"));
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> CreateFolder(CreateFolderDto createFolderDto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(createFolderDto.Name))
            return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameRequired("folder"));

        if (createFolderDto.Name.Length > MaxNameLength)
            return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameTooLong("folder", MaxNameLength));

        var newFolder = new Folder
        {
            Id = Guid.NewGuid(),
            Doeom = DateTime.UtcNow,
            Name = createFolderDto.Name,
            ParentId = createFolderDto.ParentId,
            CreationDate = DateTime.UtcNow
        };

        try
        {
            if (await _folderRepository.GetByIdAsync(createFolderDto.ParentId, ct).ConfigureAwait(false) is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(createFolderDto.ParentId));

            var saved = await _folderRepository.CreateAsync(newFolder, ct).ConfigureAwait(false);
            return saved
                ? newFolder.Id
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("save", "folder"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while creating folder {FolderId}", newFolder.Id);
            return DatabaseFailureMapper.Map(e, "create", "folder");

        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> UpdateFolder(FolderDto folderDto, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(folderDto.Name))
                return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameRequired("folder"));

            if (folderDto.Name.Length > MaxNameLength)
                return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameTooLong("folder", MaxNameLength));

            var updateFolder = await _folderRepository.GetByIdAsync(folderDto.Id, ct).ConfigureAwait(false);

            if (updateFolder is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(folderDto.Id));

            if (folderDto.Id == _rootFolderId && folderDto.ParentId != _rootFolderId)
                return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.RootFolderNotMovable);


            if (await _folderRepository.GetByIdAsync(folderDto.ParentId, ct).ConfigureAwait(false) is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(folderDto.ParentId));


            if (await WouldCreateCycleAsync(folderDto.Id, folderDto.ParentId, ct).ConfigureAwait(false))
                return new MNoteProcessFail(MNotesFailType.CONFLICT, ErrorMessages.FolderMoveWouldCreateCycle(folderDto.Id, folderDto.ParentId));

            updateFolder.Name = folderDto.Name;
            updateFolder.ParentId = folderDto.ParentId;
            updateFolder.Doeom = DateTime.UtcNow;

            var updated = await _folderRepository.UpdateAsync(updateFolder, ct).ConfigureAwait(false);
            return updated
                ? true
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("update", "folder"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while updating folder {FolderId}", folderDto.Id);
            return DatabaseFailureMapper.Map(e, "update", "folder");
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> DeleteFolder(Guid id, CancellationToken ct = default)
    {
        try
        {
            if (id == _rootFolderId)
                return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.RootFolderNotDeletable);

            if (await _folderRepository.GetByIdAsync(id, ct).ConfigureAwait(false) is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(id));

            var deleted = await _folderRepository.DeleteAsync(id, ct).ConfigureAwait(false);
            return deleted
                ? true
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("delete", "folder"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while deleting folder {FolderId}", id);
            return DatabaseFailureMapper.Map(e, "delete", "folder");
        }
    }

    private async Task<bool> WouldCreateCycleAsync(Guid folderId, Guid newParentId, CancellationToken ct)
    {
        var currentId = newParentId;
        var visitedFolderIds = new HashSet<Guid>();

        while (currentId != _rootFolderId)
        {
            if (currentId == folderId)
                return true;

            if (!visitedFolderIds.Add(currentId))
                return true;

            var currentFolder = await _folderRepository.GetByIdAsync(currentId, ct).ConfigureAwait(false);

            if (currentFolder is null)
                return false;

            currentId = currentFolder.ParentId;
        }

        return false;
    }
}
