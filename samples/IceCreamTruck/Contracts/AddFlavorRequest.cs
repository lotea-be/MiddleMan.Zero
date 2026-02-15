namespace IceCreamTruck.Contracts;

/// <summary>
/// Represents a request to add a new ice cream flavor.
/// </summary>
public sealed class AddFlavorRequest
{
    /// <summary>
    /// The name of the new ice cream flavor to add.
    /// </summary>
    public string FlavorName { get; init; } = string.Empty;
}

public sealed class Flavor
{
    public string Name { get; init; } = string.Empty;
}