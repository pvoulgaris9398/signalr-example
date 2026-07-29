using Server.Models;

namespace Server.Services;

public sealed class SocketDispatcher
{
    private readonly IMessageHandler _handler;

    public SocketDispatcher(IMessageHandler handler)
    {
        _handler = handler;
    }

    public Task DispatchAsync(
        ClientConnection connection,
        string json,
        CancellationToken cancellationToken
    )
    {
        return _handler.HandleAsync(connection, json, cancellationToken);
    }
}
