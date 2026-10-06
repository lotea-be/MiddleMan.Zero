using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering MiddleMan.Zero handlers in the service collection.
/// </summary>
public static class MiddleManZeroServiceCollectionExtensions
{
    /// <summary>
    /// Registers all MiddleMan.Zero handlers found in the currently loaded <see cref="AppDomain"/> assemblies.
    /// </summary>
    /// <remarks>
    /// Assemblies that fail to load all of their types (for example, due to missing transitive references)
    /// are skipped: the handlers in their loadable types are still registered. Calling this method multiple
    /// times is safe — duplicate (service-type, implementation-type) pairs are skipped.
    /// <para>
    /// Each handler is registered as its concrete type and behind its <c>IHandleAsync</c> interface(s).
    /// Resolving the interface wraps the handler in every registered
    /// <see cref="MiddleMan.Zero.Abstractions.IHandlerBehavior{TRequest}"/> /
    /// <see cref="MiddleMan.Zero.Abstractions.IHandlerBehavior{TRequest, TResponse}"/>, in registration
    /// order (first registered = outermost). Behaviors are resolved with the handler's lifetime.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <param name="lifetime">The lifetime to use for the registered handlers.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMiddleManZero(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        => services.AddMiddleManZero(AppDomain.CurrentDomain.GetAssemblies(), lifetime);

    /// <summary>
    /// Registers all MiddleMan.Zero handlers found in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <param name="assemblies">The assemblies to scan for handler implementations.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMiddleManZero(this IServiceCollection services, params Assembly[] assemblies)
        => services.AddMiddleManZero(assemblies, ServiceLifetime.Transient);

    /// <summary>
    /// Registers all MiddleMan.Zero handlers found in the specified assemblies with the given lifetime.
    /// </summary>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <param name="assemblies">The assemblies to scan for handler implementations.</param>
    /// <param name="lifetime">The lifetime to use for the registered handlers.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMiddleManZero(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        var handlerInterfaceType = typeof(MiddleMan.Zero.Abstractions.IHandleAsync<>);
        var handlerWithResponseInterfaceType = typeof(MiddleMan.Zero.Abstractions.IHandleAsync<,>);

        var handlerTypes = assemblies
            .SelectMany(GetLoadableTypes)
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.ContainsGenericParameters)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == handlerInterfaceType ||
                     i.GetGenericTypeDefinition() == handlerWithResponseInterfaceType))
                .Select(i => new { HandlerType = t, InterfaceType = i }));

        foreach (var handler in handlerTypes)
        {
            // The concrete handler is registered as itself so the interface factory can resolve it
            // (with its dependencies) before wrapping it in any registered IHandlerBehavior.
            if (!services.Any(d => d.ServiceType == handler.HandlerType))
            {
                services.Add(new ServiceDescriptor(handler.HandlerType, handler.HandlerType, lifetime));
            }

            // Skip duplicate (service-type, implementation-type) registrations so repeat calls are safe.
            if (services.Any(d => d.ServiceType == handler.InterfaceType && IsRegistrationFor(d, handler.HandlerType)))
            {
                continue;
            }

            var factory = HandlerFactory.For(handler.InterfaceType, handler.HandlerType);
            services.Add(new ServiceDescriptor(handler.InterfaceType, factory.Create, lifetime));
        }

        return services;
    }

    private static bool IsRegistrationFor(ServiceDescriptor descriptor, Type handlerType)
        => !descriptor.IsKeyedService
            && (descriptor.ImplementationType == handlerType
                || (descriptor.ImplementationFactory?.Target is HandlerFactory factory && factory.HandlerType == handlerType));

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
