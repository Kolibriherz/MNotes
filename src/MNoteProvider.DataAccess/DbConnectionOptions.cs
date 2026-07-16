namespace MNoteProvider.DataAccess;

/// <summary>
/// Strongly typed options bound from the <c>Settings:DBConnection</c> configuration section.
/// </summary>
public sealed class DbConnectionOptions
{
    /// <summary>The host name of the PostgreSQL server.</summary>
    public required string Host { get; init; }
    /// <summary>The port of the PostgreSQL server.</summary>
    public required int Port { get; init; }
    /// <summary>The name of the database.</summary>
    public required string Database { get; init; }
    /// <summary>The user name used to authenticate against the database.</summary>
    public required string Username { get; init; }
    /// <summary>The password used to authenticate against the database.</summary>
    public required string Password { get; init; }
}
