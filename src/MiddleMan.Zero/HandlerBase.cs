namespace MiddleMan.Zero;

using System;
using System.Threading;
using System.Threading.Tasks;

using MiddleMan.Zero.Abstractions;

/// <inheritdoc/>
public abstract class HandlerBase<TRequest>() : IHandleAsync<TRequest>
{
    /// <inheritdoc/>
    public async Task<ResultBase> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var context = new HandlerContext();

        if (request == null)
        {
            context.Log(new InvalidRequestMessage("Request is null.", "middleman_request_null"));

            return CreateResult(context);
        }

        // Validate request before handling
        await ValidateAsync(request, context, cancellationToken);

        // Fail fast if the request is not valid
        if (!context.IsRequestValid)
        {
            return CreateResult(context);
        }

        await HandleAsync(request, context, cancellationToken);

        return CreateResult(context);
    }

    /// <summary>
    /// Validates the request before processing.
    /// Derived classes should add validation errors to the Context if validation fails.
    /// </summary>
    /// <returns>A Task that represents the asynchronous validation operation.</returns>
    /// <summary>
    /// Validates the request before processing.
    /// Derived classes should implement validation logic and add validation errors to the Context if validation fails.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="context">The handler context for logging messages.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A Task that represents the asynchronous validation operation.</returns>
    protected abstract Task ValidateAsync(TRequest request, HandlerContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles the request after validation is complete.
    /// Derived classes should implement the request processing logic.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="context">The handler context for logging messages.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A Task that represents the asynchronous handling operation.</returns>
    protected abstract Task HandleAsync(TRequest request, HandlerContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a result based on the current state of the handler context.
    /// </summary>
    /// <param name="context">The handler context containing messages and state.</param>
    /// <returns>A <see cref="Result"/> with the appropriate status and messages.</returns>
    private static Result CreateResult(HandlerContext context)
    {
        // Check for forbidden
        if (context.IsForbidden)
        {
            return new(ResultStatus.Forbidden, context.Get<ForbiddenMessage>());
        }
        
        // Check for invalid
        if (!context.IsRequestValid)
        {
            return new(ResultStatus.Invalid, context.Get<InvalidRequestMessage>());
        }

        // Check for Failure
        if (context.IsSuccessful)
        {
            return new(ResultStatus.Successful, context.GetAllMessages());
        }

        // Check for NotFound
        return context.IsNotFound
            ? new(ResultStatus.NotFound, context.Get<NotFoundMessage>())
            : new(ResultStatus.Failure, context.Get<FailureMessage>());
    }
}

/// <inheritdoc/>
public abstract class HandlerBase<TRequest, TResponse>() : IHandleAsync<TRequest, TResponse?>
{
    /// <inheritdoc/>
    public async Task<ResultBase<TResponse?>> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var context = new HandlerContext();

        if (request == null)
        {
            context.Log(new InvalidRequestMessage("Request is null.", "middleman_request_null"));

            return CreateResult(context);
        }

        // Validate request before handling
        await ValidateAsync(request, context, cancellationToken);

        // Fail fast if the request is not valid
        if (!context.IsRequestValid)
        {
            // Validation failed
            return CreateResult(context);
        }

        var response = await HandleAsync(request, context, cancellationToken);

        return CreateResult(context, response);
    }

    /// <summary>
    /// Validates the request before processing.
    /// Derived classes should implement validation logic and add validation errors to the Context if validation fails.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="context">The handler context for logging messages.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A Task that represents the asynchronous validation operation.</returns>
    protected abstract Task ValidateAsync(TRequest request, HandlerContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles the request after validation is complete and returns a response.
    /// Derived classes should implement the request processing logic and return the appropriate response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="context">The handler context for logging messages.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A Task containing the response.</returns>
    protected abstract Task<TResponse?> HandleAsync(TRequest request, HandlerContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a result with response based on the current state of the handler context.
    /// </summary>
    /// <param name="response">The response to include in the result.</param>
    /// <param name="context">The handler context containing messages and state.</param>
    /// <returns>A <see cref="Result{TResponse}"/> with the appropriate status, messages, and response.</returns>
    private static Result<TResponse?> CreateResult(HandlerContext context, TResponse? response = default)
    {
        // Check for invalid
        if (!context.IsRequestValid)
        {
            return new(default, ResultStatus.Invalid, context.Get<InvalidRequestMessage>());
        }

        // Check for Failure
        if (context.IsSuccessful)
        {
            return response == null
                ? throw new ArgumentNullException(nameof(response), "Response is null.")
                : new(response, ResultStatus.Successful, context.GetAllMessages());
        }

        return context.IsNotFound
            ? new(default, ResultStatus.NotFound, context.Get<NotFoundMessage>())
            : new(response, ResultStatus.Failure, context.Get<FailureMessage>());
    }
}