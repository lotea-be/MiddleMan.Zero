namespace IceCreamTruck.Contracts;

/// <summary>
/// Status of an ice cream order.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Preparing,
    Ready,
    Delivered,
    Cancelled
}