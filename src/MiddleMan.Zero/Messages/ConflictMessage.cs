namespace MiddleMan.Zero;

/// <summary>
/// Represents a message indicating that the operation conflicts with the current state of the resource.
/// </summary>
public class ConflictMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictMessage"/> class.
    /// </summary>
    public ConflictMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictMessage"/> class with the specified message.
    /// </summary>
    /// <param name="message">The conflict message text.</param>
    public ConflictMessage(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictMessage"/> class with the specified message and code.
    /// </summary>
    /// <param name="message">The conflict message text.</param>
    /// <param name="code">A code that categorizes the conflict condition.</param>
    public ConflictMessage(string message, string code)
        : base(message, code)
    {
    }
}
