namespace MiddleMan.Zero;

/// <summary>
/// Represents a message indicating that the requested resource was not found.
/// </summary>
public class NotFoundMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundMessage"/> class.
    /// </summary>
    public NotFoundMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundMessage"/> class with the specified message.
    /// </summary>
    /// <param name="message">The not-found message text.</param>
    public NotFoundMessage(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundMessage"/> class with the specified message and code.
    /// </summary>
    /// <param name="message">The not-found message text.</param>
    /// <param name="code">A code that categorizes the not-found condition.</param>
    public NotFoundMessage(string message, string code)
        : base(message, code)
    {
    }
}
