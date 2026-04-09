using Microsoft.AspNetCore.Mvc;

namespace MiddleMan.Zero.AspNetCore.Mvc;

/// <summary>
/// Extension methods for converting Result objects to IActionResult.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an appropriate IActionResult based on the ResultStatus.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult representing the result status and messages.</returns>
    public static IActionResult ToActionResult(this ResultBase result)
    {
        return result.ResultStatus switch
        {
            ResultStatus.Successful => new OkResult(),
            ResultStatus.NotFound => new NotFoundObjectResult(new { messages = result.Messages }),
            ResultStatus.Invalid => new BadRequestObjectResult(new { messages = result.Messages }),
            ResultStatus.Failure => new ObjectResult(new { messages = result.Messages })
            {
                StatusCode = 500
            },
            ResultStatus.Forbidden => new ForbidResult(),
            _ => new ObjectResult(new { messages = result.Messages })
            {
                StatusCode = 500
            }
        };
    }

    /// <summary>
    /// Converts a Result&lt;TResponse&gt; to an appropriate IActionResult based on the ResultStatus.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IActionResult representing the result status, response, and messages.</returns>
    public static IActionResult ToActionResult<TResponse>(this ResultBase<TResponse> result)
    {
        return result.ResultStatus switch
        {
            ResultStatus.Successful => new OkObjectResult(result.Response),
            ResultStatus.NotFound => new NotFoundObjectResult(new { messages = result.Messages }),
            ResultStatus.Invalid => new BadRequestObjectResult(new { messages = result.Messages }),
            ResultStatus.Failure => new ObjectResult(new { messages = result.Messages })
            {
                StatusCode = 500
            },
            ResultStatus.Forbidden => new ForbidResult(),
            _ => new ObjectResult(new { messages = result.Messages })
            {
                StatusCode = 500
            }
        };
    }

    /// <summary>
    /// Converts a Result&lt;TResponse&gt; to an appropriate ActionResult&lt;TResponse&gt; based on the ResultStatus.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An ActionResult&lt;TResponse&gt; representing the result status, response, and messages.</returns>
    public static ActionResult<TResponse> ToTypedActionResult<TResponse>(this ResultBase<TResponse> result)
        where TResponse : class
    {
        return result.ResultStatus switch
        {
            ResultStatus.Successful => result.Response!,
            ResultStatus.NotFound => new NotFoundObjectResult(new { messages = result.Messages }),
            ResultStatus.Invalid => new BadRequestObjectResult(new { messages = result.Messages }),
            ResultStatus.Failure => new ObjectResult(new { messages = result.Messages })
            {
                StatusCode = 500
            },
            ResultStatus.Forbidden => new ForbidResult(),
            _ => new ObjectResult(new { messages = result.Messages })
            {
                StatusCode = 500
            }
        };
    }
}