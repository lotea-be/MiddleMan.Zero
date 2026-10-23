# Design — unify-error-envelope

> Stage D of QRSPI. Generated 2026-07-25.
> **Implementation is BLOCKED until a human approves this file.**

## Context

Both HTTP-mapping packages (`AspNetCore.Http` `ToResult()`, `AspNetCore.Mvc`
`ToActionResult()`/`ToTypedActionResult()`) translate a `ResultStatus` into an
HTTP response, but their error bodies have drifted apart (research.md, "Divergence
between the two packages"): `Failure`/`Undefined` emit an RFC-7807 `ProblemDetails`
in Http but an anonymous `{ messages }` object in Mvc; `NotFound`/`Invalid`/`Conflict`
emit an anonymous `{ messages: MessageBase[] }` in both (leaking `Id`/`CorrelationId`/
`CreatedAt`); `Forbidden` returns an empty body on both, silently discarding any
logged `ForbiddenMessage`. None of these shapes is a stable, documented public type.

This change routes **every** non-2xx `ResultStatus` through one canonical,
RFC 7807/9457-conformant error DTO defined once in `Abstractions` and applied
**identically** by both mappers, served as `application/problem+json`. It also
folds in backlog #7a (README/doc fixes). The 2xx success path is deliberately
untouched. After this change the error contract is one named, versioned, documented
type; the two mappers share one construction path so they cannot drift; and a
future `ResultStatus` cannot be added without touching that shared path.

## Goals / Non-Goals

**Goals:**
- One canonical error DTO (pure POCO, `Abstractions`, no ASP.NET dependency — PQ1).
- RFC 7807/9457 core fields + `messages[]` extension + null `traceId` slot (PQ2/3/4/6).
- Identical body for the same `ResultStatus` across both packages, structurally
  enforced against future drift (technical Q10).
- 403 Forbidden now carries the envelope, populated from logged `ForbiddenMessage`s (PQ5).
- README/CHANGELOG doc fixes (#7a) + version bump to `2.0.0-rc3` (PQ8/PQ9).

**Non-Goals (named follow-ups):**
- **#3 `bind-cancellation-token`** — CancellationToken auto-bind. Separate next change (PQ7).
- **#4 CorrelationId propagation** — populating `traceId` from `Activity.Current` and
  the `Guid`→`string` `MessageBase.CorrelationId` unification (PQ6/PQ10). This change
  only *types* the slot.
- **`ResultFilter`/`AddMiddleManZeroResults()`** — a README phantom; removed from docs,
  **not** implemented (PQ8).
- No change to the 200-OK success path (raw `TResponse`), and no wrapping of success
  bodies (PQ7 / technical Q7 / Q12).

## Decisions

### D1 — DTO type: a sealed POCO `ProblemResponse` in `Abstractions` (PQ1, PQ2; named via OQ1a)
Add `MiddleMan.Zero.Abstractions.ProblemResponse` (sealed; `Abstractions` is already the
home of `ResultBase`/`MessageBase`, so both mappers see it transitively via their existing
`ProjectReference` to `MiddleMan.Zero` — **no new project reference or package**, confirmed
against both `.csproj` files). It does **not** inherit `Microsoft.AspNetCore.Mvc.ProblemDetails`
(that would drag ASP.NET into `Abstractions` — foreclosed by PQ1); it is RFC-7807-*shaped*.
Rejected: shared `AspNetCore.Common` package (adds a package, breaks `.Zero = minimal`),
one AspNetCore pkg referencing the other (asymmetric coupling), duplication (drift risk).

Property set and nullability contract (in `Nullable=enable`; every prop needs an XML doc
comment or CS1591 fails). Note: after OQ2 (`type` = in-repo doc URL, always set) and OQ3
(`detail` = generic default when no messages, always set), **only `traceId` is nullable**:

| JSON name | C# member | Type | Nullability rule |
|-----------|-----------|------|------------------|
| `type`    | `Type`    | `string`  | non-null; always set to the per-status doc URL (D3, OQ2) |
| `title`   | `Title`   | `string`  | non-null; always set from the status→title table (D3) |
| `status`  | `Status`  | `int`     | non-null; the HTTP status code |
| `detail`  | `Detail`  | `string`  | non-null; joined messages, or a per-status default when empty (D5, OQ3) |
| `traceId` | `TraceId` | `string?` | nullable, **always null this change** (PQ6/PQ10; #4 owns it) |
| `messages`| `Messages`| `IReadOnlyList<ErrorMessage>` | non-null; **empty list, never null** |

Serialization: `[JsonPropertyName]` camelCase on each member (the assembly cannot assume the
host's `JsonSerializerOptions`, so name each explicitly rather than rely on a global policy).
`TraceId` gets `[JsonIgnore(WhenWritingNull)]` so the null slot disappears from the body
rather than serializing `"traceId": null` (the other members are always populated).

### D2 — Per-error item: sealed `ErrorMessage { Message, Code }` (PQ3, PQ4)
The `messages[]` items are a new sealed `MiddleMan.Zero.Abstractions.ErrorMessage` carrying
**`message` + `code` only** — *not* the raw `MessageBase[]`. This is the fix for today's
leak of `Id`/`CorrelationId`/`CreatedAt`. Array name is **`messages`** (PQ3 — matches today's
shape, lowest migration cost). A dedicated projection type (not reusing `MessageBase`) is
required precisely to drop the three unwanted fields; mapping is
`m => new ErrorMessage { Message = m.Message, Code = m.Code }`. Both `Message` and `Code` are
non-null `string` (they default to `string.Empty` on `MessageBase`, never null).

### D3 — Single shared status→(code, title, type, detail) factory in `Abstractions` (technical Q10; OQ1b, OQ2)
The heart of the lockstep guarantee. Add one **public static** builder,
`ProblemResponse.FromResult(ResultBase result)` (a method **on the DTO** — OQ1b; public because
both mappers live in different assemblies and must call it), that owns the **only** `switch` over
`ResultStatus` for the error path and returns a fully-populated `ProblemResponse` (status code,
title, type, detail, projected messages). Both mappers call it and then only wrap the returned
DTO in their framework result type + set the content type. Neither mapper contains a per-status
`switch` for bodies any more — they cannot disagree on a body because there is exactly one
body-builder.

Canonical mapping table (applied identically in both packages). The `type` URIs (OQ2) point at
in-repo docs committed by this change (Slice 4), served as raw text from the default branch —
base `https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/`:

| `ResultStatus` | HTTP | `title` | `type` (→ `<base>/<slug>.md`) | default `detail` when no messages (OQ3) |
|----------------|------|---------|-------------------------------|------------------------------------------|
| `Invalid`   | 400 | `"Bad Request"`     | `bad-request.md`           | `"The request is invalid."` |
| `Forbidden` | 403 | `"Forbidden"`       | `forbidden.md`             | `"Access denied."` |
| `NotFound`  | 404 | `"Not Found"`       | `not-found.md`             | `"The requested resource was not found."` |
| `Conflict`  | 409 | `"Conflict"`        | `conflict.md`              | `"The request conflicts with the current state."` |
| `Failure`   | 500 | `"Internal Server Error"` | `internal-server-error.md` | `"An unexpected error occurred."` |
| `Undefined` (default arm) | 500 | `"Internal Server Error"` | `internal-server-error.md` | `"An unexpected error occurred."` |
| `Successful` | — | — | **not handled here** (success path is untouched — D6) | — |

`detail` when messages exist = the joined `Message` strings (D5). Five doc files back the five
distinct `type` URIs (`Failure`/`Undefined` share `internal-server-error.md`).

### D4 — Forcing a future status to update both mappers (technical Q10)
Because there is now **one** `switch` (D3) instead of two, "update both mappers" collapses to
"update one factory". To make a *missing* status a hard failure rather than a silent
500-default, the factory's `switch` handles `Successful` explicitly (throwing
`InvalidOperationException` — success is not an error body) and keeps the `_ =>` arm mapped to
the 500/Undefined shape. To surface a newly-added enum value at build time, back the factory
with a unit test in the shared test project (Slice 4) that iterates **every** `Enum.GetValues<ResultStatus>()`
and asserts the factory produces the table's row — a new uncovered value fails that test.
Rejected as the *primary* guard: relying on switch-expression exhaustiveness (CS8509) — the
existing code carries a `_ =>` default arm, which suppresses the warning; removing the default
arm to force exhaustiveness would make `Undefined`/unmapped values a compile problem but also
lose the deliberate 500 fallback. Net: **compile-safe default + enumerate-all test** is the
enforcement, documented in CLAUDE.md's "Adding a new `ResultStatus`" checklist.

### D5 — 403 body + default `detail` wording (PQ5)
`Forbidden` stops using `Results.Forbid()` / `new ForbidResult()` and instead returns the
envelope as a 403 JSON result, `messages[]` populated from logged `ForbiddenMessage`s (empty
list if none — PQ5). `detail` rule (applied uniformly to all error statuses): when messages
exist, `detail` = the joined `Message` strings (reusing today's `JoinMessages` `"; "` semantics);
when the list is empty, `detail` = a per-status default constant. For 403 the default is a
**generic** `"Access denied."` (NOT echoing any handler internal), which contains the mild
info-disclosure concern PQ5 flagged: only messages the handler *chose* to log via
`ForbiddenMessage` are surfaced; nothing is invented. Per **OQ3**, every error status carries a
generic default `detail` when its message list is empty (the full per-status default table is in
D3), so `application/problem+json` consumers always receive a human-readable `detail` — `detail`
is therefore always non-null (D1).

### D6 — Success path (200) is explicitly unchanged (technical Q7, Q12)
`Successful` (void) stays `Results.Ok()` / `new OkResult()`; `Successful<T>` stays
`Results.Ok(result.Response)` / `new OkObjectResult(result.Response)`; `ToTypedActionResult`'s
`Successful` arm stays `result.Response!`. The `!` nullable-suppression on that arm is **out of
scope** (Q12) — it is on the success branch, which the envelope never touches. Only the
non-success arms are rewritten to call the factory (D3). No success body is wrapped in an envelope.

### D7 — Content type `application/problem+json`, set at the mapper boundary (PQ2)
The DTO is ASP.NET-free, so the `application/problem+json` content type is applied by each
**mapper**, not the POCO:
- **Http:** `Results.Json(dto, statusCode: dto.Status, contentType: "application/problem+json")`
  replaces `Results.NotFound(…)`/`BadRequest`/`Problem`/`Forbid`/`Conflict`.
- **Mvc:** `new ObjectResult(dto) { StatusCode = dto.Status, ContentTypes = { "application/problem+json" } }`
  replaces `NotFoundObjectResult`/`BadRequestObjectResult`/`ObjectResult`/`ForbidResult`/`ConflictObjectResult`.
This is the one spot where the two packages still write parallel code; the lockstep test (Slice 4)
asserts the resulting bytes + content type match.

## API surface

Public-surface additions (each breaks the build with RS0016 until declared in
`Abstractions/PublicAPI.Unshipped.txt`; each public member also needs an XML doc comment):
- `MiddleMan.Zero.Abstractions.ProblemResponse` (+ 6 property get/init lines + ctor).
- `MiddleMan.Zero.Abstractions.ErrorMessage` (+ `Message`/`Code` get/init + ctor).
- `MiddleMan.Zero.Abstractions.ProblemResponse.FromResult(ResultBase)` — the **public static**
  factory method on the DTO (OQ1b). Public because both mappers, in different assemblies, must
  call it (chosen over `internal` + `InternalsVisibleTo` to two named packages).

The two `ResultExtensions` classes gain **no new public methods** — their existing signatures
(`ToResult`/`ToActionResult`/`ToTypedActionResult`) are unchanged; only their bodies change. So
their `PublicAPI.Unshipped.txt` files are untouched. Success-path signatures are unchanged (D6).

## Vertical slices (preview)

Each slice ends in a demoable, end-to-end error body — not a horizontal layer.

- **Slice 1 — Envelope + shared factory (the contract).** `ProblemResponse`, `ErrorMessage`, the
  status→(code,title,type,detail,messages) factory in `Abstractions`, PublicAPI lines, the
  enumerate-all-statuses factory test. Demoable: unit test shows the DTO for each status.
- **Slice 2 — Http mapper on the envelope (Minimal API path).** Rewrite `ToResult`/`ToResult<T>`
  non-success arms to call the factory + emit `application/problem+json`; drop `Results.Forbid()`
  and `JoinMessages`. Update `AspNetCore.Http.Tests` (esp. the `ProblemHttpResult` Failure and
  `ForbidHttpResult` Forbidden assertions). Demoable: a Minimal-API 400/403/404/409/500 returns
  the envelope.
- **Slice 3 — Mvc mapper on the envelope (controller path).** Same rewrite for all three MVC
  methods; drop `ForbidResult`. Update `AspNetCore.Mvc.Tests`. Demoable: a controller returns
  the identical envelope.
- **Slice 4 — Lockstep proof + docs + version.** Shared cross-package test asserting byte-identical
  bodies for every status; extend `IceCreamTruck.WebApi.Tests` to assert JSON body shape end-to-end;
  **add the 5 `docs/errors/*.md` files** the `type` URIs point at (`bad-request`, `forbidden`,
  `not-found`, `conflict`, `internal-server-error` — OQ2); #7a README fixes (remove `ResultFilter`
  phantom, add `Conflict` rows, fix `Result<Order>`/.NET-Standard claims, document the envelope and
  link the error-type docs); `<Version>` → `2.0.0-rc3`; CHANGELOG breaking-change entry.

## Risks / Trade-offs

- **Breaking body-shape change.** Any consumer parsing `{ messages: MessageBase[] }` (with
  `Id`/`CorrelationId`/`CreatedAt`) breaks: those fields vanish and the wrapper gains
  `type`/`title`/`status`. Mitigated by the pre-release position and the `2.0.0-rc3` bump +
  CHANGELOG entry (PQ9). `messages` name and `message`/`code` sub-fields are retained to
  minimize churn.
- **403 info disclosure (PQ5).** Handler-authored `ForbiddenMessage` text becomes visible. This
  is opt-in per handler (empty list otherwise) and the default detail is generic (D5), but it is
  a real behavior change from today's empty 403 body.
- **`application/problem+json` is set in two places (D7).** The one remaining parallel-code spot;
  the byte-identical lockstep test (Slice 4) is the guard. If a mapper forgets the content type,
  that test fails.
- **Coverage gate (95%).** New DTO + factory + rewritten arms need tests; empty-vs-populated
  `messages`, multi-message detail joining, and the enumerate-all-statuses test are the net-new
  cases. Success-path tests are unchanged.
- **Factory exhaustiveness is test-enforced, not compile-enforced (D4).** A future status added
  without a factory arm falls through to the 500 default and is caught by the enumerate-all test,
  not by the compiler. Accepted trade-off to keep the deliberate 500 fallback for `Undefined`.

## Open questions for the human

_All resolved 2026-07-25 in the D-stage review._

- [x] **OQ1a — DTO / per-item type names.** **Answer: `ProblemResponse` (envelope) + `ErrorMessage`
  (per-item).** Applied throughout D1–D3 and the API surface.
- [x] **OQ1b — Factory visibility/shape.** **Answer: a public static method on the DTO,
  `ProblemResponse.FromResult(ResultBase)`** (chosen over a separate `ErrorResponseFactory` type
  and over `internal` + `InternalsVisibleTo`). Simplest cross-assembly call, one documented surface.
- [x] **OQ2 — `type` URI.** **Answer: in-repo, per-status doc files served as raw text from the
  default branch** — base `https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/`,
  one file per distinct status (`bad-request`, `forbidden`, `not-found`, `conflict`,
  `internal-server-error`). This change **creates** those 5 files (Slice 4). Consequence: `type` is
  always set → non-null (D1).
- [x] **OQ3 — `detail` on empty-message statuses.** **Answer: generic default `detail` per status**
  (table in D3), never omitted, so problem+json consumers always get a human-readable `detail`.
  Consequence: `detail` is always non-null (D1).
