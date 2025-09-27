namespace MiddleMan.Zero;

using MiddleMan.Zero.Abstractions;
using System.Threading.Tasks;

/// <inheritdoc/>
public abstract class HandlerBase<TRequest>() : IHandleAsync<TRequest>
{
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
    protected abstract ValueTask ValidateAsync(TRequest request, HandlerContext context);

    protected abstract ValueTask HandleAsync(TRequest request, HandlerContext context);
}

/// <inheritdoc/>
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

    protected abstract ValueTask ValidateAsync(TRequest request, HandlerContext context);

    protected abstract ValueTask<TResponse> HandleAsync(TRequest request, HandlerContext context);

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