# MiddleMan.Zero — Backlog

Improvement ideas not yet scheduled as QRSPI changes. Each item carries a
folder-safe **Change id** (verb-first kebab-case); promote an item by running
`/qrspi:questions <change-id>` with that id. The `#N` label is a stable
cross-reference; the change id is what becomes the `openspec/changes/<id>/`
folder and feature branch. Source for the initial batch: a handover scan of a
real consumer (`abkf-registrations`, .NET 10) against `2.0.0-rc1`, re-grounded
against the current source at `2.0.0-rc2`.

## Active

### #2 — Canonical, documented error-response envelope
**Change id:** `unify-error-envelope`
**Status:** proposed (change folder created 2026-07-25)
**Effort:** Medium · **Packages:** `AspNetCore.Http` + `AspNetCore.Mvc` (lockstep)

`ToResult()` currently emits three different body shapes: `{ messages: [...] }`
for 400/404/409, an RFC 7807 `ProblemDetails` for 500, and an **empty body** for
403 (`Results.Forbid()`). A consumer extending the contract (e.g. the 409
workaround that predated `ResultStatus.Conflict`) has nothing stable to target.

Route **every** status through one canonical, `ProblemDetails`-compatible DTO;
document the shape; keep Http and Mvc mappings in sync. Ties into #4 (surface
`CorrelationId`/`traceId` in the envelope).

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

`MessageBase` exposes `Id`, `CorrelationId`, `CreatedAt`, but `CorrelationId`
defaults to a **fresh `Guid.NewGuid()` per message**, so it correlates nothing,
and nothing flows it into logs or the HTTP response.

Share one correlation id per `HandlerContext`, propagate it into logs and
(optionally) the error envelope (#2), and integrate with
`Activity`/`System.Diagnostics` for OpenTelemetry traces.

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

### #7a — Documentation fixes
**Change id:** `fix-abstractions-docs`
**Effort:** Quick · **Packages:** docs only

- `MiddleMan.Zero.Abstractions/README.md` lists **`MiddleMan.Zero.AspNetCore.Mvc`**
  under related packages, but Minimal-API consumers use **`.AspNetCore.Http`** —
  make the related-packages list reflect both integration paths.
- README says the Abstractions types are "typically not used directly," yet
  endpoints reference `IHandleAsync<>` / `ResultBase` directly. Clarify the
  intended public surface.

## Parked / future direction

### #6 — Pluggable behavior pipeline (deferred)
**Change id:** `add-behavior-pipeline`
**Status:** Not scheduled — revisit after MiddleMan.Zero matures.

There is no MediatR-style `IPipelineBehavior` / middleware chain; consumers bake
cross-cutting concerns (transactions, user resolution) into a project-owned
`TransactionalHandlerBase` inherited by every write handler.

Direction: rather than growing `MiddleMan.Zero`, the pipeline (and other
batteries-included, cross-cutting features) will live in a **separate variant
package** — referred to here by the placeholder **`MiddleMan.x`** until a name
is chosen. Keeping it out of `.Zero` preserves the ".Zero = minimal,
no-dependency" promise, and lets the variant take dependencies `.Zero` refuses
(UoW/transaction abstractions, logging).

Lean: develop `MiddleMan.Zero` first; stand up `MiddleMan.x` later, once the
core is mature. Name to be decided at that point.

## Done (from the original handover, already shipped)

- **#1 — `Conflict` (409) status.** `ResultStatus.Conflict` + `ConflictMessage`
  landed post-handover (commit `50611d7`); both `AspNetCore.Http` and
  `AspNetCore.Mvc` map it to HTTP 409.

## Verify-against-source notes (open questions, not work items)

- Exact JSON shape emitted by `ToResult()` per status — feeds #2.
- Whether the void `IHandleAsync<TRequest>` (no-response) variant is
  tested/used; the scanned consumer only used the typed form.
