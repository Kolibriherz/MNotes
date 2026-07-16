using Microsoft.Extensions.Logging.Abstractions;
using MNoteProvider.BusinessCore.Provider;
using MNoteProvider.Common.Abstractions.Enums;
using MNoteProvider.Common.Abstractions.Resources;
using MNoteProvider.Common.DTOs;
using MNoteProvider.DataAccess.Repositories;
using MNoteProvider.Domain;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.BusinessCore.Tests.Provider;

[TestFixture]
public class FolderProviderTests
{
    [Test]
    public async Task UpdateFolder_WhenMovedBelowItself_ReturnsConflictAndDoesNotUpdate()
    {
        // Arrange
        var folderId = Guid.NewGuid();

        var repository = new FakeFolderRepository(CreateFolder(folderId, Guid.Empty, "Folder"));

        var provider = new FolderProvider(repository, NullLogger<FolderProvider>.Instance);

        var dto = new FolderDto
        {
            Id = folderId,
            Name = "Folder",
            ParentId = folderId,
            CreationDate = DateTime.UnixEpoch,
            ChangeDate = DateTime.UnixEpoch
        };

        // Act
        var result = await provider.UpdateFolder(dto);

        // Assert
        Assert.That(result.IsT1, Is.True);
        Assert.That(result.AsT1.FailType, Is.EqualTo(MNotesFailType.CONFLICT));
        Assert.That(result.AsT1.Message, Is.EqualTo(ErrorMessages.FolderMoveWouldCreateCycle(folderId, folderId)));

        Assert.That(repository.UpdateCallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task UpdateFolder_WhenMovedBelowDescendant_ReturnsConflict()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var grandchildId = Guid.NewGuid();

        var repository = new FakeFolderRepository(
            CreateFolder(parentId, Guid.Empty, "Parent"),
            CreateFolder(childId, parentId, "Child"),
            CreateFolder(grandchildId, childId, "Grandchild"));

        var provider = new FolderProvider(repository, NullLogger<FolderProvider>.Instance);

        var dto = new FolderDto
        {
            Id = parentId,
            Name = "Parent",
            ParentId = grandchildId,
            CreationDate = DateTime.UnixEpoch,
            ChangeDate = DateTime.UnixEpoch
        };

        // Act
        var result = await provider.UpdateFolder(dto);

        // Assert
        Assert.That(result.IsT1, Is.True);
        Assert.That(result.AsT1.FailType, Is.EqualTo(MNotesFailType.CONFLICT));
        Assert.That(result.AsT1.Message, Is.EqualTo(ErrorMessages.FolderMoveWouldCreateCycle(parentId, grandchildId)));

        Assert.That(repository.UpdateCallCount, Is.EqualTo(0));
    }

    private static Folder CreateFolder(Guid id, Guid parentId,string name) =>new()
        {
            Id = id,
            ParentId = parentId,
            Name = name,
            CreationDate = DateTime.UnixEpoch,
            Doeom = DateTime.UnixEpoch
        };

    private sealed class FakeFolderRepository : IFolderRepository
    {
        private readonly Dictionary<Guid, IFolder> _folders;

        public FakeFolderRepository(params IFolder[] folders)
        {
            _folders = folders.ToDictionary(folder => folder.Id);
        }

        public int UpdateCallCount { get; private set; }

        public Task<IFolder?> GetByIdAsync( Guid id,CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<IFolder?>(
                _folders.GetValueOrDefault(id));
        }

        public Task<IEnumerable<IFolder>> GetAllAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<IEnumerable<IFolder>>( _folders.Values);
        }

        public Task<bool> CreateAsync( IFolder entity,CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(_folders.TryAdd(entity.Id, entity));
        }

        public Task<bool> UpdateAsync(IFolder entity,CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            UpdateCallCount++;

            if (!_folders.ContainsKey(entity.Id))
                return Task.FromResult(false);

            _folders[entity.Id] = entity;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(Guid id,CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(_folders.Remove(id));
        }
    }
}
