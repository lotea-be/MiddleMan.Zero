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

### Error response types

- **`ProblemResponse`**: The canonical RFC 9457 (RFC 7807-compatible) error body returned for every
  non-success result by the ASP.NET Core integration packages. Construct one from a result with
  `ProblemResponse.FromResult(result)`.
- **`ErrorMessage`**: A `{ message, code }` projection of a logged message, carried in
  `ProblemResponse.Messages`.

### Enums

- **`ResultStatus`**: Defines the outcome of an operation
  - `Undefined`: Status not set
  - `Successful`: Operation completed successfully
  - `Failure`: Operation failed
  - `Invalid`: Operation had invalid input
  - `NotFound`: Requested resource not found
  - `Forbidden`: Caller lacks the required permissions
  - `Conflict`: Request conflicts with the current state of the resource

## Usage

These abstractions are the contract consumers depend on directly: handlers implement
`IHandleAsync<>`, and ASP.NET Core endpoints work with `ResultBase` / `ResultBase<TResponse>` (and
receive the `ProblemResponse` envelope on the wire). You typically reference this package
transitively through `MiddleMan.Zero` and one of the ASP.NET Core integration packages rather than
adding it on its own.

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

## Target frameworks

- `net8.0`, `net9.0`, `net10.0`

## Related Packages

- **MiddleMan.Zero**: Core implementation
- **MiddleMan.Zero.DependencyInjection**: DI container registration
- **MiddleMan.Zero.AspNetCore.Http**: ASP.NET Core Minimal API integration (`ToResult()`)
- **MiddleMan.Zero.AspNetCore.Mvc**: ASP.NET Core MVC integration (`ToActionResult()` / `ToTypedActionResult()`)
