namespace MiddleMan.Zero;

public sealed class Result(
    ResultStatus resultStatus,
    IEnumerable<MessageBase> messages)
        : ResultBase(resultStatus, messages);

public sealed class Result<TResponse>(
    TResponse? response,
    ResultStatus resultStatus,
    IEnumerable<MessageBase> messages)
        : ResultBase<TResponse>(response, resultStatus, messages);