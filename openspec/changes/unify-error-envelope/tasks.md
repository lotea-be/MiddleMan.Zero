# Tasks — unify-error-envelope

> Stage P of QRSPI. Tick boxes as you implement. Order matters.

## 1. Envelope contract + factory

**Compute:** model=sonnet effort=medium — new sealed POCOs + static switch factory; straightforward but the enum-coverage test and PublicAPI declaration require care

- [x] 1.1 Add `ErrorMessage` sealed record to `src/MiddleMan.Zero.Abstractions/` with `Message` (string) and `Code` (string) init-only properties and XML doc comments on all public members (D3)
- [x] 1.2 Add `ProblemResponse` sealed record to `src/MiddleMan.Zero.Abstractions/` with init-only properties `Type` (string), `Title` (string), `Status` (int), `Detail` (string), `Messages` (IReadOnlyList<ErrorMessage>), and optional `TraceId` (string?); add XML doc comments on all public members (D4)
- [x] 1.3 Implement `ProblemResponse.FromResult(ResultBase result, string? traceId = null)` static factory: one `switch` arm per non-success `ResultStatus` value (`Failure` → 500, `Invalid` → 400, `NotFound` → 404, `Forbidden` → 403, `Conflict` → 409 if present); throw `InvalidOperationException` on `Successful`; populate `Detail` from joined messages when no explicit detail string is provided (D5)
- [x] 1.4 Declare every new public member (`ErrorMessage`, `ProblemResponse`, and all their properties and the static factory) in `src/MiddleMan.Zero.Abstractions/PublicAPI.Unshipped.txt` so RS0016 does not break the build (D3, D4)
- [x] 1.5 Run `dotnet build` across all three TFMs (`net8.0`, `net9.0`, `net10.0`) and confirm zero RS0016 and zero CS1591 errors
- [x] 1.6 Add xUnit unit tests in `tests/MiddleMan.Zero.Tests/` (or a dedicated Abstractions test project) covering: `ErrorMessage` projection (only `Message` + `Code` properties serialized; no `Id`/`CorrelationId`/`CreatedAt` leakage); `ProblemResponse` JSON serialization (all required fields present; `traceId` key absent when `TraceId` is null); `FromResult` factory for every non-success `ResultStatus` value using an enumerate-all `[Theory]`; `FromResult` throws `InvalidOperationException` on `Successful`; `Detail` default-string path vs. joined-messages path (D5)
- [x] 1.7 Unit/integration test: covers happy path (each `ResultStatus` value maps correctly) + 1 error case (`Successful` throws)
- [x] 1.8 Checkpoint: `dotnet test --filter "ProblemResponse" --settings coverlet.runsettings` passes on net8.0, net9.0, net10.0; `dotnet build` reports no RS0016 errors; 95% coverage gate holds

## 2. Http mapper on the envelope

**Compute:** model=sonnet effort=medium — rewriting known extension-method arms and mirroring existing test patterns; no novel reasoning needed

- [ ] 2.1 Rewrite the non-success arms of `ToResult` and `ToResult<T>` in `src/MiddleMan.Zero.AspNetCore.Http/ResultExtensions.cs` to call `ProblemResponse.FromResult(result)` and return `Results.Json(problemResponse, contentType: "application/problem+json", statusCode: <code>)` for each non-success `ResultStatus`; remove the `Results.Forbid()` call and the `JoinMessages` helper if it is now unused (D6, D7)
- [ ] 2.2 Verify that the 200 success arm of `ToResult`/`ToResult<T>` is left unchanged
- [ ] 2.3 Run `dotnet build -f net10.0` (then net9.0, net8.0) and confirm clean build with no CS1591 warnings
- [ ] 2.4 Update `tests/MiddleMan.Zero.AspNetCore.Http.Tests/` — replace the `ForbidHttpResult` assertion with a `ProblemHttpResult` 403 assertion; add or update assertions for status codes 400, 403, 404, 409, 500 to check `Content-Type: application/problem+json` and JSON body fields (`type`, `title`, `status`, `detail`, `messages`); assert the 200 success path is unchanged; assert `Results.Forbid()` is no longer invoked (D6, D7)
- [ ] 2.5 Unit/integration test: covers happy path (each non-success status returns correct `Content-Type` and body shape) + 1 error case (success path unchanged)
- [ ] 2.6 Checkpoint: `dotnet test tests/MiddleMan.Zero.AspNetCore.Http.Tests --settings coverlet.runsettings` passes; 95% coverage gate holds; a manual `HttpClient` or curl call to a test Minimal API endpoint returns `Content-Type: application/problem+json`

## 3. Mvc mapper on the envelope

**Compute:** model=sonnet effort=low — mirrors Slice 2 exactly on the Mvc side; same factory call, same pattern, lower novelty

