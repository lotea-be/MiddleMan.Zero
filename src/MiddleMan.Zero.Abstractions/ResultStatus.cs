namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// Defines the status of an operation result.
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// The result status is undefined.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// The operation was successful.
    /// </summary>
    Successful = 1,
    /// <summary>
    /// The operation failed.
    /// </summary>
    Failure = 2,
    /// <summary>
    /// The operation was invalid.
    /// </summary>
    Invalid = 3,
    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 4
}