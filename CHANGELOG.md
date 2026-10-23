# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-10-23

The first stable 2.x release: a public-API audit and lock-down, one canonical HTTP error envelope,
two new statuses (`Conflict`, `Unauthorized`), and pipeline behaviors. It consolidates
`2.0.0-rc1` … `2.0.0-rc5`. **Upgrading from 1.2.0:** work through *Breaking changes* below; each
item has a migration note.

From this release on, the public API is tracked (`PublicAPI.Shipped.txt`) and Package Validation
compares every pack against 2.0.0, so breaking changes can only ship in a new major version.

### Breaking changes

#### HTTP error responses (`MiddleMan.Zero.AspNetCore.Http` + `MiddleMan.Zero.AspNetCore.Mvc`)

- **One canonical RFC 9457 error envelope.** Every non-success status now returns the same
  `application/problem+json` body (`type`, `title`, `status`, `detail`, `messages`, optional
  `traceId`) from `ToResult()`, `ToActionResult()` and `ToTypedActionResult()` alike. The old
  anonymous `{ messages: MessageBase[] }` body (which leaked `Id`, `CorrelationId` and `CreatedAt`)
  and the Http-only `ProblemDetails` 500 body are gone; each entry in `messages` now has only
  `message` and `code`.
  - **Migration:** clients that parsed the old body should read the RFC 9457 fields; the per-error
    list is still called `messages`, so `messages[].message` / `messages[].code` keep working.
- **403 Forbidden now has a body.** It previously returned an empty `Results.Forbid()` /
  `ForbidResult` (which also required an authentication scheme); it now returns the problem
  envelope populated from the logged `ForbiddenMessage`s.
  - **Migration:** clients that treated 403 as body-less should tolerate the problem body.
- **`type` URIs** have the form
  `https://github.com/lotea-be/MiddleMan.Zero/blob/main/docs/errors/<slug>.md` and are stable
  identifiers from now on.

#### `MiddleMan.Zero.Abstractions`

- **`MessageBase` properties are `init`-only.** `Id`, `CreatedAt`, `Message` and `Code` can no
  longer be reassigned after construction.
  - **Migration:** set them in an object initializer or use the new `(message)` /
    `(message, code)` constructors.
- **`MessageBase.CorrelationId` removed.** It defaulted to a new `Guid` per message, so it never
  correlated anything.
  - **Migration:** drop reads/initializers of `CorrelationId`. Correlation is planned through
    `ProblemResponse.TraceId` / `Activity` in a later, additive release.
- **`ResultBase.Messages` is `IReadOnlyList<MessageBase>`** (was `MessageBase[]`).
  - **Migration:** replace `.Length` with `.Count`; indexing and LINQ are unchanged. Use
    `.ToArray()` if you need an array.
- **`ResultBase<TResponse>.Response` is annotated `TResponse?`.** It was already null for every
  non-success status. The constructor still throws `ArgumentNullException` for a null `Successful`
  response.
  - **Migration:** check `ResultStatus == Successful` before reading `Response`, or use `!` once
    you have.

#### `MiddleMan.Zero`

- **`HandlerBase<TRequest, TResponse>` implements `IHandleAsync<TRequest, TResponse>`** (was
  `IHandleAsync<TRequest, TResponse?>`). Runtime types and DI resolution are unchanged; only the
  nullability annotation differs.
  - **Migration:** drop the `?` from any `IHandleAsync<TRequest, TResponse?>` dependencies.
- **Built-in message classes are `sealed`**: `DebugMessage`, `FailureMessage`,
  `InvalidRequestMessage`, `NotFoundMessage`, `ForbiddenMessage`, `UnauthorizedMessage`,
  `ConflictMessage`.
  - **Migration:** replace subclasses with the base message type plus a distinguishing `Code`.

#### `MiddleMan.Zero.DependencyInjection`

- **Registration shape changed.** Each handler is registered as its concrete type, and its
  `IHandleAsync<…>` interface through a factory (so behaviors can wrap it) instead of an
  `ImplementationType`. Resolution is unchanged. Open generic handler types are skipped (they
  previously produced unusable registrations), and an interface registration the consumer already
  made for the same handler is left alone.
  - **Migration:** only affects code that inspects `ServiceDescriptor`s for handlers.

### Added

- **`ResultStatus.Conflict` (409)** with `ConflictMessage` and `HandlerContext.IsConflict`.
- **`ResultStatus.Unauthorized` (401)** with `UnauthorizedMessage` and
  `HandlerContext.IsUnauthorized`. Status precedence is now
  `Unauthorized` > `Forbidden` > `Invalid` > `Conflict` > `Successful` > `NotFound` > `Failure`.
- **Pipeline behaviors:** `IHandlerBehavior<TRequest>` / `IHandlerBehavior<TRequest, TResponse>`
  with the `HandlerDelegate` / `HandlerDelegate<TResponse>` continuations. Register them as open or
  closed generics; `AddMiddleManZero()` applies them in registration order (first registered =
  outermost). With none registered, the handler itself is resolved.
- **`ProblemResponse` and `ErrorMessage`** (`MiddleMan.Zero.Abstractions`): the error envelope
  types, built with `ProblemResponse.FromResult(result)`. Each status links to a page under
  [`docs/errors`](docs/errors).
- Every message type has three constructors: parameterless, `(string message)` and
  `(string message, string code)`. `MessageBase.ToString()` returns `Message`.
- `AddMiddleManZero(params Assembly[])` and `AddMiddleManZero(IEnumerable<Assembly>, ServiceLifetime)`
  overloads for scoped scanning.
- `ToTypedActionResult<TResponse>` no longer requires `TResponse : class`, so value-typed responses
  (e.g. `Guid`) work.

### Changed

- `HandlerBase` calls `ThrowIfCancellationRequested()` before `ValidateAsync` and before
  `HandleAsync`, so an already-cancelled token surfaces as `OperationCanceledException`. Handlers
  remain responsible for passing the token to their own I/O.
- `HandlerBase` uses `ConfigureAwait(false)` internally.
- `AddMiddleManZero()` is idempotent (repeat calls add no duplicate registrations) and tolerates
  `ReflectionTypeLoadException` from third-party assemblies by scanning only their loadable types.
- Documented that a non-success result carries only the messages of the type that decided its
  status, so `DebugMessage` breadcrumbs never reach an HTTP error body. Successful results carry
  every message.

### Fixed

- Http `Failure` responses serialized message type names (`"MiddleMan.Zero.FailureMessage; …"`)
  instead of message text.
- Removed a duplicated `<summary>` on `HandlerBase<TRequest>.ValidateAsync` and a redundant null
  check in `HandlerBase<TRequest, TResponse>.CreateResult`.
- Samples: `IOrderRepository` moved into the `IceCreamTruck.Repositories` namespace; the in-memory
  repositories are thread-safe under concurrent integration tests.

### Dependencies

- `Microsoft.SourceLink.GitHub` 10.0.402 (10.0.401+ clears NU1902 / GHSA-23fw-v26w-5fgq on the
  transitive `Microsoft.Build.Tasks.Git`).
- `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.13.
- Tests/samples: `Microsoft.AspNetCore.Mvc.Testing` / `Microsoft.AspNetCore.OpenApi` 8.0.32 /
  9.0.21 / 10.0.13, `Microsoft.Extensions.DependencyInjection` / `Options` 10.0.13,
  `Microsoft.NET.Test.Sdk` 18.11.0, `Scalar.AspNetCore` 2.18.3. xunit stays on 3.x (4.x requires
  migrating off VSTest).

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
