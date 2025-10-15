namespace MiddleMan.Zero;

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
    /// Gets a value indicating whether the request is valid.
    /// Returns false if any messages of type <see cref="InvalidRequestMessage"/> are present in the context.
    /// </summary>
    public bool IsRequestValid { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccessful { get; private set; } = true;

    /// <summary>
    /// Gets all messages of the specified type from the context.
    /// </summary>
    /// <typeparam name="TMessage">The type of messages to retrieve.</typeparam>
    /// <returns>An enumerable collection of messages of the specified type.</returns>
    internal IEnumerable<TMessage> Get<TMessage>()
        where TMessage : MessageBase
        => _messages.OfType<TMessage>();

    /// <summary>
    /// Gets all messages from the context.
    /// </summary>
    /// <returns>An enumerable collection of all messages.</returns>
    internal IEnumerable<MessageBase> GetAllMessages()
        => _messages;

    /// <summary>
    /// Logs a debug message to the context.
    /// </summary>
    /// <param name="message">the debug message to log.</param>
    public void Log(DebugMessage message)
    {
        LogMessage(message);
    }

    /// <summary>
    /// Logs an invalid request message to the context and marks the request as invalid.
    /// </summary>
    /// <param name="message">the invalid request message to log.</param>
    public void Log(InvalidRequestMessage message)
    {
        IsRequestValid = false;
        IsSuccessful = false;
        LogMessage(message);
    }

    /// <summary>
    /// Logs a failure message to the context and marks the operation as unsuccessful.
    /// </summary>
    /// <param name="message">the failure message to log.</param>
    public void Log(FailureMessage message)
    {
        IsSuccessful = false;
        LogMessage(message);
    }

    private void LogMessage(MessageBase message)
    {
        _messages.Add(message);
    }
}
