using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Dapper;
using MNoteProvider.Common.Abstractions.Resources;
using MNoteProvider.Domain;
using MNoteProvider.Domain.Abstractions;
using MNoteProvider.Domain.Attributes;
using Npgsql;

namespace MNoteProvider.DataAccess.Repositories;

/// <summary>
/// Defines the standard CRUD operations shared by all repositories.
/// </summary>
/// <typeparam name="TInterface">
/// The domain abstraction exposed to callers. Consumers depend on this type,
/// never on the concrete persistence entity.
/// </typeparam>
public interface IBaseRepository<TInterface> where TInterface : IBaseDomainObject
{
    /// <summary>
    /// Retrieves a single entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key of the entity to retrieve.</param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// The matching entity, or <see langword="null"/> if no row with the given key exists.
    /// </returns>
    Task<TInterface?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>
    /// Retrieves all entities of this type.
    /// </summary>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// All rows of the underlying table, ordered by last modification date, newest first.
    /// The sequence is empty if the table contains no rows.
    /// </returns>
    Task<IEnumerable<TInterface>> GetAllAsync(CancellationToken ct = default);
    /// <summary>
    /// Deletes the entity with the given primary key.
    /// </summary>
    /// <param name="id">The primary key of the entity to delete.</param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// <see langword="true"/> if exactly one row was deleted;
    /// <see langword="false"/> if no matching row existed.
    /// </returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    /// <summary>
    /// Inserts a new entity into the underlying table.
    /// </summary>
    /// <param name="entity">
    /// The entity to persist. Only properties annotated with <see cref="ColumnAttribute"/>
    /// are written; all others are ignored.
    /// </param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// <see langword="true"/> if exactly one row was inserted; otherwise <see langword="false"/>.
    /// </returns>
    Task<bool> CreateAsync(TInterface entity, CancellationToken ct = default);
    /// <summary>
    /// Updates an existing entity, identified by its primary key.
    /// </summary>
    /// <param name="entity">
    /// The entity carrying the new values. Its <see cref="IBaseDomainObject.Id"/> selects the
    /// row to update and is never written. Properties marked with
    /// <see cref="ImmutableColumnAttribute"/> are likewise excluded from the SET clause.
    /// All remaining properties annotated with <see cref="ColumnAttribute"/> are overwritten
    /// with the values carried by <paramref name="entity"/>, so callers must supply a fully
    /// populated instance rather than a partial one.
    /// </param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// <see langword="true"/> if exactly one row was updated;
    /// <see langword="false"/> if no row with the given key exists.
    /// </returns>
    Task<bool> UpdateAsync(TInterface entity, CancellationToken ct = default);
}

