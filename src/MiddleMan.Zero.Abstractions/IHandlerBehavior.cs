namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// Represents the next step in a handler pipeline: either the next <see cref="IHandlerBehavior{TRequest}"/>
/// or, at the end of the chain, the handler itself.
/// </summary>
/// <param name="cancellationToken">The cancellation token to pass to the next step.</param>
/// <returns>A <see cref="Task{ResultBase}"/> containing the result of the remaining pipeline.</returns>
public delegate Task<ResultBase> HandlerDelegate(CancellationToken cancellationToken);

/// <summary>
/// Represents the next step in a handler pipeline: either the next
/// <see cref="IHandlerBehavior{TRequest, TResponse}"/> or, at the end of the chain, the handler itself.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="cancellationToken">The cancellation token to pass to the next step.</param>
/// <returns>A <see cref="Task{T}"/> containing the result of the remaining pipeline.</returns>
public delegate Task<ResultBase<TResponse>> HandlerDelegate<TResponse>(CancellationToken cancellationToken);

/// <summary>
/// Wraps the execution of an <see cref="IHandleAsync{TRequest}"/> to add cross-cutting behavior
/// (logging, metrics, transactions, retries, …) without changing the handler.
/// </summary>
/// <remarks>
/// Behaviors are applied by <c>MiddleMan.Zero.DependencyInjection</c> to every handler resolved
/// through <c>IHandleAsync&lt;TRequest&gt;</c>. Register them as open or closed generics, e.g.
/// <c>services.AddTransient(typeof(IHandlerBehavior&lt;&gt;), typeof(LoggingBehavior&lt;&gt;))</c>.
/// The first registered behavior is the outermost one.
/// </remarks>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
public interface IHandlerBehavior<TRequest>
{
    /// <summary>
    /// Executes the behavior. Call <paramref name="next"/> to continue the pipeline, or return a
    /// result directly to short-circuit it.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next step in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="Task{ResultBase}"/> containing the result of the operation.</returns>
    Task<ResultBase> HandleAsync(TRequest request, HandlerDelegate next, CancellationToken cancellationToken);
}

/// <summary>
/// Wraps the execution of an <see cref="IHandleAsync{TRequest, TResponse}"/> to add cross-cutting
/// behavior (logging, metrics, transactions, retries, …) without changing the handler.
/// </summary>
/// <remarks>
/// Behaviors are applied by <c>MiddleMan.Zero.DependencyInjection</c> to every handler resolved
/// through <c>IHandleAsync&lt;TRequest, TResponse&gt;</c>. Register them as open or closed generics, e.g.
/// <c>services.AddTransient(typeof(IHandlerBehavior&lt;,&gt;), typeof(LoggingBehavior&lt;,&gt;))</c>.
/// The first registered behavior is the outermost one.
/// </remarks>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IHandlerBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Executes the behavior. Call <paramref name="next"/> to continue the pipeline, or return a
    /// result directly to short-circuit it.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next step in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="Task{T}"/> containing the result of the operation.</returns>
    Task<ResultBase<TResponse>> HandleAsync(TRequest request, HandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
