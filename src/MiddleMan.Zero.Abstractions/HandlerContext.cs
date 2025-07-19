namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// Represents the context for a handler, allowing it to log messages.
/// </summary>
public class HandlerContext
{
    private readonly List<MessageBase> _messages = [];

    /// <summary>
    /// Gets the collection of messages logged by the handler.
    /// </summary>
    public IEnumerable<MessageBase> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Logs a message to the handler context.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void LogMessage(MessageBase message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        _messages.Add(message);
    }
}
