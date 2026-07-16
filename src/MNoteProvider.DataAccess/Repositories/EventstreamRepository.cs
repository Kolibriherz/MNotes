using System.Text.Json;
using Dapper;
using MNoteProvider.Common.Abstractions.Events;
using Npgsql;


namespace MNoteProvider.DataAccess.Repositories;

/// <summary>
/// Provides append-only persistence for domain events.
/// </summary>
/// <remarks>
/// Unlike the entity repositories, the event stream offers no update or delete operations:
/// events record what has happened and are immutable once written. Every event is stored
/// with its concrete runtime type as a discriminator, and reads filter on that discriminator
/// so that a payload is never deserialised into the wrong type.
/// </remarks>
public interface IEventstreamRepository
{
    /// <summary>
    /// Appends an event to the stream.
    /// </summary>
    /// <typeparam name="T">The event type. Its runtime type name is stored as the discriminator.</typeparam>
    /// <param name="payload">The event to append. Serialised to JSON.</param>
    /// <param name="channel">The channel the event is published on.</param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// <see langword="true"/> if exactly one row was appended; otherwise <see langword="false"/>.
    /// </returns>
    Task<bool> CreateAsync<T>(T payload, string channel, CancellationToken ct = default) where T : IBaseEvent;

    /// <summary>
    /// Retrieves all events of type <typeparamref name="T"/> belonging to the given owner.
    /// </summary>
    /// <typeparam name="T">
    /// The event type to retrieve. Only rows whose stored discriminator matches this type are
    /// returned, so events of other types are never deserialised into <typeparamref name="T"/>.
    /// </typeparam>
    /// <param name="ownerId">The owner whose events are retrieved.</param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// The matching events, newest first. Empty if the owner has none of this type.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a stored payload cannot be deserialised into <typeparamref name="T"/>.
    /// </exception>
    Task<IEnumerable<T>> GetAllAsync<T>(Guid ownerId, CancellationToken ct = default) where T : IBaseEvent;

    /// <summary>
    /// Retrieves all events of type <typeparamref name="T"/> belonging to the given owner
    /// and published on the given channel.
    /// </summary>
    /// <typeparam name="T">The event type to retrieve. Used as a discriminator filter.</typeparam>
    /// <param name="ownerId">The owner whose events are retrieved.</param>
    /// <param name="channel">The channel to restrict the result to.</param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// The matching events, newest first. Empty if no event matches all three criteria.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a stored payload cannot be deserialised into <typeparamref name="T"/>.
    /// </exception>
    Task<IEnumerable<T>> GetAllByChannelAsync<T>(Guid ownerId, string channel, CancellationToken ct = default) where T : IBaseEvent;
}

/// <summary>
/// Dapper-backed, append-only event store against the <c>eventstream</c> table in PostgreSQL.
/// </summary>
/// <remarks>
/// Payloads are stored as JSON alongside the runtime type name of the event, which acts as a
/// discriminator. Reads filter on that column: without it, a query for one event type would
/// also return rows of another and fail on deserialisation, or worse, silently produce a
/// partially populated object.
/// <para>
/// The table is deliberately not exposed through <see cref="BaseRepository{TEntity, TInterface}"/>,
/// since update and delete would contradict the append-only semantics of an event log.
/// </para>
/// </remarks>
public sealed class EventstreamRepository : IEventstreamRepository
{
    private readonly string _connectionString;
    private const string InsertSql =
             "INSERT INTO eventstream (id, active, ownerid, channel, payloadtype, payload) " +
             "VALUES (@Id, @Active, @OwnerId, @Channel, @PayloadType, @Payload);";

    private const string SelectByOwnerAndTypeSql =
            "SELECT payload FROM eventstream " +
            "WHERE ownerid = @OwnerId AND payloadtype = @PayloadType AND active " +
            "ORDER BY doeom DESC;";

    private const string SelectByOwnerTypeAndChannelSql =
            "SELECT payload FROM eventstream " +
            "WHERE ownerid = @OwnerId AND payloadtype = @PayloadType AND channel = @Channel AND active " +
            "ORDER BY doeom DESC;";

    /// <summary>
    /// Initialises a new event store bound to the given database.
    /// </summary>
    /// <param name="connectionString">
    /// The PostgreSQL connection string. It is not validated here; a malformed value surfaces
    /// on the first attempt to open a connection.
    /// </param>
    public EventstreamRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    /// <inheritdoc />
    public async Task<bool> CreateAsync<T>(T payload, string channel, CancellationToken ct = default) where T : IBaseEvent
    {

        var payloadString = JsonSerializer.Serialize(payload, payload.GetType());
        await using var conn = new NpgsqlConnection(_connectionString);
        var insertedRowCount = await conn.ExecuteAsync
        (
            new CommandDefinition
            (
                InsertSql,
                new
                {
                    Id = payload.Id,
                    Active = true,
                    OwnerId = payload.OwnerId,
                    Channel = channel,
                    PayloadType = payload.GetType().Name,
                    Payload = payloadString
                }, cancellationToken: ct
            )
        ).ConfigureAwait(false);
        return insertedRowCount == 1;
    }
    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllAsync<T>(Guid ownerId, CancellationToken ct = default) where T : IBaseEvent
    {
        return await QueryPayloadsAsync<T>(SelectByOwnerAndTypeSql, new { OwnerId = ownerId, PayloadType = typeof(T).Name }, ct).ConfigureAwait(false);
    }
    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllByChannelAsync<T>(Guid ownerId, string channel, CancellationToken ct = default)
        where T : IBaseEvent
    {
        return await QueryPayloadsAsync<T>(SelectByOwnerTypeAndChannelSql, new { OwnerId = ownerId, PayloadType = typeof(T).Name, Channel = channel }, ct).ConfigureAwait(false);
    }

    private async Task<IEnumerable<T>> QueryPayloadsAsync<T>(string sql, object parameters, CancellationToken ct) where T : IBaseEvent
    {
        await using var conn = new NpgsqlConnection(_connectionString);

        var payloads = await conn.QueryAsync<string>(
            new CommandDefinition(sql, parameters, cancellationToken: ct))
            .ConfigureAwait(false);

        return payloads
            .Select(p => JsonSerializer.Deserialize<T>(p) ?? throw new InvalidOperationException($"Failed to deserialise payload as {typeof(T).Name}."))
            .ToArray();
    }

}
