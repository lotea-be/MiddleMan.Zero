namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// Base class for all message types in the application.
/// </summary>
/// <remarks>
/// This abstract class provides common properties that all messages should have,
/// such as identifiers, timestamps, and descriptive information.
/// </remarks>
public abstract class MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBase"/> class.
    /// </summary>
    protected MessageBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBase"/> class with the specified message text.
    /// </summary>
    /// <param name="message">The human-readable message description.</param>
    protected MessageBase(string message)
    {
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBase"/> class with the specified message text and code.
    /// </summary>
    /// <param name="message">The human-readable message description.</param>
    /// <param name="code">A code that categorizes or identifies the message.</param>
    protected MessageBase(string message, string code)
    {
        Message = message;
        Code = code;
    }

    /// <summary>
    /// Gets the unique identifier for this message.
    /// Automatically initialized with a new GUID.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the correlation identifier used to track related messages.
    /// Automatically initialized with a new GUID.
    /// </summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the creation timestamp of this message.
    /// Automatically set to the current UTC time when instantiated.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the human-readable message description.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the code that can be used to categorize or identify the message type.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Returns the human-readable <see cref="Message"/> text for this instance.
    /// </summary>
    /// <returns>The value of <see cref="Message"/>.</returns>
    public override string ToString() => Message;
}
