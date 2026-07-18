using Microsoft.Extensions.Logging;
using MNoteProvider.BusinessCore.Mapping;
using MNoteProvider.Common.Abstractions;
using MNoteProvider.Common.Abstractions.Enums;
using MNoteProvider.Common.Abstractions.Resources;
using MNoteProvider.Common.DTOs;
using MNoteProvider.DataAccess.Repositories;
using MNoteProvider.Domain;
using Npgsql;
using OneOf;

namespace MNoteProvider.BusinessCore.Provider;

/// <summary>
/// Coordinates tag operations by mediating between the HTTP request handlers
/// and the tag repository. Handles creation, deletion and retrieval of tags
/// that can be assigned to notes.
/// </summary>
public interface ITagProvider
{
    /// <summary>Gets all tags.</summary>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>All tags as <see cref="TagDto"/> values, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<TagDto[], MNoteProcessFail>> GetAllTags(CancellationToken ct = default);

    /// <summary>Creates a new tag with the specified name.</summary>
    /// <param name="name">The name of the tag to create.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns>The identifier of the created tag, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<Guid, MNoteProcessFail>> CreateTag(string name, CancellationToken ct = default);

    /// <summary>Deletes the tag with the specified identifier.</summary>
    /// <param name="id">The identifier of the tag to delete.</param>
    /// <param name="ct">An optional token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the tag was deleted, or an <see cref="MNoteProcessFail"/> on failure.</returns>
    Task<OneOf<bool, MNoteProcessFail>> DeleteTag(Guid id, CancellationToken ct = default);
}

///<inheritdoc cref = "ITagProvider" />
public class TagProvider : ITagProvider
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<TagProvider> _logger;
    private const int MaxNameLength = 255;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagProvider"/> class.
    /// </summary>
    /// <param name="tagRepository">The repository used to persist and read tags.</param>
    /// <param name="logger">The logger used to record database failures.</param>
    public TagProvider(ITagRepository tagRepository, ILogger<TagProvider> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<OneOf<TagDto[], MNoteProcessFail>> GetAllTags(CancellationToken ct = default)
    {
        try
        {
            var tags = await _tagRepository.GetAllAsync(ct).ConfigureAwait(false);
            return tags.ToDtos().ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while trying to get tags");
            return new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("get", "tags"));
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<Guid, MNoteProcessFail>> CreateTag(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameRequired("tag"));

        if (name.Length > MaxNameLength)
            return new MNoteProcessFail(MNotesFailType.BADREQUEST, ErrorMessages.NameTooLong("tag", MaxNameLength));

        var newTag = new Tag
        {
            Id = Guid.NewGuid(),
            Doeom = DateTime.UtcNow,
            Name = name
        };

        try
        {
            var saved = await _tagRepository.CreateAsync(newTag, ct).ConfigureAwait(false);
            return saved
                ? newTag.Id
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("save", "tag"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while creating tag {TagId}", newTag.Id);
            return DatabaseFailureMapper.Map(e, "create", "tag");
        }
    }

    /// <inheritdoc/>
    public async Task<OneOf<bool, MNoteProcessFail>> DeleteTag(Guid id, CancellationToken ct = default)
    {
        try
        {
            if (await _tagRepository.GetByIdAsync(id, ct).ConfigureAwait(false) is null)
                return new MNoteProcessFail(MNotesFailType.NOTFOUND, ErrorMessages.EntryDoesNotExist(id));

            var deleted = await _tagRepository.DeleteAsync(id, ct).ConfigureAwait(false);
            return deleted
                ? true
                : new MNoteProcessFail(MNotesFailType.PROBLEM, ErrorMessages.DatabaseFail("delete", "tag"));
        }
        catch (OperationCanceledException) { throw; }
        catch (PostgresException e)
        {
            _logger.LogError(e, "Database error while deleting tag {TagId}", id);
            return DatabaseFailureMapper.Map(e, "delete", "tag");
        }
    }
}
