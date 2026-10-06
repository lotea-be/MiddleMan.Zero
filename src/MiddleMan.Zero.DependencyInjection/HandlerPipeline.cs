using MiddleMan.Zero.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builds the service factory for one (handler interface, handler implementation) pair.
/// The factory resolves the concrete handler and wraps it in any registered behaviors.
/// </summary>
internal abstract class HandlerFactory(Type handlerType)
{
    public Type HandlerType { get; } = handlerType;

    public abstract object Create(IServiceProvider serviceProvider);

    public static HandlerFactory For(Type interfaceType, Type handlerType)
    {
        var arguments = interfaceType.GetGenericArguments();
        var factoryType = arguments.Length == 1
            ? typeof(HandlerFactory<>).MakeGenericType(arguments)
            : typeof(HandlerFactory<,>).MakeGenericType(arguments);

        return (HandlerFactory)Activator.CreateInstance(factoryType, handlerType)!;
    }
}

internal sealed class HandlerFactory<TRequest>(Type handlerType) : HandlerFactory(handlerType)
{
    public override object Create(IServiceProvider serviceProvider)
    {
        var handler = (IHandleAsync<TRequest>)serviceProvider.GetRequiredService(HandlerType);
        var behaviors = serviceProvider.GetServices<IHandlerBehavior<TRequest>>().ToArray();

        return behaviors.Length == 0 ? handler : new HandlerPipeline<TRequest>(handler, behaviors);
    }
}

internal sealed class HandlerFactory<TRequest, TResponse>(Type handlerType) : HandlerFactory(handlerType)
{
    public override object Create(IServiceProvider serviceProvider)
    {
        var handler = (IHandleAsync<TRequest, TResponse>)serviceProvider.GetRequiredService(HandlerType);
        var behaviors = serviceProvider.GetServices<IHandlerBehavior<TRequest, TResponse>>().ToArray();

        return behaviors.Length == 0 ? handler : new HandlerPipeline<TRequest, TResponse>(handler, behaviors);
    }
}

/// <summary>
/// Runs a handler through its behaviors; the first behavior is the outermost.
/// </summary>
internal sealed class HandlerPipeline<TRequest>(IHandleAsync<TRequest> handler, IHandlerBehavior<TRequest>[] behaviors)
    : IHandleAsync<TRequest>
{
    public Task<ResultBase> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        HandlerDelegate next = ct => handler.HandleAsync(request, ct);

        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var inner = next;
            next = ct => behavior.HandleAsync(request, inner, ct);
        }

        return next(cancellationToken);
    }
}

/// <summary>
/// Runs a handler through its behaviors; the first behavior is the outermost.
/// </summary>
internal sealed class HandlerPipeline<TRequest, TResponse>(
    IHandleAsync<TRequest, TResponse> handler,
    IHandlerBehavior<TRequest, TResponse>[] behaviors)
    : IHandleAsync<TRequest, TResponse>
{
    public Task<ResultBase<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        HandlerDelegate<TResponse> next = ct => handler.HandleAsync(request, ct);

        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var inner = next;
            next = ct => behavior.HandleAsync(request, inner, ct);
        }

        return next(cancellationToken);
    }
}
