# MiddleMan.Zero — Backlog

Improvement ideas not yet scheduled as QRSPI changes. Each item carries a
folder-safe **Change id** (verb-first kebab-case); promote an item by running
`/qrspi:questions <change-id>` with that id. The `#N` label is a stable
cross-reference; the change id is what becomes the `openspec/changes/<id>/`
folder and feature branch. Source for the initial batch: a handover scan of a
real consumer (`abkf-registrations`, .NET 10) against `2.0.0-rc1`, re-grounded
against the current source at `2.0.0-rc2`, and pruned after the `2.0.0` release.

## Active

### #3 — CancellationToken auto-bind at the HTTP boundary
**Change id:** `bind-cancellation-token`
**Effort:** Small–Medium · **Packages:** `AspNetCore.Http`

The `IHandleAsync` interface already accepts a `CancellationToken`, but it is
optional (`= default`), so consumers silently call `HandleAsync(request)` and
`HttpContext.RequestAborted` never reaches handlers — long work keeps running
after the client disconnects.

Ship an `AspNetCore.Http` helper/overload that auto-binds `RequestAborted`, and
document the footgun prominently. (A Roslyn analyzer that warns on a token-less
await in an ASP.NET context is a larger, separate follow-up.)

### #4 — CorrelationId propagation + observability
**Change id:** `propagate-correlation-id`
**Effort:** Larger · **Packages:** `Abstractions`, `MiddleMan.Zero`, HTTP mappers

`MessageBase.CorrelationId` was removed before 2.0.0 (it defaulted to a fresh
`Guid.NewGuid()` per message, so it correlated nothing). Nothing currently flows a
correlation/trace id into logs or the HTTP response.

Populate the reserved `ProblemResponse.TraceId` (e.g. from `Activity.Current` /
`HttpContext.TraceIdentifier`) in both HTTP mappers, and integrate with
`Activity`/`System.Diagnostics` for OpenTelemetry traces — additively, without
reintroducing a per-message correlation field.

### #5 — Validation ergonomics (opt-in)
**Change id:** `add-validation-ergonomics`
**Effort:** Medium · **Packages:** `MiddleMan.Zero`

The consumer has **133** near-identical `context.Log(new InvalidRequestMessage(...))`
calls. Keep the "accumulate, never throw" semantics but reduce boilerplate:

- fluent helpers, e.g. `context.Require(request.Name, nameof(request.Name), "…")`
  / `context.RequireRange(...)`;
- a thin `FluentValidation` adapter that funnels failures into
  `InvalidRequestMessage`.

Must stay **opt-in** — the explicit style remains fully supported.

## Constraint since 2.0.0

`PackageValidationBaselineVersion` is `2.0.0`, so every item above must ship as an
**additive** 2.x change; anything binary-breaking waits for 3.0.0.

## Done (shipped in 2.0.0)

- **#1 — `Conflict` (409) status.** `ResultStatus.Conflict` + `ConflictMessage`;
  both HTTP packages map it to 409.
- **#2 — Unified error envelope** (`unify-error-envelope`, archived
  `2026-10-23`). One RFC 9457 `application/problem+json` body for every
  non-success status; spec `openspec/specs/http-error-response`.
- **#6 — Pipeline behaviors.** `IHandlerBehavior<TRequest>` /
  `IHandlerBehavior<TRequest, TResponse>` with `HandlerDelegate` continuations,
  applied by `AddMiddleManZero()`. Shipped in `.Zero` itself rather than the
  previously planned separate `MiddleMan.x` variant package.
- **#7a — Documentation fixes** (`fix-abstractions-docs`). The Abstractions
  README lists both `.AspNetCore.Http` and `.AspNetCore.Mvc` and no longer calls
  its types "typically not used directly".

## Resolved verify-against-source notes

- Exact JSON shape emitted by `ToResult()` per status: defined by #2's spec.
- The void `IHandleAsync<TRequest>` variant is covered by tests
  (`MiddleMan.Zero.Tests`, `DependencyInjection.Tests`, `CancelOrderHandlerTests`)
  and used in the IceCreamTruck sample.
