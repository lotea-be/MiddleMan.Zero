namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering MiddleMan.Zero handlers in the service collection.
/// </summary>
public static class MiddleManZeroServiceCollectionExtensions
{
    /// <summary>
    /// Registers all MiddleMan.Zero handlers found in the current AppDomain assemblies.
    /// </summary>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <param name="lifetime">The lifetime to use for the registered handlers.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMiddleManZero(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var handlerInterfaceType = typeof(MiddleMan.Zero.Abstractions.IHandleAsync<>);
        var handlerWithResponseInterfaceType = typeof(MiddleMan.Zero.Abstractions.IHandleAsync<,>);

        var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == handlerInterfaceType ||
                     i.GetGenericTypeDefinition() == handlerWithResponseInterfaceType))
                .Select(i => new { HandlerType = t, InterfaceType = i }));

        foreach (var handler in handlerTypes)
        {
            services.Add(new ServiceDescriptor(handler.InterfaceType, handler.HandlerType, lifetime));
        }

        return services;
    }
}
