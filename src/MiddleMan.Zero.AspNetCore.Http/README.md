# MiddleMan.Zero.AspNetCore.Http

ASP.NET Core Minimal API integration for MiddleMan.Zero, providing automatic conversion of `Result` and `Result<TResponse>` to `IResult`.

## Features

- **Extension Methods**: Convert `Result` objects to `IResult` for use in Minimal API endpoints
- **HTTP Status Code Mapping**:
  - `Successful` → 200 OK
  - `NotFound` → 404 Not Found
  - `Invalid` → 400 Bad Request
  - `Failure` → 500 Internal Server Error

## Installation

Add the package to your ASP.NET Core project:

```bash
dotnet add package MiddleMan.Zero.AspNetCore.Http
```

## Usage

Use the `ToResult()` extension method in your Minimal API endpoints to convert handler results to HTTP responses:

```csharp
using MiddleMan.Zero.AspNetCore.Http;

app.MapGet("/flavors", async (IHandleAsync<GetFlavorsRequest, Flavor[]> handler) =>
{
    var result = await handler.HandleAsync(new GetFlavorsRequest());
    return result.ToResult();
});

app.MapPost("/flavors", async (AddFlavorRequest request, IHandleAsync<AddFlavorRequest> handler) =>
{
    var result = await handler.HandleAsync(request);
    return result.ToResult();
});
```

You can also define endpoints as static methods for cleaner route registration:

```csharp
public static class FlavorEndpoints
{
    public static async Task<IResult> GetAsync(
        [FromServices] IHandleAsync<GetFlavorsRequest, Flavor[]> handler)
    {
        var request = new GetFlavorsRequest();
        var result = await handler.HandleAsync(request);
        return result.ToResult();
    }
}

// In Program.cs
app.MapGet("/flavors", FlavorEndpoints.GetAsync);
```

## Extension Methods

### `ToResult()`

Converts a `Result` (no response data) to an `IResult`:

```csharp
IResult httpResult = result.ToResult();
```

### `ToResult<TResponse>()`

Converts a `Result<TResponse>` to an `IResult`, including the response data on success:

```csharp
IResult httpResult = result.ToResult();
```

## HTTP Status Code Mapping

| ResultStatus | HTTP Status Code | Response Body |
|---|---|---|
| `Successful` (no data) | 200 OK | Empty |
| `Successful` (with data) | 200 OK | Response object |
| `NotFound` | 404 Not Found | `{ messages: [...] }` |
| `Invalid` | 400 Bad Request | `{ messages: [...] }` |
| `Failure` | 500 Internal Server Error | Problem details |
| `Undefined` | 500 Internal Server Error | Problem details |

## Examples

### Successful Response with Data
```csharp
var flavors = new[] { new Flavor { Name = "Vanilla" } };
var result = new Result<Flavor[]>(flavors, ResultStatus.Successful, []);
// Returns: 200 OK with flavors array in body
```

### Not Found Response
```csharp
var messages = new[] { new NotFoundMessage { Message = "Flavor not found" } };
var result = new Result<Flavor>(null, ResultStatus.NotFound, messages);
// Returns: 404 Not Found with { messages: [...] }
```

### Validation Error Response
```csharp
var messages = new[] { new InvalidRequestMessage("Flavor name is required.") };
var result = new Result(ResultStatus.Invalid, messages);
// Returns: 400 Bad Request with { messages: [...] }
```

## Comparison with MiddleMan.Zero.AspNetCore.Mvc

| Feature | Http | Mvc |
|---|---|---|
| Target | Minimal APIs (`IResult`) | MVC Controllers (`IActionResult`) |
| Extension method | `ToResult()` | `ToActionResult()` / `ToTypedActionResult()` |
| Typed result support | Via `Results.Ok(data)` | Via `ActionResult<T>` |

Use **Http** for Minimal API endpoints. Use **Mvc** for traditional MVC controllers.

## License

MIT
