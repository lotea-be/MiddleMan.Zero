namespace MiddleMan.Zero;

/// <summary>
/// Represents a message indicating that the operation was forbidden due to insufficient permissions.
/// </summary>
public class ForbiddenMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenMessage"/> class.
    /// </summary>
    public ForbiddenMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenMessage"/> class with the specified message.
    /// </summary>
    /// <param name="message">The forbidden message text.</param>
    public ForbiddenMessage(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenMessage"/> class with the specified message and code.
    /// </summary>
    /// <param name="message">The forbidden message text.</param>
    /// <param name="code">A code that categorizes the forbidden condition.</param>
    public ForbiddenMessage(string message, string code)
        : base(message, code)
    {
    }
}
