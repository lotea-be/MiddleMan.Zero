using System.Net;

using IceCreamTruck.Contracts;

using Microsoft.AspNetCore.Mvc.Testing;

namespace IceCreamTruck.WebApi.Tests.Endpoints;

/// <summary>
/// Integration tests for FlavorEndpoints using WebApplicationFactory.
/// These tests focus on:
/// - Full HTTP pipeline testing with real routing and middleware
/// - Dependency injection with real handlers and repositories
/// - HTTP status code responses and JSON serialization
/// </summary>
public class FlavorEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task GetFlavors_ReturnsOk_WithSeededFlavors()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/flavors", TestContext.Current.CancellationToken);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.StatusCode.ShouldBe(HttpStatusCode.OK),
            () => response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json")
        );

        var flavors = await response.Content.ReadFromJsonAsync<Flavor[]>(TestContext.Current.CancellationToken);
        flavors.ShouldSatisfyAllConditions(
            () => flavors.ShouldNotBeNull(),
            () => flavors!.Length.ShouldBeGreaterThanOrEqualTo(3),
            () => flavors!.ShouldContain(f => f.Name == "Vanilla"),
            () => flavors!.ShouldContain(f => f.Name == "Chocolate"),
            () => flavors!.ShouldContain(f => f.Name == "Strawberry")
        );
    }

    [Fact]
    public async Task GetFlavors_ReturnsJsonArray()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/flavors", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var flavors = await response.Content.ReadFromJsonAsync<Flavor[]>(TestContext.Current.CancellationToken);
        flavors.ShouldNotBeNull();
        flavors!.ShouldAllBe(f => !string.IsNullOrWhiteSpace(f.Name));
    }
}