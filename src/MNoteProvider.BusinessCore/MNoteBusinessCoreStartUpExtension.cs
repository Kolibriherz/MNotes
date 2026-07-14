using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MNoteProvider.BusinessCore.Provider;
using MNoteProvider.DataAccess;

namespace MNoteProvider.BusinessCore;

/// <summary>
/// Provides extension methods for registering BusinessCore services with the dependency injection container.
/// </summary>
/// <remarks>
/// This extension registers the business-layer providers and their required data access services.
/// Consumers depend on provider interfaces rather than concrete implementations.
/// </remarks>
public static class MNoteBusinessCoreStartUpExtension
{
    /// <summary>
    /// Registers the business-layer providers and their required data access services.
    /// </summary>
    /// <param name="services">The service collection to add the BusinessCore services to.</param>
    /// <param name="configuration">The configuration used to configure the data access services.</param>
    /// <returns>The service collection, enabling further configuration through method chaining.</returns>
    public static IServiceCollection AddBusinessCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext(configuration);
        services.AddSingleton<INoteProvider, NoteProvider>();
        services.AddSingleton<ICommentProvider, CommentProvider>();
        services.AddSingleton<IFolderProvider, FolderProvider>();
        services.AddSingleton<ITagProvider, TagProvider>();
        services.AddSingleton<INoteTagAssignmentProvider, NoteTagAssignmentProvider>();
        
        return services;
    }   
}
