using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Server.Models;
using Server.Services;

namespace Server.Handlers;

public sealed class ReplayHandler : MessageHandler<ReplayRequest>
{
    private readonly EventStore _eventStore;

    public ReplayHandler(EventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public override string MessageType => "replay";

    protected override async Task HandleAsync(
        ClientConnection connection,
        ReplayRequest message,
        CancellationToken cancellationToken
    )
    {
        var events = _eventStore.GetSince(message.LastSequence);

        foreach (var e in events)
        {
            var payload = new EventMessage
            {
                Type = "event",
                Sequence = e.Sequence,
                Timestamp = e.Timestamp,
                Message = e.Message,
            };

            var json = JsonSerializer.Serialize(payload);

            var bytes = Encoding.UTF8.GetBytes(json);

            await connection.Socket.SendAsync(
                bytes,
                WebSocketMessageType.Text,
                true,
                cancellationToken
            );
        }
    }
}
