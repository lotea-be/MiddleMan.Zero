namespace IceCreamTruck.Contracts;

/// <summary>
/// Represents a customer order.
/// </summary>
public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CustomerName { get; init; }
    public required List<IceCream> Items { get; init; }
    public DateTime OrderedAt { get; init; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; }
    public decimal TotalPrice => Items.Sum(i => i.Price);
}
