using System.Net.WebSockets;
using System.Text.Json;
using Server.Models;
using Server.Services;

namespace Server.Handlers;

public sealed class PingHandler : MessageHandler<PingMessage>
{
    public override string MessageType => "ping";

    protected override async Task HandleAsync(
        ClientConnection connection,
        PingMessage message,
        CancellationToken cancellationToken
    )
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new PongMessage { Type = "pong" });

        await connection.Socket.SendAsync(
            bytes,
            WebSocketMessageType.Text,
            true,
            cancellationToken
        );

        connection.LastSeenUtc = DateTime.UtcNow;
    }
}
