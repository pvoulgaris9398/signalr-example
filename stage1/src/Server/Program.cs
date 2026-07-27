using System.Net.WebSockets;
using Server.Models;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<EventStore>();

builder.Services.AddSingleton<ConnectionManager>();

builder.Services.AddSingleton<BroadcastService>();

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
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var manager = context.RequestServices.GetRequiredService<ConnectionManager>();

        using var socket = await context.WebSockets.AcceptWebSocketAsync();

        var connection = new ClientConnection { Socket = socket };

        manager.Add(connection);

        Console.WriteLine($"Client connected ({manager.Count} total)");

        var buffer = new byte[4096];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            connection.LastSeenUtc = DateTime.UtcNow;
        }

        manager.Remove(connection.Id);

        Console.WriteLine($"Client disconnected ({manager.Count} total)");

        await socket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Closed",
            CancellationToken.None
        );
    }
);

app.Run();
