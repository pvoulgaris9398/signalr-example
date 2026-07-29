using Server.Models;
using Server.Services;

namespace Server.Handlers;

/// <summary>
/// Temporary adapter around the existing MessageProcessor.
/// This lets us introduce a dispatcher without changing behavior.
/// </summary>
public sealed class MessageProcessorHandler : IMessageHandler
{
    private readonly MessageProcessor _processor;

    public MessageProcessorHandler(MessageProcessor processor)
    {
        _processor = processor;
    }

    public string MessageType => "*";

    public Task HandleAsync(
        ClientConnection connection,
        string json,
        CancellationToken cancellationToken
    )
    {
        return _processor.ProcessAsync(connection, json, cancellationToken);
    }
}
