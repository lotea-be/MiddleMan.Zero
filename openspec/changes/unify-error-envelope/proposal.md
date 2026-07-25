# Proposal — unify-error-envelope

> Stage S of QRSPI. Generated 2026-07-25.

## Why

Both HTTP-mapping packages (`MiddleMan.Zero.AspNetCore.Http` and `MiddleMan.Zero.AspNetCore.Mvc`)
translate a `ResultStatus` into an HTTP error response, but their error bodies have drifted apart:
`Failure`/`Undefined` produce different shapes across the two mappers, `NotFound`/`Invalid`/`Conflict`
leak internal `MessageBase` fields (`Id`, `CorrelationId`, `CreatedAt`) in both, and `Forbidden`
returns an empty body on both, silently discarding logged `ForbiddenMessage`s. None of these shapes is
a stable, documented public type.

This change routes every non-2xx `ResultStatus` through one canonical, RFC 7807/9457-conformant error
DTO defined once in `Abstractions`, applied identically by both mappers, and served as
`application/problem+json`. A single shared factory owns the only status-to-body switch, making future
drift structurally impossible. The `2.0.0-rc3` version bump and CHANGELOG entry communicate the
breaking body-shape change to consumers.

## What Changes

- **New POCO `ProblemResponse`** in `MiddleMan.Zero.Abstractions` — sealed, RFC 7807/9457-shaped, no
  ASP.NET dependency. Members: `type`, `title`, `status`, `detail` (all non-null), `traceId`
  (nullable, always null this change), `messages` (non-null `IReadOnlyList<ErrorMessage>`).
- **New POCO `ErrorMessage`** in `MiddleMan.Zero.Abstractions` — sealed, projects `Message` and `Code`
  from `MessageBase`, dropping `Id`/`CorrelationId`/`CreatedAt`.
- **New factory `ProblemResponse.FromResult(ResultBase)`** — public static, owns the only
  status-to-(code, title, type, detail, messages) switch. Both mappers call it; neither contains a
  per-status body switch any more.
- **Rewrite non-success arms of `ToResult`/`ToResult<T>`** (`AspNetCore.Http`) to call the factory and
  emit `application/problem+json`; drop `Results.Forbid()` and `JoinMessages`.
- **Rewrite non-success arms of `ToActionResult`/`ToTypedActionResult`** (`AspNetCore.Mvc`) identically;
  drop `ForbidResult`.
- **403 Forbidden** now returns the envelope populated from logged `ForbiddenMessage`s (empty list if
  none) instead of an empty body.
- **`PublicAPI.Unshipped.txt`** additions for `ProblemResponse`, `ErrorMessage`, and the factory method.
- **5 error-type doc files** under `docs/errors/` (`bad-request.md`, `forbidden.md`, `not-found.md`,
  `conflict.md`, `internal-server-error.md`) — the `type` URI targets.
- **Lockstep cross-package test** asserting byte-identical error bodies and content type across both
  mappers for every `ResultStatus`.
- **Enumerate-all-statuses test** asserting the factory produces a row for each `ResultStatus` value.
- **README fixes** (`#7a`): remove phantom `ResultFilter`/`AddMiddleManZeroResults()`, add `Conflict`
  rows, fix `.NET Standard` / `Result<Order>` documentation claims, document the envelope and link
  the error-type docs.
- **`<Version>` → `2.0.0-rc3`** in `src/Directory.Build.props` + CHANGELOG breaking-change entry.

## Capabilities

### New Capabilities

- `http-error-response`: Unified RFC 7807/9457 error-response envelope for all non-2xx `ResultStatus`
  values, defined in `Abstractions` and consumed identically by both ASP.NET Core mapper packages —
  creates `specs/http-error-response/spec.md`.

### Modified Capabilities

- _none_

## Impact

- Breaking changes: yes — error body shape changes for every non-2xx status (fields added: `type`,
  `title`, `status`, `detail`; fields removed: `Id`, `CorrelationId`, `CreatedAt` from
  `messages[]` items; 403 body changes from empty to JSON). Version bump to `2.0.0-rc3` communicates
  the break.
- Phases: phase 1 (Slice 1 — envelope + factory), phase 2 (Slices 2–3 — Http/Mvc mapper rewrites),
  phase 3 (Slice 4 — lockstep test + docs + version).
- Affected code / APIs / dependencies:
  - `src/MiddleMan.Zero.Abstractions/` — two new types + factory method + `PublicAPI.Unshipped.txt`.
  - `src/MiddleMan.Zero.AspNetCore.Http/` — `ResultExtensions.cs` non-success arms rewritten.
  - `src/MiddleMan.Zero.AspNetCore.Mvc/` — `ResultExtensions.cs` non-success arms rewritten.
  - `tests/MiddleMan.Zero.AspNetCore.Http.Tests/`, `tests/MiddleMan.Zero.AspNetCore.Mvc.Tests/` — assertions updated.
  - `tests/MiddleMan.Zero.Tests/` or shared test — new factory unit tests (enumerate-all + lockstep).
  - `samples/IceCreamTruck.WebApi.Tests/` — extended with JSON body shape assertions.
  - `src/Directory.Build.props` — version bump.
  - `CHANGELOG.md` — breaking-change entry.
  - `docs/errors/` — 5 new Markdown files.
  - Package READMEs — `#7a` fixes.

## Out of scope

- **`#3 bind-cancellation-token`** — CancellationToken auto-bind; a separate follow-up change.
- **`#4 CorrelationId propagation`** — populating `traceId` from `Activity.Current`; only the null
  slot is typed in this change.
- **`ResultFilter`/`AddMiddleManZeroResults()` implementation** — confirmed phantom; removed from docs,
  not implemented.
- **200-OK success path changes** — `Successful` arms and the `ToTypedActionResult` `Response!`
  suppression are explicitly unchanged.
- **Success body wrapping** — no envelope is applied to 2xx responses.
