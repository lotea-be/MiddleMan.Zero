namespace MiddleMan.Zero;

/// <summary>
/// Represents a message indicating an invalid request.
/// </summary>
public class InvalidRequestMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestMessage"/> class.
    /// </summary>
    public InvalidRequestMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestMessage"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message describing the invalid request.</param>
    public InvalidRequestMessage(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestMessage"/> class with the specified message and code.
    /// </summary>
    /// <param name="message">The message describing the invalid request.</param>
    /// <param name="code">The error code associated with the invalid request.</param>
    public InvalidRequestMessage(string message, string code)
        : base(message, code)
    {
    }
}
