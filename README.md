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
- **ASP.NET Core Integration**: Seamless integration with MVC controllers

## Packages

### Core Libraries

| Package | Version | Downloads | Documentation |
|---------|---------|-----------|---------------|
| **MiddleMan.Zero.Abstractions** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.Abstractions.svg)](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.Abstractions.svg)](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/) | [README](src/MiddleMan.Zero.Abstractions/README.md) |
| **MiddleMan.Zero** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.svg)](https://www.nuget.org/packages/MiddleMan.Zero/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.svg)](https://www.nuget.org/packages/MiddleMan.Zero/) | [README](src/MiddleMan.Zero/README.md) |
| **MiddleMan.Zero.DependencyInjection** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.DependencyInjection.svg)](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.DependencyInjection.svg)](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/) | [README](src/MiddleMan.Zero.DependencyInjection/README.md) |
| **MiddleMan.Zero.AspNetCore.Mvc** | [![NuGet](https://img.shields.io/nuget/v/MiddleMan.Zero.AspNetCore.Mvc.svg)](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/) | [![Downloads](https://img.shields.io/nuget/dt/MiddleMan.Zero.AspNetCore.Mvc.svg)](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/) | [README](src/MiddleMan.Zero.AspNetCore.Mvc/README.md) |

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

    protected override async Task ValidateAsync(
        GetOrderRequest request, 
        HandlerContext context, 
        CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0)
        {
            context.AddInvalidRequestMessage("Order ID must be greater than 0");
        }
    }

    protected override async Task<Order?> HandleAsync(
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
- **Discussions**: Ask questions on [GitHub Discussions](https://github.com/lotea-be/MiddleMan.Zero/discussions)
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
