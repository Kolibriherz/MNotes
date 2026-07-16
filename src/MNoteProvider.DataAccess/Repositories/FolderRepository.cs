using MNoteProvider.Domain;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.DataAccess.Repositories;

/// <summary>
/// Provides persistence access for <see cref="IFolder"/> entities.
/// </summary>
/// <remarks>
/// This interface currently adds no members beyond <see cref="IBaseRepository{TInterface}"/>.
/// It exists as a named seam: consumers depend on <c>IFolderRepository</c> rather than on the
/// generic base, so folder-specific queries can be introduced later without touching a single
/// call site.
/// </remarks>
public interface IFolderRepository : IBaseRepository<IFolder>;

/// <summary>
/// Dapper-backed repository for <see cref="Folder"/> entities, mapped to the table declared
/// by the <see cref="System.ComponentModel.DataAnnotations.Schema.TableAttribute"/> on <see cref="Folder"/>.
/// </summary>
/// <remarks>
/// All behaviour is inherited from <see cref="BaseRepository{TEntity, TInterface}"/>.
/// Folder-specific queries that fall outside the standard CRUD surface belong here.
/// </remarks>
public sealed class FolderRepository : BaseRepository<Folder, IFolder>, IFolderRepository
{
    /// <summary>
    /// Initialises a new repository bound to the given database.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public FolderRepository(string connectionString) : base(connectionString)
    {
    }
}
