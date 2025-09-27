namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// Represents a handler for processing requests asynchronously.
/// This interface is used to define a contract for handling requests of type <see cref="TRequest"/>.
/// Implementations of this interface should provide the logic to process the request.
/// </summary>
/// <typeparam name="TRequest">The type of the request to be handled.</typeparam>
public interface IHandleAsync<TRequest>
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>A <see cref="ValueTask"/> containing the result of the operation.</returns>
    ValueTask<ResultBase> HandleAsync(TRequest request);
}

/// <summary>
/// Represents a handler for processing requests asynchronously.
/// This interface is used to define a contract for handling requests of type <see cref="TRequest"/>.
/// It also defines a response type <see cref="TResponse"/> that will be returned after processing the request.
/// Implementations of this interface should provide the logic to process the request and return a response.
/// </summary>
/// <typeparam name="TRequest">The type of the request to be handled.</typeparam>
/// <typeparam name="TResponse">The type of the response to be returned.</typeparam>
public interface IHandleAsync<TRequest, TResponse>
{
    /// <summary>
    /// Handles the specified request asynchronously and returns a typed response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>A <see cref="ValueTask"/> containing the result of the operation with a strongly typed response.</returns>
    ValueTask<ResultBase<TResponse>> HandleAsync(TRequest request);
}
