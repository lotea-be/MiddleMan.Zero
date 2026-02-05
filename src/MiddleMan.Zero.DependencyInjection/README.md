# MiddleMan.Zero.DependencyInjection

Dependency injection extensions for automatic registration of MiddleMan.Zero handlers.

## Overview

This package provides extension methods for `IServiceCollection` to automatically discover and register all MiddleMan.Zero handlers in your application.

## Features

- **Automatic Handler Discovery**: Scans all loaded assemblies for handler implementations
- **Flexible Lifetime Management**: Configure handler lifetimes (Transient, Scoped, Singleton)
- **Convention-Based Registration**: Automatically registers handlers by their implemented interfaces

## Usage

### Basic Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register all handlers with transient lifetime (default)
services.AddMiddleManZero();
```

### Custom Lifetime

```csharp
// Register handlers with scoped lifetime
services.AddMiddleManZero(ServiceLifetime.Scoped);

// Register handlers with singleton lifetime
services.AddMiddleManZero(ServiceLifetime.Singleton);
```

### ASP.NET Core Integration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add MiddleMan.Zero handlers
builder.Services.AddMiddleManZero();

var app = builder.Build();
```

### Using Handlers

```csharp
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
        return result.ToActionResult();
    }
}
```

## Installation

```bash
dotnet add package MiddleMan.Zero.DependencyInjection
```

## Dependencies

- MiddleMan.Zero.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

## How It Works

The `AddMiddleManZero` extension method:

1. Scans all assemblies in the current AppDomain
2. Identifies types implementing `IHandleAsync<>` or `IHandleAsync<,>`
3. Registers each handler with its interface in the service collection
4. Uses the specified service lifetime (default: Transient)

## Related Packages

- **MiddleMan.Zero**: Core implementation
- **MiddleMan.Zero.Abstractions**: Core interfaces
- **MiddleMan.Zero.AspNetCore.Mvc**: ASP.NET Core MVC integration
