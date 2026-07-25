# Research — unify-error-envelope

> Stage R of QRSPI. Generated 2026-07-25.
> Ticket is hidden from this stage by design.

## Areas investigated

- **HTTP result → response mapping**: `ResultExtensions.cs` in both AspNetCore packages — exact per-status HTTP code and body shape.
- **Result & message contract**: `ResultBase`, `ResultBase<TResponse>`, `ResultStatus` enum, `MessageBase` and all six concrete subtypes.
- **Existing ProblemDetails / error-body construction**: every call site that builds an error body in the two AspNetCore packages.
- **Public-API surface tracking**: `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` per project, `Microsoft.CodeAnalysis.PublicApiAnalyzers` setup, `Directory.Build.props` strictness.
- **Test conventions for the HTTP layer**: `AspNetCore.Http.Tests`, `AspNetCore.Mvc.Tests`, `IceCreamTruck.WebApi.Tests` — assertion patterns, xUnit/Shouldly conventions.
- **Package documentation**: README files in both AspNetCore packages and `MiddleMan.Zero.Abstractions` — documented vs. actual API surface.
- **Versioning & changelog mechanics**: version location, current value, CHANGELOG structure.

---

## File map

### Area 1 — HTTP result → response mapping

- `src/MiddleMan.Zero.AspNetCore.Http/ResultExtensions.cs` — sole production source file for the Minimal API mapping package. Exports: `ResultExtensions` (static class). Two public extension methods: `ToResult(this ResultBase)` and `ToResult<TResponse>(this ResultBase<TResponse>)`.
- `src/MiddleMan.Zero.AspNetCore.Mvc/ResultExtensions.cs` — sole production source file for the MVC mapping package. Exports: `ResultExtensions` (static class). Three public extension methods: `ToActionResult(this ResultBase)`, `ToActionResult<TResponse>(this ResultBase<TResponse>)`, `ToTypedActionResult<TResponse>(this ResultBase<TResponse>)`.

### Area 2 — Result & message contract

- `src/MiddleMan.Zero.Abstractions/ResultBase.cs` — `ResultBase` (abstract, primary-constructor) and `ResultBase<TResponse>` (abstract). Depends on `ResultStatus`, `MessageBase`.
- `src/MiddleMan.Zero.Abstractions/ResultStatus.cs` — `ResultStatus` enum.
- `src/MiddleMan.Zero.Abstractions/MessageBase.cs` — `MessageBase` (abstract class). Depends on nothing external.
- `src/MiddleMan.Zero/Result.cs` — sealed concrete `Result : ResultBase` and `Result<TResponse> : ResultBase<TResponse>`.
- `src/MiddleMan.Zero/Messages/DebugMessage.cs` — `DebugMessage : MessageBase`.
- `src/MiddleMan.Zero/Messages/FailureMessage.cs` — `FailureMessage : MessageBase`.
- `src/MiddleMan.Zero/Messages/ForbiddenMessage.cs` — `ForbiddenMessage : MessageBase`.
- `src/MiddleMan.Zero/Messages/NotFoundMessage.cs` — `NotFoundMessage : MessageBase`.
- `src/MiddleMan.Zero/Messages/InvalidRequestMessage.cs` — `InvalidRequestMessage : MessageBase`.
- `src/MiddleMan.Zero/Messages/ConflictMessage.cs` — `ConflictMessage : MessageBase`.
- `src/MiddleMan.Zero/HandlerContext.cs` — `HandlerContext` (mutable state accumulator). Depends on all six message types.
- `src/MiddleMan.Zero/HandlerBase.cs` — `HandlerBase<TRequest>` and `HandlerBase<TRequest, TResponse>` (abstract templates). Depends on `HandlerContext`, all message types, `ResultBase`.

### Area 3 — Existing ProblemDetails / error-body construction

- `src/MiddleMan.Zero.AspNetCore.Http/ResultExtensions.cs` — all error-body construction sites.
- `src/MiddleMan.Zero.AspNetCore.Mvc/ResultExtensions.cs` — all error-body construction sites.

### Area 4 — Public-API surface tracking

