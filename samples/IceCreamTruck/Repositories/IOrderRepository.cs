using IceCreamTruck.Contracts;

namespace IceCreamTruck.Repositories;

/// <summary>
/// Defines the contract for a repository that manages customer orders.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Adds a new order to the repository.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an order by its identifier.
    /// </summary>
    /// <param name="orderId">The identifier of the order to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The order if found; otherwise, <see langword="null"/>.</returns>
    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an existing order.
    /// </summary>
    /// <param name="orderId">The identifier of the order to cancel.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> if the order was found and cancelled; otherwise, <see langword="false"/>.</returns>
    Task<bool> CancelAsync(Guid orderId, CancellationToken cancellationToken = default);
}
