namespace MiddleMan.Zero;

/// <summary>
/// Represents a message indicating a failure during processing.
/// </summary>
public class FailureMessage : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FailureMessage"/> class.
    /// </summary>
    public FailureMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FailureMessage"/> class with the specified message.
    /// </summary>
    /// <param name="message">The failure message text.</param>
    public FailureMessage(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FailureMessage"/> class with the specified message and code.
    /// </summary>
    /// <param name="message">The failure message text.</param>
    /// <param name="code">A code that categorizes the failure.</param>
    public FailureMessage(string message, string code)
        : base(message, code)
    {
    }
}
