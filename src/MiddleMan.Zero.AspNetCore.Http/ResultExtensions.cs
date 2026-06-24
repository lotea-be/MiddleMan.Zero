using MiddleMan.Zero.Abstractions;

using Microsoft.AspNetCore.Http;

namespace MiddleMan.Zero.AspNetCore.Http;

/// <summary>
/// Extension methods for converting Result objects to IResult.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an appropriate IResult based on the ResultStatus.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IResult representing the result status and messages.</returns>
    public static IResult ToResult(this ResultBase result)
    {
        return result.ResultStatus switch
        {
            ResultStatus.Successful => Results.Ok(),
            ResultStatus.NotFound => Results.NotFound(new { messages = result.Messages }),
            ResultStatus.Invalid => Results.BadRequest(new { messages = result.Messages }),
            ResultStatus.Failure => Results.Problem(
                detail: JoinMessages(result.Messages),
                statusCode: 500),
            ResultStatus.Forbidden => Results.Forbid(),
            ResultStatus.Conflict => Results.Conflict(new { messages = result.Messages }),
            _ => Results.Problem(
                detail: JoinMessages(result.Messages),
                statusCode: 500),
        };
    }

    /// <summary>
    /// Converts a Result&lt;TResponse&gt; to an appropriate IResult based on the ResultStatus.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IResult representing the result status, response, and messages.</returns>
    public static IResult ToResult<TResponse>(this ResultBase<TResponse> result)
    {
        return result.ResultStatus switch
        {
            ResultStatus.Successful => Results.Ok(result.Response),
            ResultStatus.NotFound => Results.NotFound(new { messages = result.Messages }),
            ResultStatus.Invalid => Results.BadRequest(new { messages = result.Messages }),
            ResultStatus.Failure => Results.Problem(
                detail: JoinMessages(result.Messages),
                statusCode: 500),
            ResultStatus.Forbidden => Results.Forbid(),
            ResultStatus.Conflict => Results.Conflict(new { messages = result.Messages }),
            _ => Results.Problem(
                detail: JoinMessages(result.Messages),
                statusCode: 500)
        };
    }

    private static string JoinMessages(IEnumerable<MessageBase> messages)
        => string.Join("; ", messages.Select(m => m.Message));
}
