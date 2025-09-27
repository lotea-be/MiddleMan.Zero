namespace MiddleMan.Zero;

/// <summary>
/// Represents the result of an operation without a specific response.
/// </summary>
/// <param name="resultStatus">The status of the operation result.</param>
/// <param name="messages">Collection of messages associated with the operation.</param>
public sealed class Result(
    ResultStatus resultStatus,
    IEnumerable<MessageBase> messages)
        : ResultBase(resultStatus, messages);

/// <summary>
/// Represents the result of an operation that includes a specific response.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="response">The response data.</param>
/// <param name="resultStatus">The status of the operation result.</param>
/// <param name="messages">Collection of messages associated with the operation.</param>
public sealed class Result<TResponse>(
    TResponse? response,
    ResultStatus resultStatus,
    IEnumerable<MessageBase> messages)
        : ResultBase<TResponse>(response, resultStatus, messages);