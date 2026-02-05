# MiddleMan.Zero

Core implementation of the MiddleMan.Zero mediator pattern library for .NET.

## Overview

MiddleMan.Zero provides a lightweight, zero-ceremony implementation of the mediator pattern. It handles request/response workflows with built-in validation, error handling, and message logging.

## Key Components

### Base Classes

- **`HandlerBase<TRequest>`**: Abstract base class for handlers without a response
- **`HandlerBase<TRequest, TResponse>`**: Abstract base class for handlers with a typed response

### Context

- **`HandlerContext`**: Manages handler execution state and message logging
  - Tracks request validation status
  - Logs debug, failure, invalid request, and not found messages
  - Provides access to all logged messages

### Result Types

- **`Result`**: Concrete implementation of `ResultBase`
- **`Result<TResponse>`**: Concrete implementation of `ResultBase<TResponse>`

### Message Types

- **`DebugMessage`**: For diagnostic information
- **`FailureMessage`**: For operation failures
- **`InvalidRequestMessage`**: For validation errors
- **`NotFoundMessage`**: For resource not found scenarios

## Usage

### Creating a Handler

```csharp
public class GetOrderHandler : HandlerBase<GetOrderRequest, Order>
{
    protected override async ValueTask ValidateAsync(
        GetOrderRequest request, 
        HandlerContext context, 
        CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0)
        {
            context.AddInvalidRequestMessage("Invalid order ID");
        }
    }

    protected override async ValueTask<Order?> HandleAsync(
        GetOrderRequest request, 
        HandlerContext context, 
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetOrderAsync(request.OrderId);
        
        if (order == null)
        {
            context.NotFound("Order not found");
            return null;
        }
        
        return order;
    }
}
```

### Handler Execution Flow

1. Request validation via `ValidateAsync`
2. Fast-fail if validation errors exist
3. Request processing via `HandleAsync`
4. Automatic result creation based on context state

## Installation

```bash
dotnet add package MiddleMan.Zero
```

## Dependencies

- MiddleMan.Zero.Abstractions

## Related Packages

- **MiddleMan.Zero.DependencyInjection**: Automatic handler registration
- **MiddleMan.Zero.AspNetCore.Mvc**: ASP.NET Core MVC integration