- `src/MiddleMan.Zero.Abstractions/PublicAPI.Shipped.txt` — contains the full v2 surface (36 lines). `PublicAPI.Unshipped.txt` — empty (`#nullable enable` only).
- `src/MiddleMan.Zero/PublicAPI.Shipped.txt` — empty. `PublicAPI.Unshipped.txt` — 54 lines (full current surface: all message types, `HandlerBase<>` variants, `HandlerContext`, `Result`, `Result<T>`).
- `src/MiddleMan.Zero.AspNetCore.Http/PublicAPI.Shipped.txt` — empty. `PublicAPI.Unshipped.txt` — 4 lines (`ResultExtensions` class + 2 `ToResult` overloads).
- `src/MiddleMan.Zero.AspNetCore.Mvc/PublicAPI.Shipped.txt` — empty. `PublicAPI.Unshipped.txt` — 5 lines (`ResultExtensions` class + 3 method overloads).
- `src/MiddleMan.Zero.DependencyInjection/PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` — present but not surveyed (out of area scope).
- `Directory.Build.props` (root) — `TreatWarningsAsErrors=true`, `Nullable=enable`, `EnforceCodeStyleInBuild=true`, `GenerateDocumentationFile=true`, `BuildInParallel=false`.
- `src/Directory.Build.props` — imports root, adds `<Version>2.0.0-rc2</Version>`, package metadata, `EnablePackageValidation=true`, `Microsoft.CodeAnalysis.PublicApiAnalyzers` (PrivateAssets=All), `AdditionalFiles` for both `PublicAPI.*.txt` per package.
- `tests/Directory.Build.props` — imports root, suppresses `CS1591` (doc-comment warning), configures coverlet with `Threshold` not set inline (threshold enforced via `coverlet.runsettings`). Auto-imports global `using Xunit` and `using Shouldly`.

### Area 5 — Test conventions for the HTTP layer

- `tests/MiddleMan.Zero.AspNetCore.Http.Tests/ResultExtensionsTests.cs` — 14 unit tests covering all 7 status values × 2 overloads.
- `tests/MiddleMan.Zero.AspNetCore.Mvc.Tests/ResultExtensionsTests.cs` — 17 unit tests covering all 7 status values × 3 overloads, plus 2 message-in-body tests.
- `tests/IceCreamTruck.WebApi.Tests/Controllers/OrdersControllerTests.cs` — 12 integration tests using `AuthenticatedWebApplicationFactory` (custom `WebApplicationFactory<Program>` that registers a no-op authentication scheme for `ForbidResult` to produce HTTP 403).
- `tests/IceCreamTruck.WebApi.Tests/Endpoints/FlavorEndpointsTests.cs` — 7 integration tests using `WebApplicationFactory<Program>` directly (no custom factory, no auth needed).
- `tests/IceCreamTruck.WebApi.Tests/AuthenticatedWebApplicationFactory.cs` — `AuthenticatedWebApplicationFactory : WebApplicationFactory<Program>`, `NoOpAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>` (always authenticates, enabling `ForbidResult` → HTTP 403 without throwing).

---

## Public API surface

### `MiddleMan.Zero.AspNetCore.Http.ResultExtensions`

```
static IResult ToResult(this ResultBase result)
static IResult ToResult<TResponse>(this ResultBase<TResponse> result)
```

### `MiddleMan.Zero.AspNetCore.Mvc.ResultExtensions`

```
static IActionResult ToActionResult(this ResultBase result)
static IActionResult ToActionResult<TResponse>(this ResultBase<TResponse> result)
static ActionResult<TResponse> ToTypedActionResult<TResponse>(this ResultBase<TResponse> result)
```

---

## Data model

### `ResultStatus` enum (`src/MiddleMan.Zero.Abstractions/ResultStatus.cs`)

| Value | Integer |
|---|---|
| `Undefined` | 0 |
| `Successful` | 1 |
| `Failure` | 2 |
| `Invalid` | 3 |
| `NotFound` | 4 |
| `Forbidden` | 5 |
| `Conflict` | 6 |

### `MessageBase` (`src/MiddleMan.Zero.Abstractions/MessageBase.cs`)

All five properties are `init`-only (enforced since v2.0.0).

