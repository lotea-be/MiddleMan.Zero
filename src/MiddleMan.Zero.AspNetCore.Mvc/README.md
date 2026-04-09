# MiddleMan.Zero.AspNetCore.Mvc

ASP.NET Core MVC integration for MiddleMan.Zero, providing automatic conversion of `Result` and `Result<TResponse>` to `IActionResult`.

## Features

- **Automatic Result Conversion**: Controllers can return `Result` objects directly
- **ResultFilter**: Automatically converts Result objects to appropriate HTTP responses
- **Extension Methods**: Manual conversion methods for fine-grained control
- **HTTP Status Code Mapping**:
  - `Successful` → 200 OK
  - `NotFound` → 404 Not Found
  - `Invalid` → 400 Bad Request
  - `Forbidden` → 403 Forbidden
  - `Failure` → 500 Internal Server Error

## Installation

Add the package to your ASP.NET Core project:

```bash
dotnet add package MiddleMan.Zero.AspNetCore.Mvc
```

## Usage

### 1. Register the ResultFilter (Automatic Conversion)

In your `Program.cs` or `Startup.cs`:

```csharp
builder.Services
    .AddMvc()
    .AddMiddleManZeroResults();

// Or with AddMvcCore()
builder.Services
    .AddMvcCore()
    .AddMiddleManZeroResults();
```

### 2. Return Results Directly from Controllers

Once registered, you can return `Result` or `Result<TResponse>` directly from your controller actions:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IHandleAsync<GetOrderRequest, Result<Order>> _getOrderHandler;

    public OrdersController(IHandleAsync<GetOrderRequest, Result<Order>> getOrderHandler)
    {
        _getOrderHandler = getOrderHandler;
    }

    [HttpGet("{id}")]
    public async Task<Result<Order>> GetOrder(int id)
    {
        // The ResultFilter automatically converts this to IActionResult
        return await _getOrderHandler.HandleAsync(new GetOrderRequest { Id = id });
    }

    [HttpPost]
    public async Task<Result> CreateOrder(CreateOrderRequest request)
    {
        // Non-generic Result is also supported
        return await _createOrderHandler.HandleAsync(request);
    }
}
```

### 3. Manual Conversion (Alternative)

If you prefer manual control, you can use the extension methods without registering the filter:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetOrder(int id)
{
    var result = await _getOrderHandler.HandleAsync(new GetOrderRequest { Id = id });
    return result.ToActionResult();
}

[HttpGet("{id}/typed")]
public async Task<ActionResult<Order>> GetOrderTyped(int id)
{
    var result = await _getOrderHandler.HandleAsync(new GetOrderRequest { Id = id });
    return result.ToTypedActionResult();
}
```

## Extension Methods

### `ToActionResult()`

Converts a `Result` or `Result<TResponse>` to `IActionResult`:

```csharp
IActionResult actionResult = result.ToActionResult();
```

### `ToTypedActionResult<TResponse>()`

Converts a `Result<TResponse>` to `ActionResult<TResponse>` (provides better type safety):

```csharp
ActionResult<Order> actionResult = result.ToTypedActionResult();
```

## HTTP Status Code Mapping

| ResultStatus | HTTP Status Code | Response Body |
|---|---|---|
| `Successful` (no data) | 200 OK | Empty |
| `Successful` (with data) | 200 OK | Response object |
| `NotFound` | 404 Not Found | `{ messages: [...] }` |
| `Invalid` | 400 Bad Request | `{ messages: [...] }` |
| `Forbidden` | 403 Forbidden | Empty |
| `Failure` | 500 Internal Server Error | `{ messages: [...] }` |
| `Undefined` | 500 Internal Server Error | `{ messages: [...] }` |

## Examples

### Successful Response with Data
```csharp
var order = new Order { Id = 1, Total = 100.00m };
var result = new Result<Order>(order, ResultStatus.Successful, []);
// Returns: 200 OK with order object in body
```

### Not Found Response
```csharp
var messages = new[] { new NotFoundMessage { Message = "Order not found" } };
var result = new Result<Order>(null, ResultStatus.NotFound, messages);
// Returns: 404 Not Found with { messages: [...] }
```

### Validation Error Response
```csharp
var messages = new[] { new InvalidRequestMessage("Invalid order ID") };
var result = new Result<Order>(null, ResultStatus.Invalid, messages);
// Returns: 400 Bad Request with { messages: [...] }
```

### Forbidden Response
```csharp
var result = new Result(ResultStatus.Forbidden, []);
// Returns: 403 Forbidden
```

## License

MIT
