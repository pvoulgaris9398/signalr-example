using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Server.Models;

namespace Server.Services;

public sealed class MessageProcessor
{
    private readonly EventStore _eventStore;

    public MessageProcessor(EventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task ProcessAsync(
        ClientConnection connection,
        string json,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var document = JsonDocument.Parse(json);

            var type = document.RootElement.GetProperty("type").GetString();

            switch (type)
            {
                case "ack":

                    await HandleAck(connection, document);

                    break;

                case "ping":

                    await SendAsync(
                        connection,
                        new PongMessage { Type = "pong" },
                        cancellationToken
                    );

                    break;

                case "replay":

                    await HandleReplay(connection, document, cancellationToken);

                    break;

                default:

                    Console.WriteLine($"Unknown message type {type}");

                    break;
            }
        }
        catch (JsonException)
        {
            Console.WriteLine($"\x1B[31mInvalid JSON from {connection.Id}: {json}\x1B[0m");
            //Console.WriteLine(ex);
            // Option 1: ignore it

            // Option 2: send an error message

            // Option 3: disconnect the client
        }
    }

    private Task HandleAck(ClientConnection connection, JsonDocument document)
    {
        var sequence = document.RootElement.GetProperty("sequence").GetInt64();

        connection.LastAcknowledgedSequence = sequence;

        Console.WriteLine($"Client {connection.Id} ACK {sequence}");

        return Task.CompletedTask;
    }

    private async Task HandleReplay(
        ClientConnection connection,
        JsonDocument document,
        CancellationToken cancellationToken
    )
    {
        var lastSequence = document.RootElement.GetProperty("lastSequence").GetInt64();

        var events = _eventStore.GetSince(lastSequence);

        foreach (var e in events)
        {
            await SendAsync(
                connection,
                new EventMessage
                {
                    Type = "event",
                    Sequence = e.Sequence,
                    Timestamp = e.Timestamp,
                    Message = e.Message,
                },
                cancellationToken
            );
        }
    }

    private static async Task SendAsync(
        ClientConnection connection,
        object message,
        CancellationToken cancellationToken
    )
    {
        var json = JsonSerializer.Serialize(message);

        var bytes = Encoding.UTF8.GetBytes(json);

        await connection.Socket.SendAsync(
            bytes,
            WebSocketMessageType.Text,
            true,
            cancellationToken
        );
    }
}
