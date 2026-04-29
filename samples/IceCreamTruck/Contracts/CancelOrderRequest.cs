namespace IceCreamTruck.Contracts;

/// <summary>
/// Message to cancel an existing ice cream order.
/// Only admin users are permitted to cancel orders.
/// </summary>
public sealed class CancelOrderRequest
{
    public required Guid OrderId { get; init; }

    /// <summary>
    /// Indicates whether the requester has admin privileges.
    /// Non-admin requests will result in a Forbidden response.
    /// </summary>
    public required bool IsAdminUser { get; init; }
}
