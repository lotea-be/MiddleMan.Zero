# Spec — http-error-response

> New capability introduced by the `unify-error-envelope` change. Defines the
> unified RFC 7807/9457-shaped error-response envelope (`ProblemResponse`) and its
> per-item projection (`ErrorMessage`), the single shared factory that constructs it
> from a `ResultBase`, and the requirement that both ASP.NET Core mapper packages
> produce identical `application/problem+json` bodies for every non-2xx status.

## ADDED Requirements

### Requirement: ProblemResponse type
The system MUST expose a sealed POCO `MiddleMan.Zero.Abstractions.ProblemResponse` with
the following non-null members: `Type` (string), `Title` (string), `Status` (int), `Detail`
(string), `Messages` (`IReadOnlyList<ErrorMessage>`); and one nullable member: `TraceId`
(string?). All members MUST use `[JsonPropertyName]` with their camelCase JSON name.
`TraceId` MUST carry `[JsonIgnore(WhenWritingNull)]`; no other member carries that
attribute. `ProblemResponse` MUST NOT inherit `Microsoft.AspNetCore.Mvc.ProblemDetails`
or any ASP.NET type; the type lives entirely in `MiddleMan.Zero.Abstractions`, which has
no ASP.NET dependency. Every public member MUST have an XML doc comment.

#### Scenario: type serializes required fields always
- **WHEN** a `ProblemResponse` is serialized to JSON
- **THEN** the output contains `"type"`, `"title"`, `"status"`, `"detail"`, and `"messages"` keys
- **AND** the output does NOT contain `"traceId"` when `TraceId` is null

#### Scenario: type serializes traceId only when non-null
- **WHEN** a `ProblemResponse` has a non-null `TraceId`
- **THEN** the serialized JSON contains `"traceId"` with the expected value

### Requirement: ErrorMessage type
The system MUST expose a sealed POCO `MiddleMan.Zero.Abstractions.ErrorMessage` with
two non-null string members: `Message` and `Code`. The type MUST NOT expose
`Id`, `CorrelationId`, or `CreatedAt` (the fields present on `MessageBase`). Both
members MUST have XML doc comments.

#### Scenario: ErrorMessage projects only message and code
- **WHEN** an `ErrorMessage` is constructed from a `MessageBase`-derived logged message
- **THEN** the resulting instance contains the source's `Message` and `Code` values
- **AND** no `Id`, `CorrelationId`, or `CreatedAt` field is present

### Requirement: FromResult factory
The system MUST provide a public static method `ProblemResponse.FromResult(ResultBase result)`
that is the only place in the codebase where `ResultStatus` is switched over for the
purpose of constructing an error body. The factory MUST produce the following mapping:

| `ResultStatus`       | HTTP `Status` | `Title`                | `type` slug               | Default `Detail` when `Messages` is empty               |
|----------------------|---------------|------------------------|---------------------------|---------------------------------------------------------|
| `Invalid`            | 400           | `"Bad Request"`        | `bad-request.md`          | `"The request is invalid."`                             |
| `Forbidden`          | 403           | `"Forbidden"`          | `forbidden.md`            | `"Access denied."`                                      |
| `NotFound`           | 404           | `"Not Found"`          | `not-found.md`            | `"The requested resource was not found."`               |
| `Conflict`           | 409           | `"Conflict"`           | `conflict.md`             | `"The request conflicts with the current state."`       |
| `Failure`            | 500           | `"Internal Server Error"` | `internal-server-error.md` | `"An unexpected error occurred."`                  |
| `Undefined` (default arm) | 500    | `"Internal Server Error"` | `internal-server-error.md` | `"An unexpected error occurred."`                  |

The `type` URI MUST be the concatenation of
`https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/` and the slug.
When the result's `Messages` list is non-empty, `Detail` MUST be the `Message` strings
joined by `"; "`. When `Messages` is empty, `Detail` MUST be the per-status default string.
The factory MUST throw `InvalidOperationException` when called with a `ResultBase` whose
`Status` is `Successful`. The factory MUST be declared in `Abstractions` (no ASP.NET
dependency) and be visible to both mapper packages without `InternalsVisibleTo`.

