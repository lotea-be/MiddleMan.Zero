# MiddleMan.Zero

A lightweight, zero-ceremony implementation of the mediator pattern for .NET applications.

## Overview

MiddleMan.Zero provides a simple yet powerful framework for implementing request/response workflows with built-in validation, error handling, and message logging. It eliminates boilerplate code while maintaining full control over your application's business logic.

## Features

- **Zero Ceremony**: Minimal configuration required to get started
- **Built-in Validation**: Separate validation logic from business logic
- **Structured Error Handling**: Type-safe result patterns with status codes
- **Message Logging**: Track debug messages, failures, and validation errors
- **Dependency Injection**: Automatic handler discovery and registration
- **ASP.NET Core Integration**: Seamless integration with MVC controllers

## Packages

### Core Libraries

| Package | Description | Documentation |
|---------|-------------|---------------|
| **MiddleMan.Zero.Abstractions** | Core interfaces and base types | [README](src/MiddleMan.Zero.Abstractions/README.md) |
| **MiddleMan.Zero** | Core implementation with handler base classes | [README](src/MiddleMan.Zero/README.md) |
| **MiddleMan.Zero.DependencyInjection** | Automatic handler registration for DI containers | [README](src/MiddleMan.Zero.DependencyInjection/README.md) |
| **MiddleMan.Zero.AspNetCore.Mvc** | ASP.NET Core MVC integration | [README](src/MiddleMan.Zero.AspNetCore.Mvc/README.md) |

## Quick Start

### Installation

```bash
# Install core packages
dotnet add package MiddleMan.Zero
dotnet add package MiddleMan.Zero.DependencyInjection

# For ASP.NET Core projects
dotnet add package MiddleMan.Zero.AspNetCore.Mvc
```

### 1. Define Your Request and Response

```csharp
public class GetOrderRequest
{
    public int OrderId { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public decimal Total { get; set; }
}
```

### 2. Create a Handler

```csharp
using MiddleMan.Zero;

public class GetOrderHandler : HandlerBase<GetOrderRequest, Order>
{
    private readonly IOrderRepository _repository;

    public GetOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    protected override async ValueTask ValidateAsync(
        GetOrderRequest request, 
        HandlerContext context, 
        CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0)
        {
            context.AddInvalidRequestMessage("Order ID must be greater than 0");
        }
    }

    protected override async ValueTask<Order?> HandleAsync(
        GetOrderRequest request, 
        HandlerContext context, 
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetOrderAsync(request.OrderId, cancellationToken);
        
        if (order == null)
        {
            context.NotFound($"Order {request.OrderId} not found");
            return null;
        }
        
        return order;
    }
}
```

### 3. Register Handlers

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Automatically discovers and registers all handlers
builder.Services.AddMiddleManZero();
```

### 4. Use in Controllers

```csharp
using Microsoft.AspNetCore.Mvc;
using MiddleMan.Zero.Abstractions;
using MiddleMan.Zero.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IHandleAsync<GetOrderRequest, Order> _handler;

    public OrdersController(IHandleAsync<GetOrderRequest, Order> handler)
    {
        _handler = handler;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var request = new GetOrderRequest { OrderId = id };
        var result = await _handler.HandleAsync(request);
        
        // Automatically converts Result to appropriate HTTP response
        return result.ToActionResult();
    }
}
```

## Samples

Explore complete working examples:

- **[IceCreamTruck](samples/IceCreamTruck/README.md)**: Core library implementation with order handling
- **[IceCreamTruck.WebApi](samples/IceCreamTruck.WebApi/README.md)**: ASP.NET Core Web API integration

## Key Concepts

### Handler Execution Flow

1. **Validation**: Request is validated via `ValidateAsync`
2. **Fast-Fail**: Execution stops if validation fails
3. **Processing**: Business logic executes in `HandleAsync`
4. **Result Creation**: Automatic result generation based on context state

### Result Status Mapping

| ResultStatus | HTTP Status Code | Use Case |
|--------------|------------------|----------|
| `Successful` | 200 OK | Operation completed successfully |
| `NotFound` | 404 Not Found | Resource doesn't exist |
| `Invalid` | 400 Bad Request | Validation failed |
| `Failure` | 500 Internal Server Error | Operation failed |

### Handler Context

The `HandlerContext` provides methods for logging messages during handler execution:

- `AddInvalidRequestMessage()`: Log validation errors
- `NotFound()`: Mark resource as not found
- `Failure()`: Log operation failures
- `Debug()`: Log diagnostic information

## Building from Source

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Create NuGet packages
dotnet pack
```

## Contributing

Contributions are welcome! Please ensure all tests pass before submitting a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025-2026 Lotea SRL
## Project Structure

```
MiddleMan.Zero/
├── src/
│   ├── MiddleMan.Zero.Abstractions/      # Core interfaces
│   ├── MiddleMan.Zero/                   # Core implementation
│   ├── MiddleMan.Zero.DependencyInjection/  # DI extensions
│   └── MiddleMan.Zero.AspNetCore.Mvc/    # ASP.NET Core integration
├── samples/
│   ├── IceCreamTruck/                    # Sample library
│   └── IceCreamTruck.WebApi/             # Sample API
└── tests/
    ├── MiddleMan.Zero.Tests/
    ├── MiddleMan.Zero.DependencyInjection.Tests/
    ├── MiddleMan.Zero.AspNetCore.Mvc.Tests/
    ├── IceCreamTruck.Tests/
    └── IceCreamTruck.WebApi.Tests/
```
