namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// Represents the result of an operation without a specific response.
/// </summary>
/// <param name="resultStatus">The status of the operation result.</param>
/// <param name="messages">Collection of messages associated with the operation.</param>
public abstract class ResultBase(ResultStatus resultStatus, IEnumerable<MessageBase> messages)
{
    /// <summary>
    /// Gets the status of the operation result.
    /// </summary>
    public ResultStatus ResultStatus { get; } = resultStatus;

    /// <summary>
    /// Gets the collection of messages associated with the operation.
    /// </summary>
    public MessageBase[] Messages { get; } = [.. messages];
}

/// <summary>
/// Represents the result of an operation that includes a specific response.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="response">The response data.</param>
/// <param name="resultStatus">The status of the operation result.</param>
/// <param name="messages">Collection of messages associated with the operation.</param>
/// <exception cref="ArgumentNullException">Thrown when response is null and resultStatus is Successful.</exception>
public abstract class ResultBase<TResponse>(TResponse? response, ResultStatus resultStatus, IEnumerable<MessageBase> messages)
    : ResultBase(resultStatus, messages)
{
    /// <summary>
    /// Gets the response data associated with the result.
    /// </summary>
    public TResponse Response { get; } = resultStatus == ResultStatus.Successful && response is null
        ? throw new ArgumentNullException(nameof(response), "Response cannot be null when ResultStatus is Successful.")
        : response!;
}