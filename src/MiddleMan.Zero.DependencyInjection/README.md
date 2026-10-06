# MiddleMan.Zero.DependencyInjection

Dependency injection extensions for automatic registration of MiddleMan.Zero handlers.

## Overview

This package provides extension methods for `IServiceCollection` to automatically discover and register all MiddleMan.Zero handlers in your application.

## Features

- **Automatic Handler Discovery**: Scans all loaded assemblies for handler implementations
- **Flexible Lifetime Management**: Configure handler lifetimes (Transient, Scoped, Singleton)
- **Convention-Based Registration**: Automatically registers handlers by their implemented interfaces
- **Pipeline Behaviors**: Wraps handlers in any registered `IHandlerBehavior<>` / `IHandlerBehavior<,>`
- **Idempotent**: Calling `AddMiddleManZero()` more than once does not register handlers twice

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

### Explicit Assemblies

```csharp
// Only scan the assemblies that contain your handlers
services.AddMiddleManZero(typeof(GetOrderHandler).Assembly);
services.AddMiddleManZero([typeof(GetOrderHandler).Assembly], ServiceLifetime.Scoped);
```

### Pipeline Behaviors

```csharp
// Open generic: wraps every IHandleAsync<TRequest, TResponse>
services.AddTransient(typeof(IHandlerBehavior<,>), typeof(LoggingBehavior<,>));

// Closed: wraps only IHandleAsync<CancelOrderRequest>
services.AddScoped<IHandlerBehavior<CancelOrderRequest>, AuditBehavior>();

services.AddMiddleManZero();
```

Behaviors run in registration order (the first one registered is the outermost) and are resolved
each time the handler is resolved, using the handler's lifetime. When no behavior is registered for
a request type, the handler itself is returned.

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

1. Scans all assemblies already loaded in the current AppDomain (or the assemblies you pass in).
   Assemblies with unloadable types are skipped type-by-type rather than failing.
2. Identifies non-abstract, closed types implementing `IHandleAsync<>` or `IHandleAsync<,>`
   (open generic handlers are skipped).
3. Registers each handler as its concrete type, and each implemented interface through a factory
   that wraps the handler in the registered behaviors. Existing registrations for the same
   interface/handler pair are left untouched.
4. Uses the specified service lifetime (default: Transient)

## Related Packages

- **MiddleMan.Zero**: Core implementation
- **MiddleMan.Zero.Abstractions**: Core interfaces
- **MiddleMan.Zero.AspNetCore.Mvc**: ASP.NET Core MVC integration
