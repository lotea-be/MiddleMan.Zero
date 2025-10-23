using IceCreamTruck.Contracts;
using Microsoft.AspNetCore.Mvc;
using MiddleMan.Zero.Abstractions;
using MiddleMan.Zero.AspNetCore.Mvc;

namespace IceCreamTruck.WebApi.Controllers;

/// <summary>
/// Controller for managing ice cream orders.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController(
    ILogger<OrdersController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new ice cream order.
    /// </summary>
    /// <param name="request">The order details.</param>
    /// <param name="handler">The request handler.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">Order created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, [FromServices] IHandleAsync<CreateOrderRequest, Guid> handler)
    {
        logger.LogInformation("Creating order for customer: {CustomerName}", request.CustomerName);

        var result = await handler.HandleAsync(request);

        return result.ToActionResult();
    }

    /// <summary>
    /// Retrieves an order by ID.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <param name="handler">The request handler.</param>
    /// <returns>The order details.</returns>
    /// <response code="200">Order found.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, [FromServices] IHandleAsync<GetOrderRequest> handler)
    {
        logger.LogInformation("Retrieving order: {OrderId}", id);

        var request = new GetOrderRequest { OrderId = id };
        var result = await handler.HandleAsync(request);
        return result.ToActionResult();
    }
}
