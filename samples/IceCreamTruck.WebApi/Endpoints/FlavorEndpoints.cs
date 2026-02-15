using IceCreamTruck.Contracts;
using Microsoft.AspNetCore.Mvc;
using MiddleMan.Zero.Abstractions;
using MiddleMan.Zero.AspNetCore.Http;

namespace IceCreamTruck.WebApi.Actions;

public static class FlavorEndpoints
{
    public static async Task<IResult> AddAsync([FromBody] AddFlavorRequest request, [FromServices] IHandleAsync<AddFlavorRequest, Guid> handler)
    {
        var result = await handler.HandleAsync(request);
        return result.ToResult();
    }

    public static async Task<IResult> GetAsync([FromServices] IHandleAsync<GetFlavorsRequest, Flavor[]> handler)
    {
        var request = new GetFlavorsRequest();
        var result = await handler.HandleAsync(request);
        return result.ToResult();
    }
}