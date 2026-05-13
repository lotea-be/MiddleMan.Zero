# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-05-08

This release tightens up the public API after a full audit. Most changes are non-breaking;
the breaking ones are listed first with migration steps.

### Breaking changes

#### MiddleMan.Zero.Abstractions
- **`MessageBase` properties are now `init`-only.** `Id`, `CorrelationId`, `CreatedAt`, `Message`,
  and `Code` can no longer be reassigned after construction.
  - **Migration:** if you mutated these post-construction (rare), set them via object initializer
    or one of the new `(string message)` / `(string message, string code)` constructors.
- **`ResultBase<TResponse>.Response` is now annotated as `TResponse?`.** It was already null in every
  non-`Successful` path; the type system now reflects that. The constructor still throws
  `ArgumentNullException` if `Response` is null when `ResultStatus` is `Successful`.
  - **Migration:** consumers reading `result.Response` from a result of unknown status will get
    nullable warnings under `Nullable=enable`. Either check `result.ResultStatus == Successful` first,
    or apply the null-forgiving operator (`result.Response!`) when you have already verified status.
- **`HandlerBase<TRequest, TResponse>` now implements `IHandleAsync<TRequest, TResponse>`** instead
  of `IHandleAsync<TRequest, TResponse?>`. The runtime type was identical for unconstrained generics,
  so DI registration and resolution are unchanged. Only nullability annotations differ.
  - **Migration:** if you previously declared dependencies as `IHandleAsync<TRequest, TResponse?>`,
    drop the `?` to match the cleaner annotation. The `?`-form continues to resolve at runtime.

### Added

#### MiddleMan.Zero.Abstractions
- Added `Conflict` status to `ResultStatus` enum for state-conflict errors (e.g., duplicate resource
  creation, optimistic-concurrency violations).

#### MiddleMan.Zero
- `DebugMessage`, `FailureMessage`, `ForbiddenMessage`, `NotFoundMessage`, and `InvalidRequestMessage`
  all now expose three constructors: parameterless, `(string message)`, and `(string message, string code)`.
  Previously only `InvalidRequestMessage` had constructors; everything else required object-initializer syntax.
- `MessageBase.ToString()` now returns the `Message` property text (was: type name).
- Added `ConflictMessage` class for representing state-conflict errors. Logging it via
  `HandlerContext.Log(ConflictMessage)` flips the new `IsConflict` flag and yields
  `ResultStatus.Conflict`. Precedence in `HandlerBase.CreateResult`:
  `Forbidden` > `Invalid` > `Conflict` > `Successful` > `NotFound` > `Failure`.

#### MiddleMan.Zero.AspNetCore.Mvc
- Added support for `Conflict` status mapping to HTTP 409 Conflict.

#### MiddleMan.Zero.AspNetCore.Http
- Added support for `Conflict` status mapping to HTTP 409 Conflict.

#### MiddleMan.Zero.DependencyInjection
- New `AddMiddleManZero(params Assembly[] assemblies)` overload for scoped scanning.
- New `AddMiddleManZero(IEnumerable<Assembly> assemblies, ServiceLifetime lifetime)` overload.
- `AddMiddleManZero` is now idempotent — calling it multiple times does not produce duplicate
  registrations of the same `(serviceType, implementationType)` pair.
- `AddMiddleManZero` is now resilient to `ReflectionTypeLoadException` from third-party assemblies
  with unresolvable references; only loadable types are scanned.

#### MiddleMan.Zero.AspNetCore.Mvc
- `ToTypedActionResult<TResponse>` no longer requires `where TResponse : class`. Value-typed
  responses (e.g., `Guid`) now work with the typed extension.

### Fixed

#### MiddleMan.Zero.AspNetCore.Http
- `Failure` (HTTP 500) responses now serialize the actual message text in the ProblemDetails
  `detail` field. Previously it joined `MessageBase.ToString()`, which produced type names
  (e.g., `"MiddleMan.Zero.FailureMessage; …"`).

#### MiddleMan.Zero
- Removed a duplicated `<summary>` block on `HandlerBase<TRequest>.ValidateAsync` XML doc.
- Removed a redundant `ArgumentNullException` check in `HandlerBase<TRequest, TResponse>.CreateResult`
  — the same check is enforced by `ResultBase<TResponse>`'s constructor.

#### Samples (IceCreamTruck)
- `IOrderRepository` now correctly lives in the `IceCreamTruck.Repositories` namespace
  (was: leaked into the global namespace).
