namespace Server.Models;

public sealed class AckMessage : SocketMessage
{
    public long Sequence { get; init; }
}
