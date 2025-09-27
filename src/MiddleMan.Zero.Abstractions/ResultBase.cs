namespace MiddleMan.Zero.Abstractions;

public abstract class ResultBase(ResultStatus resultStatus, IEnumerable<MessageBase> messages)
{
    public ResultStatus ResultStatus { get; } = resultStatus;

    public MessageBase[] Messages { get; } = [.. messages];
}

public abstract class ResultBase<TResponse>(TResponse? response, ResultStatus resultStatus, IEnumerable<MessageBase> messages)
    : ResultBase(resultStatus, messages)
{
    public TResponse? Response { get; } = response;
}