| Property | Type | Default |
|---|---|---|
| `Id` | `Guid` | `Guid.NewGuid()` |
| `CorrelationId` | `Guid` | `Guid.NewGuid()` |
| `CreatedAt` | `DateTime` | `DateTime.UtcNow` |
| `Message` | `string` | `string.Empty` |
| `Code` | `string` | `string.Empty` |

Constructors: `()`, `(string message)`, `(string message, string code)`.
`ToString()` returns `Message` property value.

### Message subtypes (all in `src/MiddleMan.Zero/Messages/`, namespace `MiddleMan.Zero`)

Each has identical three constructors (`()`, `(string message)`, `(string message, string code)`). No additional properties beyond `MessageBase`.

| Type | `HandlerContext.Log(…)` side-effects |
|---|---|
| `DebugMessage` | none — flags unchanged |
| `InvalidRequestMessage` | `IsRequestValid = false`, `IsSuccessful = false` |
| `FailureMessage` | `IsSuccessful = false` |
| `NotFoundMessage` | `IsSuccessful = false`, `IsNotFound = true` |
| `ForbiddenMessage` | `IsSuccessful = false`, `IsForbidden = true` |
| `ConflictMessage` | `IsSuccessful = false`, `IsConflict = true` |

### `ResultBase` and `ResultBase<TResponse>` (`src/MiddleMan.Zero.Abstractions/ResultBase.cs`)

- `ResultBase`: abstract, primary constructor `(ResultStatus resultStatus, IEnumerable<MessageBase> messages)`. Properties: `ResultStatus ResultStatus { get; }`, `MessageBase[] Messages { get; }`.
- `ResultBase<TResponse>`: abstract, primary constructor `(TResponse? response, ResultStatus resultStatus, IEnumerable<MessageBase> messages)`. Property: `TResponse? Response { get; }`. Invariant: throws `ArgumentNullException("response", "Response cannot be null when ResultStatus is Successful.")` if `response` is null and `resultStatus == Successful`.

### `Result` / `Result<TResponse>` (`src/MiddleMan.Zero/Result.cs`)

Sealed concrete implementations. No added members.

---

## Per-status HTTP mapping — exact current state

### `MiddleMan.Zero.AspNetCore.Http` (`ResultExtensions.cs:17-61`)

| `ResultStatus` | `IResult` type | HTTP code | Body |
|---|---|---|---|
| `Successful` | `Results.Ok()` | 200 | empty |
| `Successful` (generic) | `Results.Ok(result.Response)` | 200 | response object |
| `NotFound` | `Results.NotFound(new { messages = result.Messages })` | 404 | `{ "messages": [...] }` anonymous |
| `Invalid` | `Results.BadRequest(new { messages = result.Messages })` | 400 | `{ "messages": [...] }` anonymous |
| `Failure` | `Results.Problem(detail: JoinMessages(…), statusCode: 500)` | 500 | `ProblemDetails` with `detail` = messages joined by `"; "` |
| `Forbidden` | `Results.Forbid()` | 403 | empty |
| `Conflict` | `Results.Conflict(new { messages = result.Messages })` | 409 | `{ "messages": [...] }` anonymous |
| `Undefined` (default arm) | `Results.Problem(detail: JoinMessages(…), statusCode: 500)` | 500 | `ProblemDetails` with `detail` = messages joined by `"; "` |

Private helper at line 59: `JoinMessages` → `string.Join("; ", messages.Select(m => m.Message))`.

### `MiddleMan.Zero.AspNetCore.Mvc` (`ResultExtensions.cs:15-85`)

| `ResultStatus` | `IActionResult` type | HTTP code | Body |
|---|---|---|---|
| `Successful` | `new OkResult()` | 200 | empty |
| `Successful` (generic) | `new OkObjectResult(result.Response)` | 200 | response object |
| `Successful` (typed) | `result.Response!` (implicit `ActionResult<T>`) | 200 | response object |
| `NotFound` | `new NotFoundObjectResult(new { messages = result.Messages })` | 404 | `{ "messages": [...] }` anonymous |
| `Invalid` | `new BadRequestObjectResult(new { messages = result.Messages })` | 400 | `{ "messages": [...] }` anonymous |
| `Failure` | `new ObjectResult(new { messages = result.Messages }) { StatusCode = 500 }` | 500 | `{ "messages": [...] }` anonymous |
| `Forbidden` | `new ForbidResult()` | 403 | empty |
| `Conflict` | `new ConflictObjectResult(new { messages = result.Messages })` | 409 | `{ "messages": [...] }` anonymous |
| `Undefined` (default arm) | `new ObjectResult(new { messages = result.Messages }) { StatusCode = 500 }` | 500 | `{ "messages": [...] }` anonymous |

