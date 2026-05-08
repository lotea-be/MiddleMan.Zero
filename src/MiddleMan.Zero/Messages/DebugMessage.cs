namespace MiddleMan.Zero;

/// <summary>
/// Represents a debug message for logging purposes.
/// </summary>
public class DebugMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DebugMessage"/> class.
    /// </summary>
    public DebugMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugMessage"/> class with the specified message.
    /// </summary>
    /// <param name="message">The debug message text.</param>
    public DebugMessage(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugMessage"/> class with the specified message and code.
    /// </summary>
    /// <param name="message">The debug message text.</param>
    /// <param name="code">A code that categorizes the message.</param>
    public DebugMessage(string message, string code)
        : base(message, code)
    {
    }
}
