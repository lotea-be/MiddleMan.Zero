using IceCreamTruck.Contracts;

namespace IceCreamTruck.Repositories;

/// <summary>
/// Defines the contract for a repository that manages ice cream flavors.
/// </summary>
public interface IFlavorRepository
{
    /// <summary>
    /// Adds a new ice cream flavor to the repository.
    /// </summary>
    /// <param name="flavorName">The name of the flavor to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(string flavorName, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all available ice cream flavors from the repository.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An array of all available flavors.</returns>
    Task<Flavor[]> GetAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves a specific ice cream flavor by name from the repository.
    /// </summary>
    /// <param name="flavorName">The name of the flavor to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The flavor if found; otherwise, null.</returns>
    Task<Flavor?> GetAsync(string flavorName, CancellationToken cancellationToken = default);
}