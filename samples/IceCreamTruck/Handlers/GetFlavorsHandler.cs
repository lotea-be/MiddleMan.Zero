using IceCreamTruck.Contracts;
using IceCreamTruck.Repositories;

using MiddleMan.Zero;

namespace IceCreamTruck.Handlers;

/// <summary>
/// Handles retrieval of available ice cream flavors.
/// </summary>
public sealed class GetFlavorsHandler(IFlavorRepository flavorRepository) : HandlerBase<GetFlavorsRequest, Flavor[]>
{
    protected override async Task<Flavor[]?> HandleAsync(
        GetFlavorsRequest message,
        HandlerContext context,
        CancellationToken cancellationToken = default)
    {
        return await flavorRepository.GetAsync(cancellationToken);
    }

    protected override Task ValidateAsync(GetFlavorsRequest request, HandlerContext context, CancellationToken cancellationToken = default)
    {
        // No validation needed for this request
        return Task.CompletedTask;
    }
}
