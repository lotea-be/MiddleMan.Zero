# MiddleMan.Zero

[![CI](https://github.com/lotea-be/MiddleMan.Zero/actions/workflows/ci.yml/badge.svg)](https://github.com/lotea-be/MiddleMan.Zero/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.svg)](https://www.nuget.org/packages/MiddleMan.Zero/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.svg)](https://www.nuget.org/packages/MiddleMan.Zero/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)

A lightweight, zero-ceremony implementation of the mediator pattern for .NET applications.

## Overview

MiddleMan.Zero provides a simple yet powerful framework for implementing request/response workflows with built-in validation, error handling, and message logging. It eliminates boilerplate code while maintaining full control over your application's business logic.

## Features

- **Zero Ceremony**: Minimal configuration required to get started
- **Built-in Validation**: Separate validation logic from business logic
- **Structured Error Handling**: Type-safe result patterns with status codes
- **Message Logging**: Track debug messages, failures, and validation errors
- **Dependency Injection**: Automatic handler discovery and registration
- **ASP.NET Core Integration**: Seamless integration with MVC Controllers and Minimal APIs

## Packages

### Core Libraries

| Package | Version | Downloads | Documentation |
|---------|---------|-----------|---------------|
| **MiddleMan.Zero.Abstractions** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.Abstractions.svg)](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.Abstractions.svg)](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/) | [README](src/MiddleMan.Zero.Abstractions/README.md) |
| **MiddleMan.Zero** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.svg)](https://www.nuget.org/packages/MiddleMan.Zero/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.svg)](https://www.nuget.org/packages/MiddleMan.Zero/) | [README](src/MiddleMan.Zero/README.md) |
| **MiddleMan.Zero.DependencyInjection** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.DependencyInjection.svg)](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.DependencyInjection.svg)](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/) | [README](src/MiddleMan.Zero.DependencyInjection/README.md) |
| **MiddleMan.Zero.AspNetCore.Mvc** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.AspNetCore.Mvc.svg)](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.AspNetCore.Mvc.svg)](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/) | [README](src/MiddleMan.Zero.AspNetCore.Mvc/README.md) |
| **MiddleMan.Zero.AspNetCore.Http** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.AspNetCore.Http.svg)](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Http/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.AspNetCore.Http.svg)](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Http/) | [README](src/MiddleMan.Zero.AspNetCore.Http/README.md) |

## Quick Start

### Installation

```bash
# In your class libraries or Console Apps
dotnet add package MiddleMan.Zero

# Dependency injection helpers
dotnet add package MiddleMan.Zero.DependencyInjection

# ASP.NET Core integration (choose based on your API style)
dotnet add package MiddleMan.Zero.AspNetCore.Mvc  # For MVC Controllers
dotnet add package MiddleMan.Zero.AspNetCore.Http # For Minimal APIs
```

### 1. Define Your Request and Response

```csharp
namespace MyApp.Contracts;

public sealed class GetOrderRequest
{
    public required Guid OrderId { get; init; }
}

public class Order
{
    public Guid Id { get; init; }
    public required string CustomerName { get; init; }
    public decimal TotalPrice { get; init; }
    public DateTime OrderedAt { get; init; }
}
```

### 2. Create a Handler

```csharp
using MiddleMan.Zero;

namespace MyApp.Handlers;

public class GetOrderHandler(IOrderRepository repository) 
    : HandlerBase<GetOrderRequest, Order>
{
    protected override Task ValidateAsync(
        GetOrderRequest request, 
        HandlerContext context, 
        CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty)
        {
            context.Log(new InvalidRequestMessage("Order ID must be a valid GUID."));
        }

        return Task.CompletedTask;
    }

    protected override async Task<Order?> HandleAsync(
        GetOrderRequest request, 
        HandlerContext context, 
        CancellationToken cancellationToken = default)
    {
        var order = await repository.GetAsync(request.OrderId);
        
        if (order == null)
        {
            context.Log(new NotFoundMessage());
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

// Automatically discovers and registers all handlers in the calling assembly
builder.Services.AddMiddleManZero();
```

### 4. Use in Your API

#### MVC Controllers

```csharp
using Microsoft.AspNetCore.Mvc;
using MiddleMan.Zero.Abstractions;
using MiddleMan.Zero.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        Guid id, 
        [FromServices] IHandleAsync<GetOrderRequest, Order> handler)
    {
        logger.LogInformation("Retrieving order: {OrderId}", id);

        var request = new GetOrderRequest { OrderId = id };
        var result = await handler.HandleAsync(request);
        
        // Automatically converts Result to appropriate HTTP response
        return result.ToActionResult();
    }
}
```

