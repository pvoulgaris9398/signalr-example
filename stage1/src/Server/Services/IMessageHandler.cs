using Server.Models;

namespace Server.Services;

public interface IMessageHandler
{
    string MessageType { get; }

    Task HandleAsync(ClientConnection connection, string json, CancellationToken cancellationToken);
}
