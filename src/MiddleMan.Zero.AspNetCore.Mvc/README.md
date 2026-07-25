# MiddleMan.Zero.AspNetCore.Mvc

ASP.NET Core MVC integration for MiddleMan.Zero, providing conversion of `Result` and
`Result<TResponse>` to `IActionResult` / `ActionResult<TResponse>`.

## Features

- **Extension methods** — `ToActionResult()` / `ToTypedActionResult()` convert a `Result` to the
  appropriate MVC action result.
- **Canonical error envelope** — every non-success status returns one RFC 9457 (`ProblemResponse`)
  body served as `application/problem+json`, identical to the body produced by
  `MiddleMan.Zero.AspNetCore.Http` (Minimal APIs).
- **HTTP status code mapping**:
  - `Successful` → 200 OK
  - `Invalid` → 400 Bad Request
  - `Forbidden` → 403 Forbidden
  - `NotFound` → 404 Not Found
  - `Conflict` → 409 Conflict
  - `Failure` / `Undefined` → 500 Internal Server Error

## Installation

Add the package to your ASP.NET Core project:

```bash
dotnet add package MiddleMan.Zero.AspNetCore.Mvc
```

## Usage

Call the extension method on the `Result` returned by your handler:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController(IHandleAsync<GetOrderRequest, Result<Order>> getOrderHandler)
    : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var result = await getOrderHandler.HandleAsync(new GetOrderRequest { Id = id });
        return result.ToActionResult();
    }

    [HttpGet("{id}/typed")]
    public async Task<ActionResult<Order>> GetOrderTyped(int id)
    {
        var result = await getOrderHandler.HandleAsync(new GetOrderRequest { Id = id });
        return result.ToTypedActionResult();
    }
}
```

## Extension Methods

### `ToActionResult()`

Converts a `Result` or `Result<TResponse>` to `IActionResult`:

```csharp
IActionResult actionResult = result.ToActionResult();
```

### `ToTypedActionResult<TResponse>()`

Converts a `Result<TResponse>` to `ActionResult<TResponse>` (better type safety for the success body):

```csharp
ActionResult<Order> actionResult = result.ToTypedActionResult();
```

## HTTP Status Code Mapping

| ResultStatus | HTTP Status Code | Response Body |
|---|---|---|
| `Successful` (no data) | 200 OK | Empty |
| `Successful` (with data) | 200 OK | Response object (`application/json`) |
| `Invalid` | 400 Bad Request | `ProblemResponse` (`application/problem+json`) |
| `Forbidden` | 403 Forbidden | `ProblemResponse` (`application/problem+json`) |
| `NotFound` | 404 Not Found | `ProblemResponse` (`application/problem+json`) |
| `Conflict` | 409 Conflict | `ProblemResponse` (`application/problem+json`) |
| `Failure` | 500 Internal Server Error | `ProblemResponse` (`application/problem+json`) |
| `Undefined` | 500 Internal Server Error | `ProblemResponse` (`application/problem+json`) |

## The error envelope (`ProblemResponse`)

Non-success results are serialized as an RFC 9457 (RFC 7807-compatible) problem document defined in
`MiddleMan.Zero.Abstractions`:

```jsonc
// HTTP 400, Content-Type: application/problem+json
{
  "type": "https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/bad-request.md",
  "title": "Bad Request",
  "status": 400,
  "detail": "The order id is required.",
  "messages": [
    { "message": "The order id is required.", "code": "order_id_required" }
  ]
}
```

- `type` links to a human-readable description of the error class (see [`docs/errors`](../../docs/errors)).
- `detail` is the joined handler messages, or a generic per-status default when none were logged.
- `messages[]` carries each logged message projected to `{ message, code }` only.
- `traceId` is reserved (omitted while null) for a future correlation-id change.

`Forbidden` (403) now returns this body too — populated from any logged `ForbiddenMessage`s — rather
than an empty response.

## Examples

### Successful response with data
```csharp
var order = new Order { Id = 1, Total = 100.00m };
var result = new Result<Order>(order, ResultStatus.Successful, []);
// 200 OK with the order object (application/json)
```

### Not Found response
```csharp
var messages = new MessageBase[] { new NotFoundMessage("Order not found", "order_not_found") };
var result = new Result<Order>(null, ResultStatus.NotFound, messages);
// 404 Not Found with a ProblemResponse (application/problem+json)
```

### Validation error response
```csharp
var messages = new MessageBase[] { new InvalidRequestMessage("Invalid order ID", "order_id_invalid") };
var result = new Result<Order>(null, ResultStatus.Invalid, messages);
// 400 Bad Request with a ProblemResponse (application/problem+json)
```

### Forbidden response
```csharp
var messages = new MessageBase[] { new ForbiddenMessage("Access denied", "access_denied") };
var result = new Result(ResultStatus.Forbidden, messages);
// 403 Forbidden with a ProblemResponse (application/problem+json)
```

## Related packages

- **`MiddleMan.Zero.AspNetCore.Http`** — the equivalent integration for Minimal APIs (`ToResult()`),
  producing the identical `application/problem+json` envelope.

## License

MIT
