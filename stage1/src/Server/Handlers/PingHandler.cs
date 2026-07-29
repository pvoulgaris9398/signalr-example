using System.Text.Json;
using Server.Models;
using Server.Services;

namespace Server.Handlers;

public sealed class PingHandler : IMessageHandler
{
    public string MessageType => "ping";

    public async Task HandleAsync(
        ClientConnection connection,
        string json,
        CancellationToken cancellationToken
    )
    {
        await connection.Socket.SendAsync(
            JsonSerializer.SerializeToUtf8Bytes(new PongMessage { Type = "pong" }),
            System.Net.WebSockets.WebSocketMessageType.Text,
            true,
            cancellationToken
        );

        connection.LastSeenUtc = DateTime.UtcNow;

        Console.WriteLine($"Ping from {connection.Id}");
    }
}
