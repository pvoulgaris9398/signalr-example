namespace Server.Models;

public abstract record SocketMessage
{
    public required string Type { get; init; }
}
