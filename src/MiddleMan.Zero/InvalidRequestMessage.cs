namespace MiddleMan.Zero;

/// <summary>
/// Represents a message indicating an invalid request.
/// </summary>
public class InvalidRequestMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestMessage"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message describing the invalid request.</param>
    public InvalidRequestMessage(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestMessage"/> class with the specified message and code.
    /// </summary>
    /// <param name="message">The message describing the invalid request.</param>
    /// <param name="code">The error code associated with the invalid request.</param>
    public InvalidRequestMessage(string message, string code)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }
}

public class FailureMessage : MessageBase
{

}