#### Scenario: factory produces correct shape for Invalid
- **WHEN** `ProblemResponse.FromResult` is called with a result whose `Status` is `Invalid`
- **THEN** the returned `ProblemResponse` has `Status = 400`, `Title = "Bad Request"`, `Type` ending in `bad-request.md`
- **AND** `Messages` contains one `ErrorMessage` per logged message with matching `Message` and `Code`

#### Scenario: factory produces correct shape for Forbidden
- **WHEN** `ProblemResponse.FromResult` is called with a result whose `Status` is `Forbidden`
- **THEN** the returned `ProblemResponse` has `Status = 403`, `Title = "Forbidden"`, `Type` ending in `forbidden.md`

#### Scenario: factory produces correct shape for NotFound
- **WHEN** `ProblemResponse.FromResult` is called with a result whose `Status` is `NotFound`
- **THEN** the returned `ProblemResponse` has `Status = 404`, `Title = "Not Found"`, `Type` ending in `not-found.md`

#### Scenario: factory produces correct shape for Conflict
- **WHEN** `ProblemResponse.FromResult` is called with a result whose `Status` is `Conflict`
- **THEN** the returned `ProblemResponse` has `Status = 409`, `Title = "Conflict"`, `Type` ending in `conflict.md`

#### Scenario: factory produces correct shape for Failure
- **WHEN** `ProblemResponse.FromResult` is called with a result whose `Status` is `Failure`
- **THEN** the returned `ProblemResponse` has `Status = 500`, `Title = "Internal Server Error"`, `Type` ending in `internal-server-error.md`

#### Scenario: factory uses default detail when messages list is empty
- **WHEN** `ProblemResponse.FromResult` is called with a result that has no logged messages
- **THEN** the returned `ProblemResponse` has `Detail` equal to the per-status default string

#### Scenario: factory joins messages into detail
- **WHEN** `ProblemResponse.FromResult` is called with a result that has two or more logged messages
- **THEN** `Detail` is the joined `Message` strings separated by `"; "`

#### Scenario: factory throws on Successful
- **WHEN** `ProblemResponse.FromResult` is called with a result whose `Status` is `Successful`
- **THEN** the factory throws `InvalidOperationException`

#### Scenario: factory covers every ResultStatus value
- **WHEN** the factory is invoked for every value returned by `Enum.GetValues<ResultStatus>()`
- **THEN** it produces a valid `ProblemResponse` for all non-Successful values and throws only for Successful
- **AND** a test enumerating all enum values passes without uncovered cases

### Requirement: Http mapper emits application/problem+json
The system MUST ensure that `MiddleMan.Zero.AspNetCore.Http`'s `ToResult` and `ToResult<T>`
extension methods produce `application/problem+json` responses for every non-success
`ResultStatus` by calling `ProblemResponse.FromResult` and passing the result to
`Results.Json` with `contentType: "application/problem+json"` and `statusCode: dto.Status`.
The Http mapper MUST NOT contain its own per-status body switch. The Http mapper MUST NOT
call `Results.Forbid()` for a `Forbidden` result.

#### Scenario: Http ToResult produces problem+json for Invalid
- **WHEN** `ToResult` is called on an Invalid result
- **THEN** the HTTP response has status 400, content type `application/problem+json`, and a body matching the `ProblemResponse` shape

#### Scenario: Http ToResult produces problem+json for Forbidden
- **WHEN** `ToResult` is called on a Forbidden result
- **THEN** the HTTP response has status 403, content type `application/problem+json`, and a body containing the `messages` array

#### Scenario: Http ToResult success path is unchanged
- **WHEN** `ToResult` is called on a Successful result
- **THEN** the HTTP response has status 200 and does NOT use `application/problem+json`

### Requirement: Mvc mapper emits application/problem+json
The system MUST ensure that `MiddleMan.Zero.AspNetCore.Mvc`'s `ToActionResult` and
`ToTypedActionResult` extension methods produce `application/problem+json` responses for
every non-success `ResultStatus` by calling `ProblemResponse.FromResult` and returning
`new ObjectResult(dto) { StatusCode = dto.Status, ContentTypes = { "application/problem+json" } }`.
The Mvc mapper MUST NOT contain its own per-status body switch. The Mvc mapper MUST NOT
return a `ForbidResult` for a `Forbidden` result.

