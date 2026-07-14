using Dapper;
using MNoteProvider.Domain;
using MNoteProvider.Domain.Abstractions;
using Npgsql;
using static Dapper.SqlMapper;

namespace MNoteProvider.DataAccess.Repositories;

/// <summary>
/// Defines persistence operations for <see cref="IComment"/> entities.
/// </summary>
/// <remarks>
/// The generic CRUD contract is inherited from <see cref="IBaseRepository{TInterface}"/>.
/// This specialized interface adds query operations that are specific to comments.
/// </remarks>
public interface ICommentRepository : IBaseRepository<IComment>
{
    /// <summary>
    /// Gets all comments associated with the specified note.
    /// </summary>
    /// <param name="noteId">The identifier of the note whose comments are retrieved.</param>
    /// <param name="ct">A token used to cancel the operation.</param>
    /// <returns>A task whose result contains all comments associated with the specified note.</returns>
    Task<IEnumerable<IComment>> GetAllByNoteAsync(Guid noteId, CancellationToken ct = default);
}

/// <summary>
/// Provides Dapper-backed persistence operations for <see cref="Comment"/> entities.
/// </summary>
/// <remarks>
/// Standard CRUD operations are inherited from <see cref="BaseRepository{TEntity, TInterface}"/>.
/// This repository adds queries that are specific to comments.
/// </remarks>
public sealed class CommentRepository :  BaseRepository<Comment, IComment>, ICommentRepository
{
    private static readonly string _noteIdColumn = ColumnNameOf(nameof(Comment.NoteId));

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentRepository"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string used to create database connections.</param>
    public CommentRepository(string connectionString):base(connectionString)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IComment>> GetAllByNoteAsync(Guid noteId, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        var rows = await conn.QueryAsync<Comment>
        (
            new CommandDefinition
            (
                SelectAllByNoteIdSql,
                new {noteId},
                cancellationToken: ct
            )

        ).ConfigureAwait(false);
        return rows;
    }

    /// <summary>
    /// Gets the SQL query that selects all comments associated with a note.
    /// </summary>
    internal static string SelectAllByNoteIdSql => $"SELECT * FROM {TableName} " +
                                                       $"WHERE {_noteIdColumn} = @{nameof(Comment.NoteId)};";
}