- `OrderRepository` and `FlavorRepository` are now thread-safe under concurrent integration tests.

### Package Links
- [MiddleMan.Zero](https://www.nuget.org/packages/MiddleMan.Zero/2.0.0)
- [MiddleMan.Zero.Abstractions](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/2.0.0)
- [MiddleMan.Zero.DependencyInjection](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/2.0.0)
- [MiddleMan.Zero.AspNetCore.Mvc](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/2.0.0)
- [MiddleMan.Zero.AspNetCore.Http](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Http/2.0.0)

## [1.2.0] - 2026-04-18

### Added

#### MiddleMan.Zero.Abstractions
- Added `Forbidden` status to `ResultStatus` enum for permission-related errors

#### MiddleMan.Zero
- Added `ForbiddenMessage` class for representing forbidden operations
- Added `ToForbidden()` extension methods for creating forbidden results

#### MiddleMan.Zero.AspNetCore.Http
- Added support for `Forbidden` status mapping to HTTP 403 Forbidden

#### MiddleMan.Zero.AspNetCore.Mvc
- Added support for `Forbidden` status mapping to HTTP 403 Forbidden

### Changed

#### MiddleMan.Zero
- Updated dependencies

#### MiddleMan.Zero.Abstractions
- Updated dependencies

#### MiddleMan.Zero.DependencyInjection
- Updated dependencies

#### MiddleMan.Zero.AspNetCore.Mvc
- Updated dependencies

#### MiddleMan.Zero.AspNetCore.Http
- Updated dependencies

### Package Links
- [MiddleMan.Zero](https://www.nuget.org/packages/MiddleMan.Zero/1.2.0)
- [MiddleMan.Zero.Abstractions](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/1.2.0)
- [MiddleMan.Zero.DependencyInjection](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/1.2.0)
- [MiddleMan.Zero.AspNetCore.Mvc](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/1.2.0)
- [MiddleMan.Zero.AspNetCore.Http](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Http/1.2.0)

## [1.1.2] - 2026-03-01

### Changed

#### MiddleMan.Zero
- Updated dependencies

#### MiddleMan.Zero.Abstractions
- Updated dependencies

#### MiddleMan.Zero.DependencyInjection
- Updated dependencies

#### MiddleMan.Zero.AspNetCore.Mvc
- Updated dependencies

#### MiddleMan.Zero.AspNetCore.Http
- Updated dependencies

### Package Links
- [MiddleMan.Zero](https://www.nuget.org/packages/MiddleMan.Zero/1.1.2)
- [MiddleMan.Zero.Abstractions](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/1.1.2)
- [MiddleMan.Zero.DependencyInjection](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/1.1.2)
- [MiddleMan.Zero.AspNetCore.Mvc](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/1.1.2)
- [MiddleMan.Zero.AspNetCore.Http](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Http/1.1.2)

## [1.1.1] - 2026-02-18

### Changed

#### MiddleMan.Zero.AspNetCore.Http
- Replaced MiddleMan.Zero.Abstractions package by MiddleMan.Zero, so users have to install only one dependency

#### MiddleMan.Zero.AspNetCore.Mvc
- Replaced MiddleMan.Zero.Abstractions package by MiddleMan.Zero, so users have to install only one dependency

### Package Links
- [MiddleMan.Zero](https://www.nuget.org/packages/MiddleMan.Zero/1.1.1)
- [MiddleMan.Zero.Abstractions](https://www.nuget.org/packages/MiddleMan.Zero.Abstractions/1.1.1)
- [MiddleMan.Zero.DependencyInjection](https://www.nuget.org/packages/MiddleMan.Zero.DependencyInjection/1.1.1)
- [MiddleMan.Zero.AspNetCore.Mvc](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Mvc/1.1.1)
- [MiddleMan.Zero.AspNetCore.Http](https://www.nuget.org/packages/MiddleMan.Zero.AspNetCore.Http/1.1.1)

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

[2.0.0]: https://github.com/lotea-be/MiddleMan.Zero/compare/v1.2.0...v2.0.0
[1.2.0]: https://github.com/lotea-be/MiddleMan.Zero/compare/v1.1.2...v1.2.0
[1.1.2]: https://github.com/lotea-be/MiddleMan.Zero/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/lotea-be/MiddleMan.Zero/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/lotea-be/MiddleMan.Zero/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/lotea-be/MiddleMan.Zero/releases/tag/v1.0.0