---

## Divergence between the two packages

| Status | Http body | Mvc body |
|---|---|---|
| `Failure` | `ProblemDetails` (`detail` = joined `Message` strings, no `messages` array) | Anonymous `{ messages: MessageBase[] }` |
| `Undefined` | `ProblemDetails` (same) | Anonymous `{ messages: MessageBase[] }` |
| `NotFound` | Anonymous `{ messages: MessageBase[] }` | Anonymous `{ messages: MessageBase[] }` — identical |
| `Invalid` | Anonymous `{ messages: MessageBase[] }` | Anonymous `{ messages: MessageBase[] }` — identical |
| `Conflict` | Anonymous `{ messages: MessageBase[] }` | Anonymous `{ messages: MessageBase[] }` — identical |
| `Forbidden` | `Results.Forbid()` — empty body | `new ForbidResult()` — empty body — both are body-less |

For `Failure` / `Undefined`: Http uses `Results.Problem(detail: …, statusCode: 500)` (producing a `ProblemDetails` JSON object with `type`, `title`, `status`, `detail` fields); Mvc uses a plain anonymous `{ messages }` object. These two bodies are structurally different for the same status.

For all other error statuses (NotFound, Invalid, Conflict): both packages produce an anonymous `{ "messages": MessageBase[] }` object, where `MessageBase[]` is the full rich array including `Id`, `CorrelationId`, `CreatedAt`, `Message`, `Code` fields per element.

---

## Existing error-body construction call sites

### `src/MiddleMan.Zero.AspNetCore.Http/ResultExtensions.cs`

- **Line 22** (`NotFound`): `Results.NotFound(new { messages = result.Messages })`
- **Line 23** (`Invalid`): `Results.BadRequest(new { messages = result.Messages })`
- **Lines 24-27** (`Failure`): `Results.Problem(detail: JoinMessages(result.Messages), statusCode: 500)`
- **Line 28** (`Forbidden`): `Results.Forbid()` — no body
- **Line 29** (`Conflict`): `Results.Conflict(new { messages = result.Messages })`
- **Lines 29-32** (`Undefined`/default): `Results.Problem(detail: JoinMessages(result.Messages), statusCode: 500)`
- Same pattern repeated at lines 43-56 for the generic overload.

### `src/MiddleMan.Zero.AspNetCore.Mvc/ResultExtensions.cs`

- **Line 20** (`Successful`, non-generic): `new OkResult()`
- **Line 21** (`NotFound`): `new NotFoundObjectResult(new { messages = result.Messages })`
- **Line 22** (`Invalid`): `new BadRequestObjectResult(new { messages = result.Messages })`
- **Lines 23-26** (`Failure`): `new ObjectResult(new { messages = result.Messages }) { StatusCode = 500 }`
- **Line 27** (`Forbidden`): `new ForbidResult()` — no body
- **Line 28** (`Conflict`): `new ConflictObjectResult(new { messages = result.Messages })`
- **Lines 28-31** (`Undefined`/default): `new ObjectResult(new { messages = result.Messages }) { StatusCode = 500 }`
- Same pattern repeated at lines 41-58 (generic `ToActionResult`) and lines 67-84 (`ToTypedActionResult`).

No `Microsoft.AspNetCore.Mvc.ProblemDetails` is constructed anywhere in either package. No `Results.Problem(…)` call in the Mvc package. No `Results.Forbid()` / `ForbidResult` carries any body.

---

## Implicit contracts and conventions

