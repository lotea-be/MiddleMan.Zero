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

    public bool IsSuccesful { get; private set; } = true;

    /// <summary>
    /// Logs a message to the handler context.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void LogMessage(MessageBase message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (IsRequestValid && message is InvalidRequestMessage)
        {
            IsRequestValid = false;
        }

        _messages.Add(message);
    }

    internal IEnumerable<TMessage> Get<TMessage>()
        where TMessage : MessageBase
        => _messages.OfType<TMessage>();

    internal IEnumerable<MessageBase> GetAllMessages()
        => _messages;
}
