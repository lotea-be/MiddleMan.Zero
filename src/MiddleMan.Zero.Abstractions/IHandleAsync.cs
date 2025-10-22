namespace MiddleMan.Zero.Abstractions;

/// <summary>
/// Represents a handler for processing requests asynchronously.
/// This interface is used to define a contract for handling requests of type <typeparamref name="TRequest" />.
/// Implementations of this interface should provide the logic to process the request.
/// </summary>
/// <typeparam name="TRequest">The type of the request to be handled.</typeparam>
public interface IHandleAsync<TRequest>
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{ResultBase}"/> containing the result of the operation.</returns>
    ValueTask<ResultBase> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a handler for processing requests asynchronously.
/// This interface is used to define a contract for handling requests of type <typeparamref name="TRequest" />.
/// It also defines a response type <typeparamref name="TResponse" /> that will be returned after processing the request.
/// Implementations of this interface should provide the logic to process the request and return a response.
/// </summary>
/// <typeparam name="TRequest">The type of the request to be handled.</typeparam>
/// <typeparam name="TResponse">The type of the response to be returned.</typeparam>
public interface IHandleAsync<TRequest, TResponse>
{
    /// <summary>
    /// Handles the specified request asynchronously and returns a <see cref="ResultBase{TResponse}" /> />.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> containing the result of the operation response of type <see cref="ResultBase{TResponse}"/>.</returns>
    ValueTask<ResultBase<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}