1. **Anonymous object shape `{ messages: MessageBase[] }`** is the established error envelope for NotFound, Invalid, Conflict in both packages. The property name is lowercase `messages`. The value is the full `MessageBase[]` array (not projected — consumers receive `Id`, `CorrelationId`, `CreatedAt`, `Message`, `Code` per element).

2. **`Failure` uses a different envelope in Http vs. Mvc.** Http uses `ProblemDetails` (RFC 9457 / RFC 7807 structure); Mvc uses the same anonymous `{ messages }` object as the other statuses. This is not documented as intentional in the README or CHANGELOG.

3. **`Forbidden` body is always empty** (`Results.Forbid()` / `new ForbidResult()`). Any `ForbiddenMessage` logged by the handler is discarded from the HTTP response body. The messages appear in `ResultBase.Messages` but `ForbidResult` carries no body.

4. **`JoinMessages` private helper in Http only.** `src/MiddleMan.Zero.AspNetCore.Http/ResultExtensions.cs:59` — only used for `Failure`/`Undefined` cases where `Results.Problem(detail:…)` is called. The Mvc package has no equivalent helper because it passes the full array.

5. **`HandlerBase.CreateResult` message filtering.** When building a `Result`, `CreateResult` passes only the messages of the matching type (e.g. `context.Get<NotFoundMessage>()` for NotFound), not all messages. The only exception is `Successful` which passes `context.GetAllMessages()`. This means `DebugMessage`s logged alongside errors are dropped from the final `Result.Messages` for error statuses.

6. **Status precedence in `HandlerBase.CreateResult`** (both overloads, `src/MiddleMan.Zero/HandlerBase.cs`): `Forbidden` > `Invalid` > `Conflict` > `Successful` > `NotFound` > `Failure`. (Note: CHANGELOG states this order; the CLAUDE.md states `Forbidden > Invalid > Successful > NotFound > Failure` — omitting Conflict. The code itself at lines 65-91 confirms: Forbidden → Invalid → Conflict → Successful → NotFound → Failure.)

7. **`PublicAPI.Unshipped.txt` pattern.** For `MiddleMan.Zero.Abstractions`, the full surface is in `Shipped.txt`. For the three other shipped packages (`MiddleMan.Zero`, `AspNetCore.Http`, `AspNetCore.Mvc`), `Shipped.txt` is empty and `Unshipped.txt` carries the current surface — indicating these packages have not yet had a formal release that "locks" their shipped surface.

8. **Tests use reflection to inspect anonymous types.** The `{ messages = result.Messages }` body is an anonymous C# type; tests access it via `value.GetType().GetProperty("messages").GetValue(value)` (e.g. `AspNetCore.Http.Tests:ResultExtensionsTests.cs:123`, `AspNetCore.Mvc.Tests:ResultExtensionsTests.cs:439`). This is the established pattern for asserting on the envelope shape.

9. **Integration tests assert on `response.StatusCode` and deserialized JSON, not on `IResult`/`IActionResult` types directly.** `IceCreamTruck.WebApi.Tests` uses `HttpStatusCode` enum comparisons and `response.Content.ReadAsStringAsync(…).ShouldContain("…")` for body text. The `FlavorEndpointsTests.AddFlavor_WithDuplicateFlavorName_ReturnsBadRequest` test expects `HttpStatusCode.BadRequest` (400), not 409 — the `AddFlavorHandler` uses `InvalidRequestMessage` for duplicate-flavor validation, not `ConflictMessage`.

10. **`ForbidResult` requires an authentication scheme to be registered** to produce HTTP 403 (rather than throwing). `AuthenticatedWebApplicationFactory` registers a no-op scheme (`NoOpAuthHandler`) precisely to satisfy this requirement for integration tests.

11. **xUnit v3 conventions.** Global `using Xunit` and `using Shouldly` are injected via `tests/Directory.Build.props`. No explicit `using` lines in test files. `TestContext.Current.CancellationToken` is passed to all async calls. `FakeItEasy` is available in `tests/Directory.Packages.props` but is not used in the HTTP layer tests surveyed.

---

## README documentation vs. source discrepancies

### `src/MiddleMan.Zero.AspNetCore.Mvc/README.md`