- [ ] 3.1 Rewrite the non-success arms of `ToActionResult` and `ToTypedActionResult` in `src/MiddleMan.Zero.AspNetCore.Mvc/ResultExtensions.cs` to call `ProblemResponse.FromResult(result)` and return `new ObjectResult(problemResponse) { StatusCode = <code>, ContentTypes = { "application/problem+json" } }` for each non-success `ResultStatus`; remove the `ForbidResult` return (D6, D7)
- [ ] 3.2 Verify that the 200 success arm of `ToActionResult`/`ToTypedActionResult` is left unchanged
- [ ] 3.3 Run `dotnet build -f net10.0` (then net9.0, net8.0) and confirm clean build with no CS1591 warnings
- [ ] 3.4 Update `tests/MiddleMan.Zero.AspNetCore.Mvc.Tests/` — replace the `ForbidResult` assertion with an `ObjectResult` 403 assertion; add or update assertions for status codes 400, 403, 404, 409, 500 to check `Content-Type: application/problem+json` and JSON body fields (`type`, `title`, `status`, `detail`, `messages`); assert `ToTypedActionResult` 200 success path is unchanged; assert `ForbidResult` is no longer returned (D6, D7)
- [ ] 3.5 Unit/integration test: covers happy path (each non-success status returns `ObjectResult` with correct content type and body shape) + 1 error case (success path unchanged)
- [ ] 3.6 Checkpoint: `dotnet test tests/MiddleMan.Zero.AspNetCore.Mvc.Tests --settings coverlet.runsettings` passes; 95% coverage gate holds

## 4. Lockstep proof + docs + version

**Compute:** model=sonnet effort=high — the lockstep test requires serializing both mapper outputs to bytes and comparing them across package boundaries; the IceCreamTruck integration test wiring and the five doc files add breadth; still templated work but the highest surface area of the four slices

- [ ] 4.1 Create a cross-package lockstep test class (e.g. `tests/MiddleMan.Zero.Tests/LockstepHttpMvcTests.cs` or a new shared test project) with a single `[Theory]` parameterized over every non-success `ResultStatus` value; each theory case calls both `ToResult` (Http) and `ToActionResult` (Mvc) with an identical `ResultBase`, serializes each response body to UTF-8 bytes, and asserts byte-identical JSON output and matching `Content-Type: application/problem+json` from both mappers (D8, D9)
- [ ] 4.2 Extend `samples/IceCreamTruck.WebApi.Tests/` with JSON body-shape assertions that verify the full pipeline end-to-end: for at least one non-success scenario per mapper (controller action + minimal endpoint), assert the response body contains the `type`, `title`, `status`, `detail`, and `messages` fields (D9)
- [ ] 4.3 Create the five `docs/errors/*.md` files: `bad-request.md` (400), `forbidden.md` (403), `not-found.md` (404), `conflict.md` (409), `internal-error.md` (500); each file must contain at minimum the HTTP status code, the `type` URI it documents, a human-readable description, and an example JSON body
- [ ] 4.4 Update `README.md`: remove the `ResultFilter` phantom section/reference; update any HTTP-mapping code examples to reflect the `application/problem+json` contract; correct any outdated status→HTTP code tables (D7)
- [ ] 4.5 Add a `## [2.0.0-rc3]` breaking-change section to `CHANGELOG.md` describing the unified error envelope, removal of `ForbidResult`/`Results.Forbid()`, and the new `ProblemResponse`/`ErrorMessage` types
- [ ] 4.6 Bump `<Version>` in `src/Directory.Build.props` from `2.0.0-rc2` to `2.0.0-rc3` in the same commit as the CHANGELOG entry
- [ ] 4.7 Run the full test suite: `dotnet test --settings coverlet.runsettings` (all projects, all TFMs); confirm 95% coverage gate holds and no test is red
- [ ] 4.8 Run `dotnet build` across all three TFMs and confirm clean build with zero warnings-as-errors
- [ ] 4.9 Unit/integration test: lockstep `[Theory]` covers every non-success `ResultStatus` (happy path byte-identity) + 1 error case (Successful is excluded from the theory and tested to throw if called directly on the factory)
- [ ] 4.10 e2e: `IceCreamTruck.WebApi.Tests` asserts JSON body shape through the real ASP.NET Core pipeline for both a controller action (Mvc mapper) and a Minimal API endpoint (Http mapper)
- [ ] 4.11 Checkpoint: `dotnet test --settings coverlet.runsettings` passes in full; `dotnet build` is clean; `src/Directory.Build.props` reads `<Version>2.0.0-rc3</Version>`; `docs/errors/bad-request.md` (and the other four) exist on disk; `CHANGELOG.md` contains a `## [2.0.0-rc3]` breaking-change section
