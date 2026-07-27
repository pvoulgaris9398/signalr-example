using System.Net.WebSockets;
using Server.Models;
using Server.Services;
using Server.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<EventStore>();

builder.Services.AddSingleton<ConnectionManager>();

builder.Services.AddSingleton<BroadcastService>();

builder.Services.AddHostedService<HeartbeatService>();

builder.Services.AddSingleton<MessageProcessor>();

builder.Services.AddSingleton<WebSocketEndpoint>();

var app = builder.Build();

app.UseRouting();

app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) });

app.MapControllers();

app.MapGet(
    "/",
    () =>
    {
        return Results.Text(
            """
WebSocket Demo

GET  /ws
POST /api/events
GET  /api/events?since=0
"""
        );
    }
);

app.Map(
    "/ws",
    async context =>
    {
        var endpoint = context.RequestServices.GetRequiredService<WebSocketEndpoint>();

        await endpoint.HandleAsync(context);
    }
);

app.Run();
