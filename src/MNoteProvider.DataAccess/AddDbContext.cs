using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MNoteProvider.DataAccess.Repositories;
using Npgsql;

namespace MNoteProvider.DataAccess;

/// <summary>
/// Provides the service collection extension that wires up the data access layer.
/// </summary>
public static class AddDbContextExtension
{
    /// <summary>
    /// Reads the database options from configuration, builds the connection string and
    /// registers every repository as a singleton.
    /// </summary>
    /// <param name="services">The service collection to register the repositories into.</param>
    /// <param name="config">The configuration providing the <c>Settings:DBConnection</c> section.</param>
    /// <returns>The service collection, for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <c>Settings:DBConnection</c> section is missing.
    /// </exception>
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration config)
    {
        var options = config.GetRequiredSection("Settings:DBConnection").Get<DbConnectionOptions>()
                      ?? throw new InvalidOperationException("Missing configuration section 'Settings:DBConnection'.");

        var connectionString = BuildConnectionString(options);

        services.AddSingleton<INoteRepository>(_ => new NoteRepository(connectionString));
        services.AddSingleton<IFolderRepository>(_ => new FolderRepository(connectionString));
        services.AddSingleton<ITagRepository>(_ => new TagRepository(connectionString));
        services.AddSingleton<INoteTagAssignmentRepository>(_ => new NoteTagAssignmentRepository(connectionString));
        services.AddSingleton<ICommentRepository>(_ => new CommentRepository(connectionString));
        services.AddSingleton<IEventstreamRepository>(sp => new EventstreamRepository(connectionString));
        return services;
    }

    private static string BuildConnectionString(DbConnectionOptions o) =>
        new NpgsqlConnectionStringBuilder
        {
            Host = o.Host,
            Port = o.Port,
            Database = o.Database,
            Username = o.Username,
            Password = o.Password,
        }.ConnectionString;
}
