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
    /// Non-success statuses produce an RFC 9457 problem detail body with
    /// <c>Content-Type: application/problem+json</c>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IResult representing the result status and messages.</returns>
    public static IResult ToResult(this ResultBase result)
    {
        if (result.ResultStatus == ResultStatus.Successful)
        {
            return Results.Ok();
        }

        var problem = ProblemResponse.FromResult(result);

        return Results.Json(problem, contentType: "application/problem+json", statusCode: problem.Status);
    }

    /// <summary>
    /// Converts a Result&lt;TResponse&gt; to an appropriate IResult based on the ResultStatus.
    /// Non-success statuses produce an RFC 9457 problem detail body with
    /// <c>Content-Type: application/problem+json</c>.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IResult representing the result status, response, and messages.</returns>
    public static IResult ToResult<TResponse>(this ResultBase<TResponse> result)
    {
        if (result.ResultStatus == ResultStatus.Successful)
        {
            return Results.Ok(result.Response);
        }

        var problem = ProblemResponse.FromResult(result);

        return Results.Json(problem, contentType: "application/problem+json", statusCode: problem.Status);
    }
}