#### Scenario: Mvc ToActionResult produces problem+json for Invalid
- **WHEN** `ToActionResult` is called on an Invalid result
- **THEN** the HTTP response has status 400, content type `application/problem+json`, and a body matching the `ProblemResponse` shape

#### Scenario: Mvc ToActionResult produces problem+json for Forbidden
- **WHEN** `ToActionResult` is called on a Forbidden result
- **THEN** the HTTP response has status 403, content type `application/problem+json`, and a body containing the `messages` array (populated from logged `ForbiddenMessage`s)

#### Scenario: Mvc ToTypedActionResult success path is unchanged
- **WHEN** `ToTypedActionResult` is called on a Successful result
- **THEN** the HTTP response has status 200 and the response body is the raw `TResponse` value

### Requirement: Lockstep body identity
The system MUST guarantee that for every non-2xx `ResultStatus` the JSON bytes and the
`Content-Type` header produced by `ToResult` (Http) and `ToActionResult` (Mvc) are
byte-for-byte identical when given the same `ResultBase` input. A shared cross-package
test MUST assert this for all error-path statuses (`Invalid`, `Forbidden`, `NotFound`,
`Conflict`, `Failure`).

#### Scenario: Http and Mvc produce identical bytes for Invalid
- **WHEN** both mappers are called with the same Invalid result
- **THEN** the serialized JSON body bytes are identical and both responses report `Content-Type: application/problem+json`

#### Scenario: Http and Mvc produce identical bytes for Forbidden
- **WHEN** both mappers are called with the same Forbidden result carrying logged ForbiddenMessages
- **THEN** the serialized JSON body bytes are identical

### Requirement: Error type documentation files
The system MUST include five Markdown files under `docs/errors/` — `bad-request.md`,
`forbidden.md`, `not-found.md`, `conflict.md`, `internal-server-error.md` — reachable at
the raw-GitHub URLs used as `type` values in `ProblemResponse`. Each file MUST describe
the corresponding error status, its meaning in the MiddleMan.Zero context, and remediation
guidance for consumers.

#### Scenario: type URI resolves to a Markdown file
- **WHEN** the `type` field of a `ProblemResponse` is fetched as a URL
- **THEN** it returns the corresponding `docs/errors/<slug>.md` file from the repository

### Requirement: PublicAPI tracking for new types
The system MUST declare `ProblemResponse`, `ErrorMessage`, and `ProblemResponse.FromResult`
in `src/MiddleMan.Zero.Abstractions/PublicAPI.Unshipped.txt` so that the RS0016 build
error is resolved and the surface change is tracked as an explicit diff.

#### Scenario: build succeeds with new types declared
- **WHEN** `dotnet build` is run after adding `ProblemResponse` and `ErrorMessage`
- **THEN** the build succeeds with no RS0016 errors on any of the three target frameworks

### Requirement: Version bump and changelog
The system MUST update `<Version>` in `src/Directory.Build.props` to `2.0.0-rc3` and
include a breaking-change entry in `CHANGELOG.md` describing the error-body shape change,
the addition of `type`/`title`/`status`/`detail` fields, the removal of
`Id`/`CorrelationId`/`CreatedAt` from message items, and the 403 body change.

#### Scenario: version and changelog updated atomically
- **WHEN** the change is committed
- **THEN** `src/Directory.Build.props` contains `<Version>2.0.0-rc3</Version>`
- **AND** `CHANGELOG.md` contains a breaking-change section for `2.0.0-rc3`

### Requirement: README corrections
The system MUST remove the phantom `ResultFilter`/`AddMiddleManZeroResults()` entries
from the `MiddleMan.Zero.AspNetCore.Mvc` README, add `Conflict` rows to the status-mapping
tables where missing, correct any `.NET Standard` / `Result<Order>` documentation claims,
and add documentation of the unified error envelope and links to the `docs/errors/`
files.

#### Scenario: Mvc README no longer references ResultFilter
- **WHEN** the Mvc package README is read
- **THEN** it contains no mention of `ResultFilter` or `AddMiddleManZeroResults()`
- **AND** it documents `ProblemResponse` and links to the error-type docs
