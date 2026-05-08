using IceCreamTruck.Contracts;

namespace IceCreamTruck.Repositories;

internal class FlavorRepository : IFlavorRepository
{
    private readonly List<Flavor> _flavors = [
        new Flavor { Name = "Vanilla" },
        new Flavor { Name = "Chocolate" },
        new Flavor { Name = "Strawberry" }
    ];
    private readonly object _gate = new();

    public Task AddAsync(string flavorName, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _flavors.Add(new Flavor { Name = flavorName });
        }

        return Task.CompletedTask;
    }

    public Task<Flavor[]> GetAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_flavors.ToArray());
        }
    }

    public Task<Flavor?> GetAsync(string flavor, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var existingFlavor = _flavors.FirstOrDefault(f => string.Equals(f.Name, flavor, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(existingFlavor);
        }
    }
}
