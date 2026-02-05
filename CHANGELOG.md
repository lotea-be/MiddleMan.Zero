# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-02-05

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

[0.1.0]: https://github.com/lotea-be/MiddleMan.Zero/releases/tag/v0.1.0
