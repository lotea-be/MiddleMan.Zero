namespace IceCreamTruck.Contracts;

/// <summary>
/// Message to retrieve an existing order by ID.
/// </summary>
public sealed class GetOrderRequest
{
    public required Guid OrderId { get; init; }
}
