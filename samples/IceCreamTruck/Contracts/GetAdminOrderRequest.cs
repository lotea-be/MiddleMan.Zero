namespace IceCreamTruck.Contracts;

/// <summary>
/// Message to retrieve an existing ice cream order with admin privileges.
/// Non-admin requests result in a Forbidden response.
/// </summary>
public sealed class GetAdminOrderRequest
{
    public required Guid OrderId { get; init; }

    /// <summary>
    /// Indicates whether the requester has admin privileges.
    /// Non-admin requests will result in a Forbidden response.
    /// </summary>
    public required bool IsAdminUser { get; init; }
}
