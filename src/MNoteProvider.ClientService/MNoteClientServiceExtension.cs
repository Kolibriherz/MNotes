using Microsoft.Extensions.DependencyInjection;
using MNoteProvider.ClientService.Abstractions;
using MNoteProvider.ClientService.SignalRClient;
using MNoteProvider.Common;

namespace MNoteProvider.ClientService;
/// <summary>Provides dependency injection registration for the MNote client service.</summary>
public static class MNoteClientServiceExtension 
{
    /// <summary>Registers the MNote client service, note hub connection and related services.</summary>
    /// <param name="services">The service collection to register the services in.</param>
    /// <param name="baseUri">The base URI of the MNote API.</param>
    /// <returns>The service collection with the MNote client services registered.</returns>
    public static IServiceCollection AddMNoteClientService(this IServiceCollection services, Uri baseUri)
    {
        var hubAddress = new Uri(baseUri, MNotesRoutes.Hubs.Name);   
        services.AddSingleton(new NoteHubConOptions { HubAddress = hubAddress });
        services.AddSingleton<NoteHubCon>();
        services.AddSingleton<NoteEventRelay>();
        services.AddSingleton<INoteEventRelay>(sp => sp.GetRequiredService<NoteEventRelay>()); 
        
        services.AddHttpClient<IMNoteClientService, MNoteClientService>(c => c.BaseAddress = baseUri);
        services.AddHostedService<NoteHubConnectionStarter>();
        
        return services;
    }
}


