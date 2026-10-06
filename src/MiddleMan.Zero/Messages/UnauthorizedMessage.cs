namespace MiddleMan.Zero;

/// <summary>
/// Represents a message indicating that the caller must be authenticated to perform the operation.
/// </summary>
public class UnauthorizedMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedMessage"/> class.
    /// </summary>
    public UnauthorizedMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedMessage"/> class with the specified message.
    /// </summary>
    /// <param name="message">The unauthorized message text.</param>
    public UnauthorizedMessage(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedMessage"/> class with the specified message and code.
    /// </summary>
    /// <param name="message">The unauthorized message text.</param>
    /// <param name="code">A code that categorizes the unauthorized condition.</param>
    public UnauthorizedMessage(string message, string code)
        : base(message, code)
    {
    }
}
