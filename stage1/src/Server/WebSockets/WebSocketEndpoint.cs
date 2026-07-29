using System.Net.WebSockets;
using System.Text;
using Server.Services;

namespace Server.WebSockets;

public sealed class WebSocketEndpoint
{
    private readonly ConnectionManager _manager;

    private readonly SocketDispatcher _dispatcher;

    public WebSocketEndpoint(ConnectionManager manager, SocketDispatcher dispatcher)
    {
        _manager = manager;
        _dispatcher = dispatcher;
    }

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var socket = await context.WebSockets.AcceptWebSocketAsync();

        var connection = new ClientConnection { Socket = socket };

        _manager.Add(connection);

        Console.WriteLine($"Connected {connection.Id}");

        var buffer = new byte[8192];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                connection.LastSeenUtc = DateTime.UtcNow;

                await _dispatcher.DispatchAsync(connection, json, CancellationToken.None);
            }
        }
        finally
        {
            _manager.Remove(connection.Id);

            Console.WriteLine($"Disconnected {connection.Id}");

            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closing",
                    CancellationToken.None
                );
            }
        }
    }
}
