using Microsoft.AspNetCore.Mvc;

namespace MiddleMan.Zero.AspNetCore.Mvc;

/// <summary>
/// Extension methods for converting Result objects to IActionResult.
/// </summary>
public static class ResultExtensions
{
    private const string ProblemJsonContentType = "application/problem+json";

    /// <summary>
    /// Converts a Result to an appropriate IActionResult based on the ResultStatus.
    /// Non-success statuses produce an RFC 9457 problem detail body with
    /// <c>Content-Type: application/problem+json</c>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult representing the result status and messages.</returns>
    public static IActionResult ToActionResult(this ResultBase result)
    {
        if (result.ResultStatus == ResultStatus.Successful)
        {
            return new OkResult();
        }

        var problem = ProblemResponse.FromResult(result);

        return new ObjectResult(problem) { StatusCode = problem.Status, ContentTypes = { ProblemJsonContentType } };
    }

    /// <summary>
    /// Converts a Result&lt;TResponse&gt; to an appropriate IActionResult based on the ResultStatus.
    /// Non-success statuses produce an RFC 9457 problem detail body with
    /// <c>Content-Type: application/problem+json</c>.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult representing the result status, response, and messages.</returns>
    public static IActionResult ToActionResult<TResponse>(this ResultBase<TResponse> result)
    {
        if (result.ResultStatus == ResultStatus.Successful)
        {
            return new OkObjectResult(result.Response);
        }

        var problem = ProblemResponse.FromResult(result);

        return new ObjectResult(problem) { StatusCode = problem.Status, ContentTypes = { ProblemJsonContentType } };
    }

    /// <summary>
    /// Converts a Result&lt;TResponse&gt; to an appropriate ActionResult&lt;TResponse&gt; based on the ResultStatus.
    /// Non-success statuses produce an RFC 9457 problem detail body with
    /// <c>Content-Type: application/problem+json</c>.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An ActionResult&lt;TResponse&gt; representing the result status, response, and messages.</returns>
    public static ActionResult<TResponse> ToTypedActionResult<TResponse>(this ResultBase<TResponse> result)
    {
        if (result.ResultStatus == ResultStatus.Successful)
        {
            return result.Response!;
        }

        var problem = ProblemResponse.FromResult(result);

        return new ObjectResult(problem) { StatusCode = problem.Status, ContentTypes = { ProblemJsonContentType } };
    }
}