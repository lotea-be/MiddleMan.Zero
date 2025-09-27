namespace MiddleMan.Zero.Abstractions;

public abstract class ResultBase(ResultStatus resultStatus, IEnumerable<MessageBase> messages)
{
    public ResultStatus ResultStatus { get; } = resultStatus;

    public MessageBase[] Messages { get; } = [.. messages];
}

/// <summary>
/// Represents the result of an operation that includes a specific response.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="response">The response data.</param>
/// <param name="resultStatus">The status of the operation result.</param>
/// <param name="messages">Collection of messages associated with the operation.</param>
public abstract class ResultBase<TResponse>(TResponse? response, ResultStatus resultStatus, IEnumerable<MessageBase> messages)
    : ResultBase(resultStatus, messages)
{
    /// <summary>
    /// Gets the response data associated with the result.
    /// </summary>
    public TResponse? Response { get; } = response;
}
