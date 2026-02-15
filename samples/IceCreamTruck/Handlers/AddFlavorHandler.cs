using IceCreamTruck.Contracts;
using IceCreamTruck.Repositories;

using MiddleMan.Zero;

namespace IceCreamTruck.Handlers;

public sealed class AddFlavorHandler(IFlavorRepository flavorRepository) : HandlerBase<AddFlavorRequest>
{
    protected override Task HandleAsync(
        AddFlavorRequest message,
        HandlerContext context,
        CancellationToken cancellationToken = default)
    {
        return flavorRepository.AddAsync(message.FlavorName, cancellationToken);
    }

    protected override async Task ValidateAsync(AddFlavorRequest request, HandlerContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FlavorName))
        {
            context.Log(new InvalidRequestMessage("Flavor name is required."));
        }
        
        var flavor = await flavorRepository.GetAsync(request.FlavorName, cancellationToken);

        if (flavor != null)
        {
            context.Log(new InvalidRequestMessage($"Flavor '{request.FlavorName}' already exists."));
        }
    }
}