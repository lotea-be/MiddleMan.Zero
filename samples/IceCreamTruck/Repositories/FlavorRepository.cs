using IceCreamTruck.Contracts;

namespace IceCreamTruck.Repositories;

internal class FlavorRepository : IFlavorRepository
{
    private readonly List<Flavor> _flavors = [
        new Flavor { Name = "Vanilla" },
        new Flavor { Name = "Chocolate" },
        new Flavor { Name = "Strawberry" }
    ];

    public Task AddAsync(string flavorName, CancellationToken cancellationToken = default)
    {
        _flavors.Add(new Flavor { Name = flavorName });
        return Task.CompletedTask;
    }

    public Task<Flavor[]> GetAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_flavors.ToArray());
    }

    public Task<Flavor?> GetAsync(string flavor, CancellationToken cancellationToken = default)
    {
        var existingFlavor = _flavors.FirstOrDefault(f => string.Equals(f.Name, flavor, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(existingFlavor);
    }
}