/// <summary>
/// Provides a Dapper-backed implementation of the standard CRUD operations against PostgreSQL.
/// </summary>
/// <remarks>
/// Table and column names are derived from <see cref="TableAttribute"/> and
/// <see cref="ColumnAttribute"/> declared on <typeparamref name="TEntity"/>. The resulting
/// SQL fragments are computed once per closed generic type in the static constructor,
/// so the class behaves identically under any DI lifetime.
/// <para>
/// Instances carry no mutable state and are safe for concurrent use.
/// </para>
/// </remarks>
/// <typeparam name="TEntity">
/// The concrete, attribute-annotated entity that Dapper materialises rows into.
/// A concrete type is required because Dapper cannot map to interfaces.
/// </typeparam>
/// <typeparam name="TInterface">
/// The domain abstraction returned to callers.
/// </typeparam>
public abstract class BaseRepository<TEntity, TInterface> : IBaseRepository<TInterface>
    where TEntity : BaseDomainObject, TInterface
     where TInterface : IBaseDomainObject
{
    private static readonly string _idColumn;
    private static readonly string _doeomColumn;

    /// <summary>
    /// The PostgreSQL connection string used by every operation of this repository.
    /// </summary>
    protected readonly string ConnectionString;
    /// <summary>
    /// The database table backing <typeparamref name="TEntity"/>, taken from its
    /// <see cref="TableAttribute"/>. Exposed to derived repositories so that entity-specific
    /// statements can target the same table without repeating its name.
    /// </summary>
    internal static readonly string TableName;

    private static readonly List<PropertyInfo> _props;
    private static readonly string _columnsJoined;
    private static readonly string _paramsJoined;
    private static readonly List<PropertyInfo> _updatableProps;
    private static readonly string _updateSetClause;


    /// <summary>
    /// Initialises a new repository bound to the given database.
    /// </summary>
    /// <param name="connectionString">
    /// The PostgreSQL connection string. It is not validated here; a malformed value
    /// surfaces on the first attempt to open a connection.
    /// </param>
    protected BaseRepository(string connectionString)
    {
        ConnectionString = connectionString;
    }

    /// <summary>
    /// Reads the persistence metadata of <typeparamref name="TEntity"/> and precomputes the
    /// SQL fragments shared by every instance of this closed generic type.
    /// </summary>
    /// <remarks>
    /// The runtime invokes this exactly once per closed generic type, before first use,
    /// and guarantees thread safety. A missing <see cref="TableAttribute"/> is a programming
    /// error and fails fast here rather than surfacing as an obscure database error later.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="TEntity"/> carries no <see cref="TableAttribute"/>.
    /// The runtime wraps this in a <see cref="TypeInitializationException"/>.
    /// </exception>
    static BaseRepository()
    {

        TableName = typeof(TEntity).GetCustomAttribute<TableAttribute>()?.Name
            ?? throw new InvalidOperationException(ErrorMessages.NoDataBaseTableAttribute(typeof(TEntity).Name));
        _props = [.. typeof(TEntity).GetProperties()
           .Where(p => p.GetCustomAttribute<ColumnAttribute>()?.Name != null)
           .OrderBy(p => p.Name, StringComparer.Ordinal)];

        _idColumn = ColumnNameOf(nameof(IBaseDomainObject.Id));
        _doeomColumn = ColumnNameOf(nameof(IBaseDomainObject.Doeom));


        _columnsJoined = string.Join(", ", _props.Select(p => p.GetCustomAttribute<ColumnAttribute>()!.Name));
        _paramsJoined = string.Join(", ", _props.Select(p => "@" + p.Name));

        _updatableProps = [.. _props.Where(p => p.Name != nameof(IBaseDomainObject.Id) && p.GetCustomAttribute<ImmutableColumnAttribute>() is null)];

        _updateSetClause = string.Join(", ", _updatableProps
                                        .Select(d => d.GetCustomAttribute<ColumnAttribute>()?.Name + " = @" + d.Name));
    }

    /// <inheritdoc />
    public async Task<TInterface?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        var row = await conn.QuerySingleOrDefaultAsync<TEntity>
        (
            new CommandDefinition
            (
                SelectByIdSql,
                new { id }, cancellationToken: ct
            )
        ).ConfigureAwait(false);

        return row;
    }
    /// <inheritdoc />
    public async Task<IEnumerable<TInterface>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        var rows = await conn.QueryAsync<TEntity>
        (
            new CommandDefinition
            (
                SelectAllSql,
                cancellationToken: ct
            )

        ).ConfigureAwait(false);
        return rows;
    }
    /// <inheritdoc />
    public async Task<bool> CreateAsync(TInterface entity, CancellationToken ct = default)
    {
        var parameters = BuildInsertParameters(entity);
        await using var conn = new NpgsqlConnection(ConnectionString);

        var insertedRowCount = await conn.ExecuteAsync(
            new CommandDefinition
            (
                InsertSql,
                parameters,
                cancellationToken: ct
             )
        ).ConfigureAwait(false);

        return insertedRowCount == 1;
    }
    /// <inheritdoc />
    public async Task<bool> UpdateAsync(TInterface entity, CancellationToken ct = default)
    {
        var parameters = BuildUpdateParameters(entity);

        await using var conn = new NpgsqlConnection(ConnectionString);
        var updatedRowCount = await conn.ExecuteAsync
        (
            new CommandDefinition
            (
                UpdateSql,
                parameters,
                cancellationToken: ct
            )
        ).ConfigureAwait(false);
        return updatedRowCount == 1;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        var deletedRowCount = await conn.ExecuteAsync
        (
            new CommandDefinition
            (
                DeleteSql,
                new { id },
                cancellationToken: ct
            )
        ).ConfigureAwait(false);
        return deletedRowCount == 1;
    }

    /// <summary>
    /// Resolves the database column name of a property of <typeparamref name="TEntity"/> from its
    /// <see cref="ColumnAttribute"/>. Exposed so that derived repositories can build
    /// entity-specific statements without hard-coding column names.
    /// </summary>
    /// <param name="propertyName">The property name, best supplied via <c>nameof</c>.</param>
    /// <returns>
    /// The mapped column name, or <see langword="null"/> if the property carries a
    /// <see cref="ColumnAttribute"/> without a name.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no mapped property of that name exists on <typeparamref name="TEntity"/>.
    /// </exception>
    internal static string ColumnNameOf(string propertyName) => _props.Single(p => p.Name == propertyName).GetCustomAttribute<ColumnAttribute>()!.Name!;

    /// <summary>Builds the parameter set for an INSERT of the given entity.</summary>
    internal static DynamicParameters BuildInsertParameters(TInterface entity)
    {
        var parameters = new DynamicParameters();
        foreach (var p in _props)
            parameters.Add(p.Name, p.GetValue(entity));
        return parameters;
    }

    /// <summary>Builds the parameter set for an UPDATE, excluding immutable columns.</summary>
    internal static DynamicParameters BuildUpdateParameters(TInterface entity)
    {
        var parameters = new DynamicParameters();
        foreach (var p in _updatableProps)
            parameters.Add(p.Name, p.GetValue(entity));
        parameters.Add(nameof(IBaseDomainObject.Id), entity.Id);
        return parameters;
    }

    /// <summary>The SELECT statement for this entity type </summary>
    internal static string SelectAllSql => $"SELECT {_columnsJoined} FROM {TableName} ORDER BY {_doeomColumn} DESC;";

    /// <summary>The statement selecting the single row identified by its primary key. </summary>
    internal static string SelectByIdSql =>
        $"SELECT {_columnsJoined} FROM {TableName} WHERE {_idColumn} = @{nameof(IBaseDomainObject.Id)};";

    /// <summary>The INSERT statement for this entity type.</summary>
    internal static string InsertSql =>
        $"INSERT INTO {TableName} ({_columnsJoined}) VALUES ({_paramsJoined});";

    /// <summary>The UPDATE statement for this entity type identified by its primary key.</summary>
    internal static string UpdateSql =>
        $"UPDATE {TableName} SET {_updateSetClause} WHERE {_idColumn} = @{nameof(IBaseDomainObject.Id)};";

    /// <summary>The DELETE statement for this entity type identified by its primary key.</summary>
    internal static string DeleteSql =>
        $"DELETE FROM {TableName} WHERE {_idColumn} = @{nameof(IBaseDomainObject.Id)};";

}







