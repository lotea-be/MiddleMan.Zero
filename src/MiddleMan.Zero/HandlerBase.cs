namespace MiddleMan.Zero;

using MiddleMan.Zero.Abstractions;
using System.Threading.Tasks;

/// <inheritdoc/>
public abstract class HandlerBase<TRequest>() : IHandleAsync<TRequest>
{
    protected TRequest Request { get; private set; } = default!;

    /// <inheritdoc/>
    public async ValueTask HandleAsync(TRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        Request = request;

        await HandleAsync();
    }

    protected abstract ValueTask HandleAsync();
}

/// <inheritdoc/>
public abstract class HandlerBase<TRequest, TResponse>() : IHandleAsync<TRequest, TResponse>
{
    protected TRequest Request { get; private set; } = default!;

    /// <inheritdoc/>
    public async ValueTask<TResponse> HandleAsync(TRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        Request = request;

        return await HandleAsync();

    }

    protected abstract ValueTask<TResponse> HandleAsync();
}