using System.Text.Json;

namespace Server.Services;

public sealed class SocketDispatcher
{
    private readonly Dictionary<string, IMessageHandler> _handlers;

    private readonly IMessageHandler _fallback;

    public SocketDispatcher(IEnumerable<IMessageHandler> handlers)
    {
        _handlers = handlers
            .Where(h => h.MessageType != "*")
            .ToDictionary(h => h.MessageType, StringComparer.OrdinalIgnoreCase);

        _fallback = handlers.Single(h => h.MessageType == "*");
    }

    public async Task DispatchAsync(
        ClientConnection connection,
        string json,
        CancellationToken cancellationToken
    )
    {
        using var document = JsonDocument.Parse(json);

        var type = document.RootElement.GetProperty("type").GetString() ?? "";

        if (_handlers.TryGetValue(type, out var handler))
        {
            await handler.HandleAsync(connection, json, cancellationToken);

            return;
        }

        await _fallback.HandleAsync(connection, json, cancellationToken);
    }
}
