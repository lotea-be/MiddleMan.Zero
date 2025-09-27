namespace MiddleMan.Zero;

using MiddleMan.Zero.Abstractions;
using System.Threading.Tasks;

/// <summary>
/// Base class for handlers that process requests without returning a specific response type.
/// Implements common validation and handling logic, with template methods for specific behaviors.
/// </summary>
/// <typeparam name="TRequest">The type of the request to handle.</typeparam>
public abstract class HandlerBase<TRequest>() : IHandleAsync<TRequest>
{
    /// <summary>
    /// Handles the specified request by validating it and then processing it if valid.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>A <see cref="ValueTask{ResultBase}"/> containing the result of the operation.</returns>
    public async ValueTask<ResultBase> HandleAsync(TRequest request)
    {
        var context = new HandlerContext();

        if (request == null)
        {
            context.LogMessage(new InvalidRequestMessage("Request is null.", "middleman_request_null"));

            return CreateResult(context);
        }

        // Validate request before handling
        await ValidateAsync(request, context);

        // Fail fast if the request is not valid
        if (!context.IsRequestValid)
        {
            return CreateResult(context);
        }

        await HandleAsync(request, context);

        return CreateResult(context);
    }

    /// <summary>
    /// Creates a result based on the current state of the handler context.
    /// </summary>
    /// <param name="context">The handler context containing messages and state.</param>
    /// <returns>A <see cref="Result"/> with the appropriate status and messages.</returns>
    private static Result CreateResult(HandlerContext context)
    {
        // Check for invalid
        if (!context.IsRequestValid)
        {
            return new(ResultStatus.Invalid, context.Get<InvalidRequestMessage>());
        }

        // Check for Failure
        if (!context.IsSuccesful)
        {
            return new(ResultStatus.Failure, context.Get<FailureMessage>());
        }

        return new(ResultStatus.Succesful, context.GetAllMessages());

    }

    /// <summary>
    /// Validates the request before processing.
    /// Derived classes should add validation errors to the Context if validation fails.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous validation operation.</returns>
    /// <summary>
    /// Validates the request before processing.
    /// Derived classes should implement validation logic and add validation errors to the Context if validation fails.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="context">The handler context for logging messages.</param>
    /// <returns>A ValueTask that represents the asynchronous validation operation.</returns>
    protected abstract ValueTask ValidateAsync(TRequest request, HandlerContext context);

    /// <summary>
    /// Handles the request after validation is complete.
    /// Derived classes should implement the request processing logic.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="context">The handler context for logging messages.</param>
    /// <returns>A ValueTask that represents the asynchronous handling operation.</returns>
    protected abstract ValueTask HandleAsync(TRequest request, HandlerContext context);
}

/// <summary>
/// Base class for handlers that process requests and return a specific response type.
/// Implements common validation and handling logic, with template methods for specific behaviors.
/// </summary>
/// <typeparam name="TRequest">The type of the request to handle.</typeparam>
/// <typeparam name="TResponse">The type of the response to return.</typeparam>
public abstract class HandlerBase<TRequest, TResponse>() : IHandleAsync<TRequest, TResponse>
    where TResponse : class
{
    /// <inheritdoc/>
    public async ValueTask<ResultBase<TResponse>> HandleAsync(TRequest request)
    {
        var context = new HandlerContext();

        if (request == null)
        {
            context.LogMessage(new InvalidRequestMessage("Request is null.", "middleman_request_null"));

            return CreateResult(null, context);
        }

        // Validate request before handling
        await ValidateAsync(request, context);

        // Fail fast if the request is not valid
        if (!context.IsRequestValid)
        {
            // Validation failed
            throw new InvalidOperationException($"Request validation failed: {string.Join(", ", context.Messages.Select(m => m.Message))}");
        }

        var response = await HandleAsync(request, context);

        return CreateResult(response, context);
    }

    /// <summary>
    /// Validates the request before processing.
    /// Derived classes should implement validation logic and add validation errors to the Context if validation fails.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="context">The handler context for logging messages.</param>
    /// <returns>A ValueTask that represents the asynchronous validation operation.</returns>
    protected abstract ValueTask ValidateAsync(TRequest request, HandlerContext context);

    /// <summary>
    /// Handles the request after validation is complete and returns a response.
    /// Derived classes should implement the request processing logic and return the appropriate response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="context">The handler context for logging messages.</param>
    /// <returns>A ValueTask containing the response.</returns>
    protected abstract ValueTask<TResponse> HandleAsync(TRequest request, HandlerContext context);

    /// <summary>
    /// Creates a result with response based on the current state of the handler context.
    /// </summary>
    /// <param name="response">The response to include in the result.</param>
    /// <param name="context">The handler context containing messages and state.</param>
    /// <returns>A <see cref="Result{TResponse}"/> with the appropriate status, messages, and response.</returns>
    private static Result<TResponse> CreateResult(TResponse? response, HandlerContext context)
    {
        // Check for invalid
        if (!context.IsRequestValid)
        {
            return new(null, ResultStatus.Invalid, context.Get<InvalidRequestMessage>());
        }

        // Check for Failure
        if (!context.IsSuccesful)
        {
            return new(response, ResultStatus.Failure, context.Get<FailureMessage>());
        }

        if (response == null)
        {
            throw new ArgumentNullException(nameof(response), "Response is null.");
        }

        return new(response, ResultStatus.Succesful, context.GetAllMessages());
    }
}