#### Minimal APIs

```csharp
using MiddleMan.Zero.Abstractions;
using MiddleMan.Zero.AspNetCore.Http;
using MyApp.Contracts;

var app = builder.Build();

app.MapGet("/api/orders/{id}", async (
    Guid id, 
    IHandleAsync<GetOrderRequest, Order> handler) =>
{
    var request = new GetOrderRequest { OrderId = id };
    var result = await handler.HandleAsync(request);
    
    // Automatically converts Result to appropriate IResult
    return result.ToResult();
});

app.Run();
```

## Samples

Explore complete working examples:

- **[IceCreamTruck](samples/IceCreamTruck/README.md)**: Core library implementation with order handling
- **[IceCreamTruck.WebApi](samples/IceCreamTruck.WebApi/README.md)**: ASP.NET Core Web API integration

## Key Concepts

### Handler Execution Flow

1. **Null Check**: Request is validated for null
2. **Validation**: Request is validated via `ValidateAsync()`
3. **Fast-Fail**: Execution stops if validation fails
4. **Processing**: Business logic executes in `HandleAsync()`
5. **Result Creation**: Automatic result generation based on context state

### Result Status Mapping

| ResultStatus | HTTP Status Code | Use Case |
|--------------|------------------|----------|
| `Successful` | 200 OK | Operation completed successfully |
| `NotFound` | 404 Not Found | Resource doesn't exist |
| `Invalid` | 400 Bad Request | Validation failed |
| `Forbidden` | 403 Forbidden | Caller lacks required permissions |
| `Failure` | 500 Internal Server Error | Operation failed |

### Handler Context

The `HandlerContext` provides methods for logging messages during handler execution:

```csharp
// Log validation errors (marks request as invalid)
context.Log(new InvalidRequestMessage("Order ID must be valid."));

// Mark resource as not found
context.Log(new NotFoundMessage());
context.Log(new NotFoundMessage("Order not found."));

// Deny access when the caller lacks permissions
context.Log(new ForbiddenMessage());

// Log operation failures
context.Log(new FailureMessage("Failed to process order."));

// Log diagnostic information
context.Log(new DebugMessage("Processing order..."));
```

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

## Why MiddleMan.Zero?

- **🚀 Zero Boilerplate**: Get started with minimal configuration
- **✅ Built-in Validation**: Clean separation of validation and business logic
- **🎯 Type-Safe**: Fully typed request/response patterns
- **🔌 Easy Integration**: Works seamlessly with ASP.NET Core
- **📦 Modular**: Use only what you need
- **🧪 Well-Tested**: Comprehensive test coverage
- **📚 Well-Documented**: Clear documentation and examples

## Use Cases

- **Clean Architecture**: Implement CQRS patterns without complexity
- **API Development**: Build RESTful APIs with consistent response handling
- **Business Logic**: Separate validation, processing, and error handling
- **Microservices**: Keep request/response workflows organized and maintainable

## Community & Support

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/lotea-be/MiddleMan.Zero/issues)
- **Releases**: Check out [Release Notes](https://github.com/lotea-be/MiddleMan.Zero/releases) for version updates

## Contributing

Contributions are welcome! Please ensure all tests pass before submitting a pull request.

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to your fork
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025-2026 Lotea SRL

## Project Structure

```
MiddleMan.Zero/
├── src/
│   ├── MiddleMan.Zero.Abstractions/      # Core interfaces and base types
│   ├── MiddleMan.Zero/                   # Core implementation with HandlerBase
│   ├── MiddleMan.Zero.DependencyInjection/  # DI extensions for handler registration
│   ├── MiddleMan.Zero.AspNetCore.Mvc/    # MVC Controller integration
│   └── MiddleMan.Zero.AspNetCore.Http/   # Minimal API integration
├── samples/
│   ├── IceCreamTruck/                    # Sample library with handlers
│   └── IceCreamTruck.WebApi/             # Sample API (MVC + Minimal APIs)
└── tests/
    ├── MiddleMan.Zero.Tests/
    ├── MiddleMan.Zero.DependencyInjection.Tests/
    ├── MiddleMan.Zero.AspNetCore.Mvc.Tests/
    ├── MiddleMan.Zero.AspNetCore.Http.Tests/
    ├── IceCreamTruck.Tests/
    └── IceCreamTruck.WebApi.Tests/
```
