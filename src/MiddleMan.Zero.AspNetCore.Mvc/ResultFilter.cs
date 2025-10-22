namespace MiddleMan.Zero.AspNetCore.Mvc;

/// <summary>
/// A result filter that automatically converts Result and Result&lt;TResponse&gt; objects
/// to appropriate IActionResult instances.
/// </summary>
public class ResultFilter : IResultFilter
{
    /// <summary>
    /// Called before the action result executes.
    /// Converts Result objects to IActionResult if necessary.
    /// </summary>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Handle ObjectResult with ResultBase value
        if (context.Result is ObjectResult objectResult && objectResult.Value is ResultBase resultBase)
        {
            context.Result = ConvertResultToActionResult(resultBase);
        }
    }

    /// <summary>
    /// Called after the action result executes.
    /// </summary>
    public void OnResultExecuted(ResultExecutedContext context)
    {
        // No action needed after execution
    }

    private static IActionResult ConvertResultToActionResult(ResultBase result)
    {
        // Check if it's a generic result (ResultBase<T>)
        var resultType = result.GetType();

        // Check if the type or its base type is ResultBase<T>
        var baseType = resultType.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ResultBase<>))
            {
                // Use reflection to call ToActionResult<T>
                var responseType = baseType.GetGenericArguments()[0];

                // Find the generic ToActionResult method
                var method = typeof(ResultExtensions).GetMethods()
                    .FirstOrDefault(m => m.Name == nameof(ResultExtensions.ToActionResult)
                                      && m.IsGenericMethod
                                      && m.ReturnType == typeof(IActionResult));

                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(responseType);
                    return (IActionResult)genericMethod.Invoke(null, new[] { result })!;
                }
                break;
            }
            baseType = baseType.BaseType;
        }

        // Non-generic result
        return result.ToActionResult();
    }
}
