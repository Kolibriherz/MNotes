using Dapper;
using MNoteProvider.Domain;
using MNoteProvider.Domain.Abstractions;
using Npgsql;

namespace MNoteProvider.DataAccess.Repositories;

/// <summary>
/// Provides persistence access for <see cref="INoteTagAssignment"/> entities.
/// </summary>
/// <remarks>
/// Beyond the standard CRUD surface of <see cref="IBaseRepository{TInterface}"/>, this
/// interface exposes deletion by composite key, since an assignment is identified in practice
/// by the pair of note and tag it links rather than by its surrogate primary key.
/// </remarks>
public interface INoteTagAssignmentRepository : IBaseRepository<INoteTagAssignment>
{
    /// <summary>
    /// Deletes the assignment linking the given note and tag.
    /// </summary>
    /// <remarks>
    /// This overload addresses the row by its natural key. It complements, rather than replaces,
    /// <see cref="IBaseRepository{TInterface}.DeleteAsync(Guid, CancellationToken)"/>, which
    /// addresses the row by its surrogate primary key.
    /// </remarks>
    /// <param name="noteId">The note side of the assignment to remove.</param>
    /// <param name="tagId">The tag side of the assignment to remove.</param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// <see langword="true"/> if exactly one row was deleted;
    /// <see langword="false"/> if no assignment linked the given note and tag.
    /// </returns>
    Task<bool> DeleteAsync(Guid noteId, Guid tagId, CancellationToken ct = default);
}

/// <summary>
/// Dapper-backed repository for <see cref="NoteTagAssignment"/> entities, mapped to the table
/// declared by the <see cref="System.ComponentModel.DataAnnotations.Schema.TableAttribute"/>
/// on <see cref="NoteTagAssignment"/>.
/// </summary>
/// <remarks>
/// The standard CRUD surface is inherited from <see cref="BaseRepository{TEntity, TInterface}"/>.
/// Only deletion by composite key is implemented here, because the generic base addresses rows
/// solely by their primary key.
/// </remarks>
public sealed class NoteTagAssignmentRepository: BaseRepository<NoteTagAssignment, INoteTagAssignment>, INoteTagAssignmentRepository
{
    private static readonly string _noteIdColumn = ColumnNameOf(nameof(NoteTagAssignment.NoteId));
    private static readonly string _tagIdColumn = ColumnNameOf(nameof(NoteTagAssignment.TagId));
    /// <summary>
    /// Initialises a new repository bound to the given database.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public NoteTagAssignmentRepository(string connectionString) : base(connectionString)
    {
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid noteId, Guid tagId, CancellationToken ct = default)
    {

        await using var conn = new NpgsqlConnection(ConnectionString);
        var deletedRowCount = await conn.ExecuteAsync
        (
            new CommandDefinition
            (
                DeleteAssignmentSql,
                new { NoteId = noteId, TagId = tagId },
                cancellationToken: ct
            )
        ).ConfigureAwait(false);
        return deletedRowCount == 1;
    
    }

    /// <summary>
    /// The statement deleting the assignment identified by its natural key.
    /// The parameters <c>@NoteId</c> and <c>@TagId</c> must be supplied by the caller.
    /// </summary>
    internal static string DeleteAssignmentSql => $"DELETE FROM {TableName} " +
                                                       $"WHERE {_noteIdColumn} = @{nameof(NoteTagAssignment.NoteId)} " +
                                                       $"AND {_tagIdColumn} = @{nameof(NoteTagAssignment.TagId)};";
}
