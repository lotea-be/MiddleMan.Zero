namespace IceCreamTruck.Contracts;

/// <summary>
/// Message to create a new ice cream order.
/// </summary>
public sealed class CreateOrderRequest{
    public required string CustomerName { get; init; }
    public required List<IceCream> Items { get; init; }
}
