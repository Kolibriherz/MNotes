using MNoteProvider.Domain;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.DataAccess.Repositories;

/// <summary>
/// Provides persistence access for <see cref="ITag"/> entities.
/// </summary>
/// <remarks>
/// This interface currently adds no members beyond <see cref="IBaseRepository{TInterface}"/>.
/// It exists as a named seam: consumers depend on <c>ITagRepository</c> rather than on the
/// generic base, so tag-specific queries can be introduced without touching a single
/// call site.
/// </remarks>
public interface ITagRepository : IBaseRepository<ITag>;
/// <summary>
/// Dapper-backed repository for <see cref="Tag"/> entities, mapped to the table declared
/// by the <see cref="System.ComponentModel.DataAnnotations.Schema.TableAttribute"/> on <see cref="Tag"/>.
/// </summary>
/// <remarks>
/// All behaviour is inherited from <see cref="BaseRepository{TEntity, TInterface}"/>.
/// Tag-specific queries that fall outside the standard CRUD surface belong here.
/// </remarks>
public sealed class TagRepository : BaseRepository<Tag, ITag>, ITagRepository
{
    /// <summary>
    /// Initialises a new repository bound to the given database.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public TagRepository(string connectionString) : base(connectionString)
    {
    }
}
