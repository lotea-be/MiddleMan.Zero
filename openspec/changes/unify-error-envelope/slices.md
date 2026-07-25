# Slices — unify-error-envelope

> Stage V of QRSPI. Generated 2026-07-25.
> Vertical slices, not horizontal layers.

## Overview

The four slices below deliver the unified error envelope one independently-demoable
path at a time. Slice 1 lands the contract (the two POCOs and the factory) and makes
it verifiable through unit tests before any HTTP-wiring is touched. Slices 2 and 3
each rewrite one mapper in isolation — Http then Mvc — so each mapper can be
integration-tested on its own without the other. Slice 4 closes the lockstep guarantee,
adds the documentation files that the `type` URIs resolve to, and bumps the version.
This ordering means that if Slice 2 or 3 is interrupted, the contract layer and at
least one mapper are already merged and correct.

The `(D<n>)` tags embedded throughout this file are required — this `slices.md`
dogfoods the rule it describes. Every slice bullet that implements a named design
decision carries its tag; pure scaffolding bullets (e.g. a test-only line with no
decision reference) carry no tag.

## Slices

### Slice 1 — Envelope contract + factory

The deliverable at the end of this slice is a passing test suite in
`MiddleMan.Zero.Tests` (or a dedicated shared test project) that constructs a
`ProblemResponse` via `FromResult` for every `ResultStatus` value and asserts the
correct `Status`, `Title`, `Type` suffix, `Detail`, and `Messages` mapping. A human
can run `dotnet test --filter "ProblemResponse"` and see every scenario green. There
is no HTTP layer touched yet; the slice is self-contained in `Abstractions`.

- M: no mock service needed — this slice is a library type with no I/O; the
  "demo" is the unit test output (D1)
- F: n/a — this is a .NET library project with no browser UI surface
- D: n/a — no data-store surface; types live entirely in `MiddleMan.Zero.Abstractions`
- T (Tests): xUnit unit tests covering — `ProblemResponse` JSON serialization
  (required fields always present, `traceId` omitted when null), `ErrorMessage`
  projection (only `Message` + `Code`, no `Id`/`CorrelationId`/`CreatedAt`),
  `FromResult` factory for each `ResultStatus` value (enumerate-all enum test), factory
  throws `InvalidOperationException` on `Successful`, `Detail` default string vs.
  joined-messages paths; declare new public members in
  `PublicAPI.Unshipped.txt` so RS0016 does not break the build (D3, D4, D5)
- **Compute:** model=sonnet effort=medium — new sealed POCOs + static switch factory; straightforward but the enum-coverage test and PublicAPI declaration require care
- Checkpoint: `dotnet test --filter "ProblemResponse" --settings coverlet.runsettings` passes on all three TFMs (net8.0, net9.0, net10.0); `dotnet build` reports no RS0016 errors

### Slice 2 — Http mapper on the envelope

The deliverable is a set of integration tests in `MiddleMan.Zero.AspNetCore.Http.Tests`
that verify a Minimal API endpoint returns `application/problem+json` with the correct
status code and body shape for every non-success `ResultStatus`. A human can run
`dotnet test --filter "Http" --settings coverlet.runsettings` and see the new
`ProblemHttpResult`-based assertions green and the old `ForbidHttpResult` assertion
gone. The 200 success path is verified unchanged.

- M: no mock needed — `ResultExtensions.ToResult` is a pure static extension with
  no I/O; the existing test harness constructs results directly (D2)
- F: n/a — library package, no browser UI
- D: n/a — no data-store surface
- T (Tests): update `MiddleMan.Zero.AspNetCore.Http.Tests` — replace `ForbidHttpResult`
  assertion with `ProblemHttpResult` 403 assertion; add/update assertions for 400, 403,
  404, 409, 500 to check `Content-Type: application/problem+json` and JSON body fields
  (`type`, `title`, `status`, `detail`, `messages`); assert 200 success path is
  unchanged; assert `Results.Forbid()` is no longer called (D6, D7)
- **Compute:** model=sonnet effort=medium — rewriting known extension-method arms and mirroring existing test patterns; no novel reasoning needed
- Checkpoint: `dotnet test tests/MiddleMan.Zero.AspNetCore.Http.Tests --settings coverlet.runsettings` passes; 95% coverage gate holds; a manual curl or `HttpClient` call to a test Minimal API endpoint returns `Content-Type: application/problem+json`

### Slice 3 — Mvc mapper on the envelope

The deliverable is a set of integration tests in `MiddleMan.Zero.AspNetCore.Mvc.Tests`
that verify a controller action returns `application/problem+json` with the correct
status code and body shape for every non-success `ResultStatus`. A human can run
`dotnet test --filter "Mvc" --settings coverlet.runsettings` and see the new
`ObjectResult`-based assertions green and the old `ForbidResult` assertion gone.

- M: no mock needed — `ResultExtensions.ToActionResult`/`ToTypedActionResult` are
  pure static extensions; the test harness constructs results directly (D2)
- F: n/a — library package, no browser UI
- D: n/a — no data-store surface
- T (Tests): update `MiddleMan.Zero.AspNetCore.Mvc.Tests` — replace `ForbidResult`
  assertion with `ObjectResult` 403 assertion; add/update assertions for 400, 403, 404,
  409, 500 to check `Content-Type: application/problem+json` and JSON body fields;
  assert `ToTypedActionResult` 200 success path is unchanged; assert `ForbidResult` is
  no longer returned (D6, D7)
- **Compute:** model=sonnet effort=low — mirrors Slice 2 exactly on the Mvc side; same factory call, same pattern, lower novelty
- Checkpoint: `dotnet test tests/MiddleMan.Zero.AspNetCore.Mvc.Tests --settings coverlet.runsettings` passes; 95% coverage gate holds

### Slice 4 — Lockstep proof + docs + version

The deliverable is a byte-identity cross-package test that asserts Http and Mvc produce
the same JSON bytes and `Content-Type` for every error status, five `docs/errors/*.md`
files reachable at the `type` URIs, a corrected README, a `2.0.0-rc3` CHANGELOG entry,
and an updated `<Version>` in `src/Directory.Build.props`. A human can run
`dotnet test --filter "Lockstep" --settings coverlet.runsettings` and see the
byte-identity assertions green, then inspect `docs/errors/` and the CHANGELOG.
`IceCreamTruck.WebApi.Tests` is extended to assert the JSON body shape end-to-end
through the real pipeline.

- M: no mock needed — the lockstep test exercises both mappers through their real code
  paths with in-memory results (D2, D8)
- F: n/a — no browser UI; the README edit is documentation, not a UI component
- D: n/a — no data-store surface
- T (Tests): add cross-package lockstep test (one test class, one `[Theory]` per
  `ResultStatus` error value) asserting byte-identical JSON bodies and matching
  `Content-Type` from both `ToResult` and `ToActionResult`; extend
  `samples/IceCreamTruck.WebApi.Tests` with JSON body shape assertions (D9)
- **Compute:** model=sonnet effort=high — the lockstep test requires serializing both mapper outputs to bytes and comparing them across package boundaries; the IceCreamTruck integration test wiring and the five doc files add breadth; still templated work but the highest surface area of the four slices
- Checkpoint: `dotnet test --settings coverlet.runsettings` passes in full (all projects, 95% gate); `dotnet build` is clean; `src/Directory.Build.props` reads `<Version>2.0.0-rc3</Version>`; `docs/errors/bad-request.md` (and the other four) exist on disk; the CHANGELOG contains a `## [2.0.0-rc3]` breaking-change section
