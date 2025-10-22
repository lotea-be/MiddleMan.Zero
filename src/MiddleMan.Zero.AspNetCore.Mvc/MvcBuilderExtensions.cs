namespace MiddleMan.Zero.AspNetCore.Mvc;

/// <summary>
/// Extension methods for configuring MiddleMan.Zero services in an ASP.NET Core MVC application.
/// </summary>
public static class MvcBuilderExtensions
{
    /// <summary>
    /// Adds MiddleMan.Zero result filters to the MVC pipeline.
    /// This enables automatic conversion of Result and Result&lt;TResponse&gt; to IActionResult.
    /// </summary>
    /// <param name="builder">The IMvcBuilder to add the filter to.</param>
    /// <returns>The IMvcBuilder for method chaining.</returns>
    public static IMvcBuilder AddMiddleManZeroResults(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(options =>
        {
            options.Filters.Add<ResultFilter>();
        });

        return builder;
    }

    /// <summary>
    /// Adds MiddleMan.Zero result filters to the MVC Core pipeline.
    /// This enables automatic conversion of Result and Result&lt;TResponse&gt; to IActionResult.
    /// </summary>
    /// <param name="builder">The IMvcCoreBuilder to add the filter to.</param>
    /// <returns>The IMvcCoreBuilder for method chaining.</returns>
    public static IMvcCoreBuilder AddMiddleManZeroResults(this IMvcCoreBuilder builder)
    {
        builder.AddMvcOptions(options =>
        {
            options.Filters.Add<ResultFilter>();
        });

        return builder;
    }
}