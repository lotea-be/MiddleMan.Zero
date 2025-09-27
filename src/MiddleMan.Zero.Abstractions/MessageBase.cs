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
    /// Gets or sets the unique identifier for this message.
    /// Automatically initialized with a new GUID.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the correlation identifier used to track related messages.
    /// Automatically initialized with a new GUID.
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the creation timestamp of this message.
    /// Automatically set to the current UTC time when instantiated.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a human-readable message description.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a code that can be used to categorize or identify the message type.
    /// </summary>
    public string Code { get; set; } = string.Empty;
}