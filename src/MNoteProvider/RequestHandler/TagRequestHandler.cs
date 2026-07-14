using MNoteProvider.BusinessCore.Provider;

namespace MNoteProvider.RequestHandler;

/// <summary>
/// Handles incoming HTTP requests for tag resources by delegating to the business layer
/// and translating its result types into HTTP responses.
/// </summary>
/// <remarks>
/// This is the boundary between transport and domain: it is the only place where a
/// business result is turned into an <see cref="IResult"/>. Everything below this layer
/// is unaware of HTTP.
/// </remarks>
public interface ITagRequestHandler
{
    /// <summary>
    /// Retrieves all tags.
    /// </summary>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the list of tags, or an error response derived from the failure
    /// returned by the business layer.
    /// </returns>
    Task<IResult> GetAllTags(CancellationToken ct = default);

    /// <summary>
    /// Creates a new tag with the given name.
    /// </summary>
    /// <param name="name">Name of the tag to create. Tag names are globally unique.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>201 Created</c> with the identifier of the created tag, or an error
    /// response derived from the failure returned by the business layer.
    /// </returns>
    Task<IResult> CreateTag(string name,CancellationToken ct = default);

    /// <summary>
    /// Deletes the tag with the given id.
    /// </summary>
    /// <param name="id">Id of the tag to delete.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> on success, or an error response derived from the failure returned by
    /// the business layer. Deleting a tag also removes its assignments, but never the notes
    /// it was assigned to.
    /// </returns>
    Task<IResult> DeleteTag(Guid id, CancellationToken ct = default);
}
///<inheritdoc cref = "ITagRequestHandler" />
public class TagRequestHandler : ITagRequestHandler
{
    private readonly ITagProvider _tagProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagRequestHandler"/> class.
    /// </summary>
    /// <param name="tagProvider">Business layer component this handler delegates all tag operations to.</param>
    public TagRequestHandler(ITagProvider tagProvider)
    {
        _tagProvider = tagProvider;
    }
    /// <inheritdoc/>
    public async Task<IResult> GetAllTags(CancellationToken ct = default)
    {
        var mTagsResult = await _tagProvider.GetAllTags(ct).ConfigureAwait(false);
        return   mTagsResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
    }

    /// <inheritdoc/>
    public async Task<IResult> CreateTag(string name, CancellationToken ct = default)
    {
        var mTagsResult = await _tagProvider.CreateTag(name,ct).ConfigureAwait(false);
        return mTagsResult.Match(id => Results.Created((string?)null, id), fail => fail.ToIResult());
    }

    /// <inheritdoc/>
    public async Task<IResult> DeleteTag(Guid id, CancellationToken ct = default)
    {
        var mTagsResult = await _tagProvider.DeleteTag(id, ct).ConfigureAwait(false);
        return   mTagsResult.Match(l => Results.Ok(l), fail => fail.ToIResult());
       
    }
}
