# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2026-02-18

### Changed

#### MiddleMan.Zero.AspNetCore.Http
- Replaced MiddleMan.Zero.Abstractions package by MiddleMan.Zero, so users have to install only one dependency

#### MiddleMan.Zero.AspNetCore.Mvc
- Replaced MiddleMan.Zero.Abstractions package by MiddleMan.Zero, so users have to install only one dependency

## [1.1.0] - 2026-02-15

### Added

#### MiddleMan.Zero.AspNetCore.Http (New Package)
- New package for ASP.NET Core Minimal API integration
- `ToResult()` extension method for `ResultBase` → `IResult` conversion
- `ToResult<TResponse>()` extension method for `ResultBase<TResponse>` → `IResult` conversion
- HTTP status code mapping matching the MVC package:
  - Successful → 200 OK
  - NotFound → 404 Not Found
  - Invalid → 400 Bad Request
  - Failure → 500 Internal Server Error
- README with usage examples and comparison with the MVC package

### Changed

#### MiddleMan.Zero.AspNetCore.Mvc
- Replaced `Microsoft.AspNetCore.Mvc.Core` and `Microsoft.Extensions.DependencyInjection.Abstractions` package references with `Microsoft.AspNetCore.App` framework reference
- Removed unnecessary project reference to `MiddleMan.Zero` (now only depends on `MiddleMan.Zero.Abstractions`)

### Package Links
- [MiddleMan.Zero](https://www.nuget.org/packages/MiddleMan.Zero/1.1.0)
- [MiddleMan.Zero.Abstractions](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/1.1.0)
- [MiddleMan.Zero.DependencyInjection](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/1.1.0)
- [MiddleMan.Zero.AspNetCore.Mvc](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/1.1.0)
- [MiddleMan.Zero.AspNetCore.Http](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Http/1.1.0)

## [1.0.0] - 2026-02-05

### 🎉 Initial Release

This is the first stable release of MiddleMan.Zero, a lightweight, zero-ceremony implementation of the mediator pattern for .NET applications.

### Added

#### MiddleMan.Zero.Abstractions
- Initial release of core abstractions
- `IHandleAsync<TRequest>` interface for handlers without response
- `IHandleAsync<TRequest, TResponse>` interface for handlers with typed response
- `MessageBase` abstract class for all messages
- `ResultBase` and `ResultBase<TResponse>` for operation results
- `ResultStatus` enum (Successful, Failure, Invalid, NotFound)

#### MiddleMan.Zero
- Initial release of core implementation
- `HandlerBase<TRequest>` abstract class for request handlers
- `HandlerBase<TRequest, TResponse>` abstract class for handlers with response
- `HandlerContext` for message logging and validation tracking
- `Result` and `Result<TResponse>` sealed implementations
- Built-in message types:
  - `InvalidRequestMessage` for validation errors
  - `FailureMessage` for operation failures
  - `NotFoundMessage` for missing resources
  - `DebugMessage` for debug logging
- Automatic null request validation
- Built-in validation pipeline with fail-fast behavior

#### MiddleMan.Zero.DependencyInjection
- Initial release of dependency injection extensions
- `AddMiddleManZero()` extension method for `IServiceCollection`
- Automatic handler discovery and registration
- Configurable service lifetime (Transient, Scoped, Singleton)

#### MiddleMan.Zero.AspNetCore.Mvc
- Initial release of ASP.NET Core MVC integration
- `ToActionResult()` extension methods for `ResultBase` and `ResultBase<TResponse>`
- Automatic HTTP status code mapping:
  - Successful → 200 OK
  - NotFound → 404 Not Found
  - Invalid → 400 Bad Request
  - Failure → 500 Internal Server Error
- `AddMiddleManZeroResults()` extension for automatic result conversion in controllers

### Technical Details
- Multi-target support: .NET 8.0, .NET 9.0, .NET 10.0
- Zero external dependencies (core packages)
- Async/await support with `Task`
- XML documentation for all public APIs
- Comprehensive test coverage
- Published to NuGet.org and GitHub Packages

### Package Links
- [MiddleMan.Zero](https://www.nuget.org/packages/MiddleMan.Zero/1.0.0)
- [MiddleMan.Zero.Abstractions](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/1.0.0)
- [MiddleMan.Zero.DependencyInjection](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/1.0.0)
- [MiddleMan.Zero.AspNetCore.Mvc](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/1.0.0)

[1.1.0]: https://github.com/lotea-be/MiddleMan.Zero/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/lotea-be/MiddleMan.Zero/releases/tag/v1.0.0
