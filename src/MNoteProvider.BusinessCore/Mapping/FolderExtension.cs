using MNoteProvider.Common.DTOs;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.BusinessCore.Mapping;

/// <summary>
/// Provides extension methods for mapping folder domain entities to data transfer objects.
/// </summary>
/// <remarks>
/// The mapping creates DTOs for transferring folder data across application boundaries without
/// exposing the domain entities themselves.
/// </remarks>
public static class FolderExtension
{
    /// <summary>
    /// Maps a folder domain entity to a data transfer object.
    /// </summary>
    /// <param name="folder">The folder to map.</param>
    /// <returns>A data transfer object containing the folder data.</returns>
    public static FolderDto ToDto(this IFolder folder) => new FolderDto
    {
        Id = folder.Id,
        Name = folder.Name,
        ParentId = folder.ParentId,
        CreationDate = folder.CreationDate,
        ChangeDate = folder.Doeom
    };
    /// <summary>
    /// Lazily maps a sequence of folders domain entities to data transfer objects.
    /// </summary>
    /// <param name="folders">The folders to map.</param>
    /// <returns>A lazily evaluated sequence of folders data transfer objects.</returns>
    public static IEnumerable<FolderDto> ToDtos(this IEnumerable<IFolder> folders) => MapToDtos(folders);
    private static IEnumerable<FolderDto> MapToDtos(IEnumerable<IFolder> folders)
    {
        foreach (var folder in folders)
            yield return folder.ToDto();
    }
}
