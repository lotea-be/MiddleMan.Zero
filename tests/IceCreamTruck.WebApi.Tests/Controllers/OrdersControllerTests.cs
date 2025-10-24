using System.Net;

using IceCreamTruck.Contracts;

using Microsoft.AspNetCore.Mvc.Testing;

namespace IceCreamTruck.WebApi.Tests.Controllers;

/// <summary>
/// Integration tests for OrdersController using WebApplicationFactory.
/// These tests focus on:
/// - Full HTTP pipeline testing with real routing and middleware
/// - Dependency injection with mocked handlers
/// - HTTP status code responses and JSON serialization
/// </summary>
public class OrdersControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    #region CreateOrder Tests

    [Fact]
    public async Task CreateOrder_ReturnsOk_WhenHandlerReturnsSuccessful()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "John Doe",
            Items = [new IceCream { Flavor = "Vanilla", Price = 5.00m, Scoops = 2 }]
        };

        using var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.StatusCode.ShouldBe(HttpStatusCode.OK),
            () => response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json")
        );

        var responseId = await response.Content.ReadFromJsonAsync<Guid>();
        responseId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenHandlerReturnsInvalid()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = []
        };

        using var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetOrder Tests

    [Fact]
    public async Task GetOrder_ReturnsOkWithOrder_WhenHandlerReturnsSuccessful()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Jane Smith",
            Items = [new IceCream { Flavor = "Chocolate", Price = 6.50m, Scoops = 3 }],
            Status = OrderStatus.Pending
        };

        using var client = _factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/orders", order);
        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.StatusCode.ShouldBe(HttpStatusCode.OK),
            () => response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json")
        );

        var responseOrder = await response.Content.ReadFromJsonAsync<Order>();
        responseOrder.ShouldSatisfyAllConditions(
            () => responseOrder.ShouldNotBeNull(),
            () => responseOrder!.Id.ShouldBe(orderId),
            () => responseOrder!.CustomerName.ShouldBe("Jane Smith")
        );
    }

    [Fact]
    public async Task GetOrder_ReturnsNotFound_WhenHandlerReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrder_ReturnsBadRequest_WhenHandlerReturnsInvalid()
    {
        // Arrange
        var orderId = Guid.Empty;

        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    #endregion
}
