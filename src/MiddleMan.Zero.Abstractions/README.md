# MiddleMan.Zero.Abstractions

Core abstractions and interfaces for the MiddleMan.Zero mediator pattern library.

## Overview

This package contains the fundamental building blocks for implementing the mediator pattern in .NET applications. It defines the core interfaces, base classes, and types used across the MiddleMan.Zero ecosystem.

## Key Components

### Interfaces

- **`IHandleAsync<TRequest>`**: Interface for handlers that process requests without returning a response
- **`IHandleAsync<TRequest, TResponse>`**: Interface for handlers that process requests and return a typed response

### Base Classes

- **`MessageBase`**: Base class for all message types with common properties:
  - `Id`: Unique identifier for the message
  - `CorrelationId`: Identifier for tracking related messages
  - `CreatedAt`: UTC timestamp of message creation
  - `Message`: Human-readable description
  - `Code`: Categorization or identification code

- **`ResultBase`**: Represents operation results without a specific response
- **`ResultBase<TResponse>`**: Represents operation results with a typed response

### Enums

- **`ResultStatus`**: Defines the outcome of an operation
  - `Undefined`: Status not set
  - `Successful`: Operation completed successfully
  - `Failure`: Operation failed
  - `Invalid`: Operation had invalid input
  - `NotFound`: Requested resource not found
  - `Forbidden`: Caller lacks the required permissions

## Usage

This package is typically not used directly. Instead, reference it when building custom handlers or extending the MiddleMan.Zero framework.

```csharp
using MiddleMan.Zero.Abstractions;

public class MyHandler : IHandleAsync<MyRequest, MyResponse>
{
    public async Task<ResultBase<MyResponse>> HandleAsync(
        MyRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

## Installation

```bash
dotnet add package MiddleMan.Zero.Abstractions
```

## Dependencies

- .NET Standard 2.1 or higher

## Related Packages

- **MiddleMan.Zero**: Core implementation
- **MiddleMan.Zero.DependencyInjection**: DI container registration
- **MiddleMan.Zero.AspNetCore.Mvc**: ASP.NET Core MVC integration
