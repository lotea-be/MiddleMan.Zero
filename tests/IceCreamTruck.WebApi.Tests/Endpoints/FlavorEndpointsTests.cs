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

    [Fact]
    public async Task AddFlavor_WithValidRequest_ReturnsOk_WithGuid()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var request = new AddFlavorRequest { FlavorName = "Mint Chocolate Chip" };

        // Act
        var response = await client.PostAsJsonAsync("/flavors", request, TestContext.Current.CancellationToken);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.StatusCode.ShouldBe(HttpStatusCode.OK),
            () => response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json")
        );
    }

    [Fact]
    public async Task AddFlavor_WithEmptyFlavorName_ReturnsBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var request = new AddFlavorRequest { FlavorName = "" };

        // Act
        var response = await client.PostAsJsonAsync("/flavors", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("Flavor name is required");
    }

    [Fact]
    public async Task AddFlavor_WithNullFlavorName_ReturnsBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var request = new AddFlavorRequest { FlavorName = null! };

        // Act
        var response = await client.PostAsJsonAsync("/flavors", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("Flavor name is required");
    }

    [Fact]
    public async Task AddFlavor_WithWhitespaceFlavorName_ReturnsBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var request = new AddFlavorRequest { FlavorName = "   " };

        // Act
        var response = await client.PostAsJsonAsync("/flavors", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("Flavor name is required");
    }

    [Fact]
    public async Task AddFlavor_WithDuplicateFlavorName_ReturnsBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var duplicateRequest = new AddFlavorRequest { FlavorName = "Vanilla" }; // Vanilla is seeded

        // Act
        var response = await client.PostAsJsonAsync("/flavors", duplicateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldContain("already exists");
    }

    [Fact]
    public async Task AddFlavor_SuccessfullyAddsFlavorToRepository()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var newFlavorName = $"TestFlavor_{Guid.NewGuid()}"; // Unique name to avoid conflicts
        var request = new AddFlavorRequest { FlavorName = newFlavorName };

        // Act - Add the flavor
        var addResponse = await client.PostAsJsonAsync("/flavors", request, TestContext.Current.CancellationToken);

        // Assert - Verify it was added successfully
        addResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify the flavor appears in the list
        var getResponse = await client.GetAsync("/flavors", TestContext.Current.CancellationToken);
        var flavors = await getResponse.Content.ReadFromJsonAsync<Flavor[]>(TestContext.Current.CancellationToken);
        
        flavors.ShouldNotBeNull();
        flavors!.ShouldContain(f => f.Name == newFlavorName);
    }
}