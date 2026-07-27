namespace Server.Models;

public sealed class AckMessage
{
    public string Type { get; init; } = "";

    public long Sequence { get; init; }
}