1. **`ResultFilter` / `AddMiddleManZeroResults()` — documented but do not exist in source.** The README at lines 8, 27, 34, 39, 61 describes a `ResultFilter` and a `AddMiddleManZeroResults()` extension on `IMvcBuilder`. Neither exists anywhere in `src/MiddleMan.Zero.AspNetCore.Mvc/`. The only public surface is `ResultExtensions` with the three `ToActionResult`/`ToTypedActionResult` extension methods.

2. **`IHandleAsync<GetOrderRequest, Result<Order>>` is incorrect.** The README at line 51 shows the type parameter as `Result<Order>`, which would be doubly-wrapped. The correct type is `IHandleAsync<GetOrderRequest, Order>` (as seen in the actual sample `OrdersController.cs:48`).

3. **`Conflict` status is not in the README's HTTP mapping table.** The table at lines 114-122 lists only 6 rows (Successful×2, NotFound, Invalid, Forbidden, Failure) — `Conflict` → 409 is absent, though it exists in source.

### `src/MiddleMan.Zero.AspNetCore.Http/README.md`

4. **`Conflict` status is not in the HTTP mapping table.** The table at lines 83-90 omits the `Conflict` → 409 row, though it exists in source.

### `src/MiddleMan.Zero.Abstractions/README.md`

5. **Claims ".NET Standard 2.1 or higher".** The actual target frameworks are `net8.0;net9.0;net10.0`, not .NET Standard.

6. **`ResultStatus` enum list omits `Conflict`.** The README lists only 6 values (Undefined, Successful, Failure, Invalid, NotFound, Forbidden) and does not mention `Conflict = 6`.

---

## Versioning and changelog mechanics

- **Current version**: `2.0.0-rc2` — set at `src/Directory.Build.props:9` (`<Version>2.0.0-rc2</Version>`). All five packages share this value.
- **Version auto-tag**: bumping `<Version>` on `main` triggers `ci.yml` to auto-create the `vX.Y.Z` tag; `publish-nuget.yml` (tag-triggered) builds, packs, and pushes to NuGet.org + GitHub Packages.
- **`PackageValidationBaselineVersion` is intentionally commented out** (`src/Directory.Build.props:48`) — the API-compat baseline diff gate is inactive until 2.0.0 is released stable on NuGet.
- **CHANGELOG.md** (`CHANGELOG.md`) — follows Keep a Changelog / Semantic Versioning format. Two entries observed:
  - `[2.0.0] - 2026-05-08` — major release with breaking changes (`MessageBase` init-only, `Response` nullability, `HandlerBase<TRequest,TResponse>` interface fix) and additions (Conflict status, all message constructors, `MessageBase.ToString()`, `AddMiddleManZero` overloads, `ToTypedActionResult` constraint removal). Http `Failure` ProblemDetails fix noted.
  - `[1.2.0] - 2026-04-18` — added `Forbidden` status + mapping.
- **CHANGELOG update policy**: the rule (enforced by convention, not CI) is to update `CHANGELOG.md` in the same commit as the version bump.

---

## Open gaps

- [ ] Cannot determine whether the `AddMiddleManZeroResults()` / `ResultFilter` documented in the Mvc README was removed in v2.0.0 or was never implemented — no `[Removed]` entry in CHANGELOG.md covers it; would need git history.
- [ ] The `coverlet.runsettings` file was not read — the exact `Threshold=95` configuration and any per-package excludes are unconfirmed from source; known only from CLAUDE.md/README description.
- [ ] `src/MiddleMan.Zero.DependencyInjection` source files were not surveyed — `AddMiddleManZero()` overloads, assembly-scanning behavior, and `PublicAPI.*.txt` content are not mapped.
- [ ] The `.editorconfig` was not read — the exact enforced style rules (beyond what CLAUDE.md documents) are unconfirmed.
- [ ] No gap found on the `Forbidden` body-empty behavior being intentional vs. accidental — it is consistent across both packages but there is no code comment or CHANGELOG entry explaining the design choice.
- [ ] `samples/IceCreamTruck.WebApi/Endpoints/FlavorEndpoints.cs` and `samples/IceCreamTruck.WebApi/Program.cs` were not surveyed — the exact minimal API endpoint wiring is unread (only test assertions against it were read).
