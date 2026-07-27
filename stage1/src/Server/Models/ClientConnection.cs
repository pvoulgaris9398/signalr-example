using System.Net.WebSockets;

namespace Server.Models;

public sealed class ClientConnection
{
    public Guid Id { get; } = Guid.NewGuid();

    public required WebSocket Socket { get; init; }

    public DateTime ConnectedUtc { get; } = DateTime.UtcNow;

    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;

    public long LastAcknowledgedSequence { get; set; }
}
