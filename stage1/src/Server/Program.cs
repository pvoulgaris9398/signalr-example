using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

        using WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();

        Console.WriteLine("Socket Connected");

        var buffer = new byte[4096];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Socket Closed");

                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closed",
                    CancellationToken.None
                );

                break;
            }

            var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

            Console.WriteLine($"Received: {message}");

            var bytes = System.Text.Encoding.UTF8.GetBytes($"Echo: {message}");

            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
);

app.Run();
