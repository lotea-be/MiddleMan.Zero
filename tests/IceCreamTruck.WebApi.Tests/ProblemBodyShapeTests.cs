using System.Net;
using System.Text.Json;

using IceCreamTruck.Contracts;

namespace IceCreamTruck.WebApi.Tests;

/// <summary>
/// End-to-end tests that verify the unified <c>application/problem+json</c> error envelope
/// is produced by both mappers through the real ASP.NET Core pipeline.
///
/// Controller actions exercise <c>ToActionResult()</c> (Mvc mapper).
/// Minimal API endpoints exercise <c>ToResult()</c> (Http mapper).
/// </summary>
public class ProblemBodyShapeTests(AuthenticatedWebApplicationFactory factory)
    : IClassFixture<AuthenticatedWebApplicationFactory>
{
    private readonly AuthenticatedWebApplicationFactory _factory = factory;
    private const string ProblemJsonMediaType = "application/problem+json";

    // -------------------------------------------------------------------------
    // MVC mapper -- controller action (OrdersController) non-success scenarios
    // -------------------------------------------------------------------------

    /// <summary>
    /// An Invalid result through the Mvc mapper (bad request for empty order ID)
    /// must return a body with type, title, status, detail, and messages fields.
    /// </summary>
    [Fact]
    public async Task MvcMapper_InvalidResult_ReturnsProblemJsonBodyShape()
    {
        // Arrange: Guid.Empty triggers InvalidRequestMessage in GetOrderHandler
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/orders/{Guid.Empty}", TestContext.Current.CancellationToken);

        // Assert -- HTTP 400
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Assert -- Content-Type is application/problem+json
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        mediaType.ShouldBe(ProblemJsonMediaType);

        // Assert -- body contains all required RFC 9457 fields
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("type", out _).ShouldBeTrue();
        root.TryGetProperty("title", out _).ShouldBeTrue();
        root.TryGetProperty("status", out var statusEl).ShouldBeTrue();
        statusEl.GetInt32().ShouldBe(400);
        root.TryGetProperty("detail", out _).ShouldBeTrue();
        root.TryGetProperty("messages", out _).ShouldBeTrue();
    }

    /// <summary>
    /// A Forbidden result through the Mvc mapper must return a body with all
    /// required fields (and must NOT be an empty 403 like the old ForbidResult).
    /// </summary>
    [Fact]
    public async Task MvcMapper_ForbiddenResult_ReturnsProblemJsonBodyShape()
    {
        // Arrange: create an order first, then attempt cancel without admin privileges
        var createRequest = new CreateOrderRequest
        {
            CustomerName = "Jane",
            Items = [new IceCream { Flavor = "Vanilla", Price = 3.0m, Scoops = 1 }]
        };
        using var client = _factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/orders", createRequest, TestContext.Current.CancellationToken);
        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/orders/{orderId}");
        deleteRequest.Headers.Add("X-Admin-User", "false");

        // Act
        var response = await client.SendAsync(deleteRequest, TestContext.Current.CancellationToken);

        // Assert -- HTTP 403
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // Assert -- Content-Type is application/problem+json (NOT empty like old ForbidResult)
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        mediaType.ShouldBe(ProblemJsonMediaType);

        // Assert -- body contains all required RFC 9457 fields
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("type", out _).ShouldBeTrue();
        root.TryGetProperty("title", out _).ShouldBeTrue();
        root.TryGetProperty("status", out var statusEl).ShouldBeTrue();
        statusEl.GetInt32().ShouldBe(403);
        root.TryGetProperty("detail", out _).ShouldBeTrue();
        root.TryGetProperty("messages", out _).ShouldBeTrue();
    }

    /// <summary>
    /// A NotFound result through the Mvc mapper must produce a problem body with all required fields.
    /// </summary>
    [Fact]
    public async Task MvcMapper_NotFoundResult_ReturnsProblemJsonBodyShape()
    {
        // Arrange: non-existent order ID
        using var client = _factory.CreateClient();
        var orderId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/orders/{orderId}", TestContext.Current.CancellationToken);

        // Assert -- HTTP 404
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Assert -- Content-Type is application/problem+json
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        mediaType.ShouldBe(ProblemJsonMediaType);

        // Assert -- body contains all required RFC 9457 fields
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("type", out _).ShouldBeTrue();
        root.TryGetProperty("title", out _).ShouldBeTrue();
        root.TryGetProperty("status", out var statusEl).ShouldBeTrue();
        statusEl.GetInt32().ShouldBe(404);
        root.TryGetProperty("detail", out _).ShouldBeTrue();
        root.TryGetProperty("messages", out _).ShouldBeTrue();
    }

    // -------------------------------------------------------------------------
    // Http mapper -- Minimal API endpoint (FlavorEndpoints) non-success scenarios
    // -------------------------------------------------------------------------

    /// <summary>
    /// An Invalid result through the Http mapper (empty flavor name) must return
    /// a body with type, title, status, detail, and messages fields.
    /// </summary>
    [Fact]
    public async Task HttpMapper_InvalidResult_ReturnsProblemJsonBodyShape()
    {
        // Arrange: empty FlavorName triggers InvalidRequestMessage in AddFlavorHandler
        using var client = _factory.CreateClient();
        var request = new AddFlavorRequest { FlavorName = "" };

        // Act
        var response = await client.PostAsJsonAsync("/flavors", request, TestContext.Current.CancellationToken);

        // Assert -- HTTP 400
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Assert -- Content-Type is application/problem+json
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        mediaType.ShouldBe(ProblemJsonMediaType);

        // Assert -- body contains all required RFC 9457 fields
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("type", out _).ShouldBeTrue();
        root.TryGetProperty("title", out _).ShouldBeTrue();
        root.TryGetProperty("status", out var statusEl).ShouldBeTrue();
        statusEl.GetInt32().ShouldBe(400);
        root.TryGetProperty("detail", out _).ShouldBeTrue();
        root.TryGetProperty("messages", out _).ShouldBeTrue();
    }
}
