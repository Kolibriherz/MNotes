using MNoteProvider.BusinessCore;
using MNoteProvider.Common.Abstractions.Events;
using MNoteProvider.Endpoints;
using MNoteProvider.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpoints();
builder.Services.AddSingleton<INoteEventPublisher, SignalRNoteEventPublisher>();
builder.Services.AddBusinessCore(builder.Configuration);
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors =
        builder.Environment.IsDevelopment();
});

var app = builder.Build();
app.MapEndpoints();
app.MapHubs();

app.Run();
