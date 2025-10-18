namespace IceCreamTruck.Contracts;

/// <summary>
/// Represents an ice cream product.
/// </summary>
public class IceCream
{
    public required string Flavor { get; init; }
    public required decimal Price { get; init; }
    public int Scoops { get; init; } = 1;
    public bool HasCone { get; init; } = true;
}
