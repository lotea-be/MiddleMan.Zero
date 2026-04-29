using IceCreamTruck.Contracts;
using Microsoft.AspNetCore.Mvc;
using MiddleMan.Zero.Abstractions;
using MiddleMan.Zero.AspNetCore.Http;

namespace IceCreamTruck.WebApi.Endpoints;

public static class FlavorEndpoints
{
    public static void MapFlavorEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/flavors", FlavorEndpoints.GetAsync);
        endpointRouteBuilder.MapPost("/flavors", FlavorEndpoints.AddAsync);
    }

    private static async Task<IResult> AddAsync([FromBody] AddFlavorRequest request, [FromServices] IHandleAsync<AddFlavorRequest> handler)
    {
        var result = await handler.HandleAsync(request);
        return result.ToResult();
    }

    private static async Task<IResult> GetAsync([FromServices] IHandleAsync<GetFlavorsRequest, Flavor[]> handler)
    {
        var request = new GetFlavorsRequest();
        var result = await handler.HandleAsync(request);
        return result.ToResult();
    }
}