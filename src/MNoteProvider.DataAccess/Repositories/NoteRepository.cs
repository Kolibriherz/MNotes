using MNoteProvider.Domain;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.DataAccess.Repositories;

/// <summary>
/// Provides persistence access for <see cref="INote"/> entities.
/// </summary>
/// <remarks>
/// This interface currently adds no members beyond <see cref="IBaseRepository{TInterface}"/>.
/// It exists as a named seam: consumers depend on <c>INoteRepository</c> rather than on the
/// generic base, so note-specific queries can be introduced without touching a single
/// call site.
/// </remarks>
public interface INoteRepository : IBaseRepository<INote>;

/// <summary>
/// Dapper-backed repository for <see cref="Note"/> entities, mapped to the table declared
/// by the <see cref="System.ComponentModel.DataAnnotations.Schema.TableAttribute"/> on <see cref="Note"/>.
/// </summary>
/// <remarks>
/// All behaviour is inherited from <see cref="BaseRepository{TEntity, TInterface}"/>.
/// Note-specific queries that fall outside the standard CRUD surface belong here.
/// </remarks>
public sealed class NoteRepository : BaseRepository<Note, INote>, INoteRepository
{
    /// <summary>
    /// Initialises a new repository bound to the given database.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public NoteRepository(string connectionString) : base(connectionString)
    {
    }
}


