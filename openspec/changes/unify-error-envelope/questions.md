# Questions — unify-error-envelope

> Stage Q of QRSPI. Generated 2026-07-25.
> Change summary: Route every `ResultStatus` through one canonical, ProblemDetails-compatible error-response DTO across both `MiddleMan.Zero.AspNetCore.Http` and `MiddleMan.Zero.AspNetCore.Mvc`, kept in lockstep, and document the shape.

<!-- Surface inference (Rule B — no explicit ## Repo surface block in stack cheatsheet):
     http-api  → present (AspNetCore.Http + AspNetCore.Mvc packages)
     typed-nullable → present (C# Nullable=enable)
     data-store → absent
     ui        → absent
     auth      → absent

     Sections emitted: ## API, ## Testing, ## Sequencing & scope, ## Open product questions.
     Sections omitted: ## Data model, ## Indexing & query performance, ## Migrations & data,
                       ## UI, ## Front-end state, ## Auth & authorization.
-->

## Current-state audit (for reference)

Today's body shapes per status and package (verified against source):

| Status | Http body | Mvc body |
|--------|-----------|----------|
| Successful (void) | empty | empty |
| Successful (T) | `T` serialised | `T` serialised |
| Invalid (400) | `{ messages: MessageBase[] }` anonymous | `{ messages: MessageBase[] }` anonymous |
| NotFound (404) | `{ messages: MessageBase[] }` anonymous | `{ messages: MessageBase[] }` anonymous |
| Conflict (409) | `{ messages: MessageBase[] }` anonymous | `{ messages: MessageBase[] }` anonymous |
| Forbidden (403) | empty (`Results.Forbid()`) | empty (`ForbidResult`) |
| Failure (500) | RFC 7807 ProblemDetails via `Results.Problem(detail, 500)` | `{ messages: MessageBase[] }` anonymous via `ObjectResult` |
| Undefined (500) | RFC 7807 ProblemDetails (same) | `{ messages: MessageBase[] }` anonymous (same) |

Three divergences to close: (1) `Forbidden` has no body on either package;
(2) `Failure`/`Undefined` bodies differ between packages (ProblemDetails vs `{ messages }`);
(3) all non-success error bodies today are anonymous C# types, not a stable public DTO.

`MessageBase` currently exposes: `Id` (Guid), `CorrelationId` (Guid, fresh per message — not useful yet),
`CreatedAt` (DateTime), `Message` (string), `Code` (string).

## API

### DTO shape and placement

1. Where does the new error DTO live? Options:
   - (a) `MiddleMan.Zero.AspNetCore.Http` only, imported by `AspNetCore.Mvc` via project reference.
   - (b) `MiddleMan.Zero.AspNetCore.Mvc` only, imported by `AspNetCore.Http` via project reference.
   - (c) A new shared `MiddleMan.Zero.AspNetCore.Common` package that both depend on.
   - (d) `MiddleMan.Zero.Abstractions` (no ASP.NET Core dependency — DTO stays pure).
   - (e) Duplicated verbatim in both packages (no shared dependency, maximum independence).

2. What is the canonical JSON property set for the error envelope? Options:
   - (a) Strict RFC 7807 subset: `type`, `title`, `status`, `detail` — no extension fields.
   - (b) RFC 7807 with extensions: `type`, `title`, `status`, `detail` + `errors` array + `traceId` placeholder.
   - (c) Custom DTO that is *compatible* with ProblemDetails (same top-level fields) but adds `errors`/`messages` array and a `traceId` slot, without inheriting from `Microsoft.AspNetCore.Mvc.ProblemDetails`.
   - (d) Inherit from `Microsoft.AspNetCore.Mvc.ProblemDetails` and populate its `Extensions` dictionary.

3. What goes into the `errors` / `messages` array inside the envelope? Each element should carry:
   - (a) Only `message` (string) and `code` (string) — omit `Id`, `CorrelationId`, `CreatedAt`.
   - (b) `message`, `code`, and `correlationId` (forwarded from `MessageBase.CorrelationId`) — omit `Id` and `CreatedAt`.
   - (c) All `MessageBase` fields verbatim (`Id`, `CorrelationId`, `CreatedAt`, `Message`, `Code`).
   - (d) A separate per-error DTO defined on the envelope type, mapping only selected fields.

> ⮕ Resolved by PQ5: 403 returns the envelope populated from logged `ForbiddenMessage`s (empty `messages[]` if none) — no longer body-free. The mapper must stop using `Results.Forbid()`/`ForbidResult` and emit a 403 JSON result. And by PQ7: #3 (CancellationToken) is OUT of scope, so the 499-class note below does not apply to this change.

4. Should the `Forbidden` (403) response include a body with the new envelope (even if `errors` is empty), or remain body-free to match the HTTP-level `ForbidResult`/`Results.Forbid()` semantics? Note: if PQ3 puts #3 (CancellationToken) in scope, a 499-class cancelled-request status may also need a body decision.

> ⮕ Resolved by PQ6 + PQ10: define the slot now as a nullable **`string?`** and leave it null; #4 owns population (from `Activity.Current`, not `HttpContext.TraceIdentifier`, since the value may live on `MessageBase` in the ASP.NET-free `Abstractions` layer).

5. Should the `traceId`/`correlationId` slot on the envelope be populated now with `Activity.Current?.Id` / `HttpContext.TraceIdentifier`, or left null / omitted until backlog #4 (CorrelationId propagation) fills it? Options:
   - (a) Populate `traceId` immediately from `HttpContext.TraceIdentifier` (available at the mapper call site in Http; requires `HttpContext` injection in Mvc).
   - (b) Populate `traceId` from `Activity.Current?.Id` (W3C trace parent, no `HttpContext` needed).
   - (c) Define the slot on the DTO now (nullable string) but leave it null — the field exists so #4 can fill it without a breaking change.
   - (d) Do not add the slot in this change; revisit in #4.

6. What is the JSON property name for the array of per-error items in the envelope? Options:
   - (a) `errors` (aligns with ASP.NET Core's built-in `ValidationProblemDetails.Errors` style).
   - (b) `messages` (matches the current `{ messages: [...] }` shape — reduces consumer migration cost).
   - (c) Both: `errors` as the primary name, with `messages` as an alias via a `[JsonPropertyName]` attribute (risky: schema ambiguity).

7. Does the successful path (200 OK) change at all? Specifically: should `Successful` with data still return the raw `TResponse` body, or wrap it in an envelope too? (This change's stated goal is the *error* envelope — but the answer needs to be explicit to avoid scope creep.)

> ⮕ Resolved by PQ8: it is a documentation phantom. Fix the README to remove the `ResultFilter`/`AddMiddleManZeroResults()` reference; do NOT implement the filter in this change.

8. The `Mvc` package's README describes a `ResultFilter` and `AddMiddleManZeroResults()` that do not exist in the current source (`ResultExtensions.cs` is the only file). Is this a known documentation phantom? If the ResultFilter concept needs to be implemented as part of this change (to enable returning `Result`/`Result<T>` directly from controllers and routing through the new envelope), that substantially widens scope.

### Lockstep discipline

9. Which test layer is authoritative for proving both packages produce identical JSON for the same `ResultStatus`? Options:
   - (a) A shared test-helper method called from both `AspNetCore.Http.Tests` and `AspNetCore.Mvc.Tests`.
   - (b) A new `MiddleMan.Zero.AspNetCore.Tests.Shared` test library referenced by both.
   - (c) A cross-package integration test in `IceCreamTruck.WebApi.Tests` that calls both controller and minimal-API endpoints and compares JSON bodies.
   - (d) No shared test; each package tests its own serialisation, and a manual review checklist enforces lockstep.

10. When a future `ResultStatus` is added, where is the "new status must update both mappers" rule enforced? Options:
    - (a) Only convention (documented in CLAUDE.md / contributing guide).
    - (b) A shared constant or extension method containing the full status→(httpCode, envelopeTitle) mapping, imported by both packages, so a missing case is a compile error.
    - (c) A unit test in a shared test project that asserts every `ResultStatus` enum value is handled by both mappers.

### Nullable safety

11. The new envelope DTO will be a public type in a `Nullable=enable` assembly. Which properties must be declared nullable (`string?`) vs. non-nullable with a guaranteed non-null value at construction? Specifically: `type`, `title`, `detail`, `traceId`, and the errors array — what is the nullability contract for each?

12. The current `ToTypedActionResult<TResponse>` returns `result.Response!` on the `Successful` branch (a nullable-suppression operator `!`). Does this change touch that suppression, or is it out of scope because `Successful` returns the raw `TResponse` and not the error envelope?

## Testing

13. The current tests verify HTTP status codes and confirm the body carries a `messages` property via reflection on an anonymous type. After this change, tests will need to deserialise into the new named DTO. Should tests use:
    - (a) `System.Text.Json` deserialisation into the actual DTO type (tight coupling to the public type).
    - (b) Deserialisation into `JsonElement` / `JsonDocument` and property-name assertions (decoupled from the DTO type).
    - (c) The existing reflection-based approach extended to the new property names.

> ⮕ Resolved by PQ5: Forbidden DOES gain a body, so `ForbidHttpResult`/`ForbidResult` is replaced by a 403 JSON result carrying the envelope. Tests must assert the 403 status + envelope body (with `messages[]` from logged `ForbiddenMessage`s) rather than `ShouldBeOfType<ForbidHttpResult>()`.

14. The `Forbidden` status currently produces no body, and the test (`iResult.ShouldBeOfType<ForbidHttpResult>()`) makes no body assertion. If Forbidden gains a body, what does the new test look like — and does the `ForbidHttpResult` type even support a body in the ASP.NET Core Minimal API layer?

15. The `Failure` case in `AspNetCore.Http` currently returns a `ProblemHttpResult` (via `Results.Problem(...)`). If the envelope replaces this, the test must stop asserting `ShouldBeOfType<ProblemHttpResult>()`. What is the replacement assertion type — `Json<ErrorEnvelopeDto>`, `IValueHttpResult`, or something else?

16. The 95% coverage gate is hard. How many net-new test cases are expected? Are there scenarios missing from the current test matrix that this change must add (e.g. multiple errors in one response, an empty-messages Forbidden, Failure with messages vs. without)?

17. Should the `IceCreamTruck.WebApi.Tests` integration suite be extended to assert on the actual JSON body shape (not just status codes), making it the canonical end-to-end proof of the envelope contract? Currently those tests appear to cover status codes via `Mvc.Testing` — what is their current scope?

## Sequencing & scope

18. Backlog #3 (CancellationToken auto-bind) touches `AspNetCore.Http`. Does this change open the same file (`ResultExtensions.cs` in `AspNetCore.Http`) in a way that makes a merge conflict with #3 likely if they run in parallel, or are they sufficiently independent (different methods, no shared state)?

19. Should backlog #7a (documentation fixes for Abstractions README — related-packages list + public-surface clarification) ride in this same PR, given this change will update both `AspNetCore.Http/README.md` and `AspNetCore.Mvc/README.md` anyway? Or should #7a remain a separate, independent commit?

20. This change will produce a breaking API change for any consumer that currently deserialises the `{ messages: [...] }` body. Does the library's current semver position (`2.0.0-rc2` pre-release) mean breaking changes are acceptable without a major-version bump, or does the PR need to bump the version (e.g. to `2.1.0-rc1` or `2.0.0-rc3`)?

21. The backlog notes that #4 (CorrelationId propagation) is designed to fill the `traceId` slot added by this change without re-opening the mappers. Does that dependency assumption constrain the DTO design enough that #4 must be outlined (even sketchily) before this change's Design stage, or is a nullable `traceId` slot sufficient to keep the two changes independent?

## Open product questions (for the human)

_All answered 2026-07-25 in the Q-stage interactive pass._

- [x] **PQ1 — DTO placement:** Where should the new error envelope DTO be declared — in `AspNetCore.Http`, in `AspNetCore.Mvc`, in a new shared package, in `Abstractions`, or duplicated in both packages? This decision gates the dependency graph for the entire change. Options: (a) `AspNetCore.Http` imports by Mvc, (b) `AspNetCore.Mvc` imported by Http, (c) new `AspNetCore.Common` package, (d) `Abstractions` (no ASP.NET Core dep), (e) duplicate in both.
  **Answer: (d) `Abstractions` — a pure POCO with no ASP.NET Core dependency, referenced by both AspNetCore packages. Keeps the '.Zero minimal' promise, adds no package, avoids either AspNetCore package depending on the other. (This forecloses inheriting `Microsoft.AspNetCore.Mvc.ProblemDetails`.)**

- [x] **PQ2 — Envelope JSON shape:** Should the envelope be a strict RFC 7807 `ProblemDetails` (or inherit `Microsoft.AspNetCore.Mvc.ProblemDetails`), a custom DTO that *looks like* ProblemDetails but does not inherit from it, or something else? Options: (a) strict RFC 7807 only (`type`/`title`/`status`/`detail`), (b) RFC 7807 + `errors` array extension, (c) custom compatible DTO with `errors` + `traceId` slot, (d) inherit `Microsoft.AspNetCore.Mvc.ProblemDetails` and use `Extensions` dictionary.
  **Answer: 7807-conformant + `messages` extension. A pure POCO with the RFC 7807/9457 core fields (`type`/`title`/`status`/`detail`) served as `application/problem+json`, honoring the standard's field semantics, with the per-error `messages[]` array as a documented extension member (the `ValidationProblemDetails` pattern) plus the `traceId` slot. Chosen for max ecosystem interop over a plain lookalike POCO.**

- [x] **PQ3 — Error array field name:** Should the per-error list inside the envelope be named `errors` (aligns with ASP.NET Core `ValidationProblemDetails` convention) or `messages` (matches today's shape to reduce consumer migration cost)? Options: (a) `errors`, (b) `messages`, (c) `errors` now and provide a migration note in CHANGELOG.
  **Answer: (b) `messages` — matches today's `{ messages: [...] }` shape, lowest consumer migration cost.**

- [x] **PQ4 — Per-error fields exposed:** What fields from `MessageBase` should each error item in the envelope carry? (a) `message` + `code` only, (b) `message` + `code` + `correlationId`, (c) all `MessageBase` fields verbatim, (d) a separate per-error DTO type. Note: if PQ2 picks strict RFC 7807, the `detail` string (joined messages) replaces the array entirely.
  **Answer: (a) `message` + `code` only. Omit `Id`/`CorrelationId`/`CreatedAt` — `CorrelationId` is a meaningless fresh Guid until #4, and the request-level correlation handle lives in the envelope's `traceId`, not per message. (See PQ10 for the traceId-vs-CorrelationId distinction the human raised.)**

- [x] **PQ5 — Forbidden body:** Should the 403 Forbidden response carry the new envelope DTO in the body (even if the errors array is empty), or remain body-free to preserve `ForbidResult`/`Results.Forbid()` semantics? Options: (a) body-free (status code only, no change from today), (b) envelope body with empty errors array, (c) envelope body with a fixed "access denied" detail string.
  **Answer: Envelope body populated from logged `ForbiddenMessage`s (empty `messages[]` if none were logged). Consistent with every other status carrying its logged messages — 403 is no longer the odd-one-out. Note the mild info-disclosure consideration (handler-provided forbidden reasons become visible); the Design stage should decide default `detail` wording.**

- [x] **PQ6 — traceId slot:** Should `traceId` be populated now (from `HttpContext.TraceIdentifier` or `Activity.Current?.Id`), defined as a nullable slot but left null until backlog #4 fills it, or omitted entirely from this change? Options: (a) populate immediately from `HttpContext.TraceIdentifier`, (b) populate from `Activity.Current?.Id`, (c) define the nullable slot now and leave it null, (d) omit entirely — add in #4.
  **Answer: (c) Define the nullable slot now and leave it null — #4 owns population. Typed `string?` (see PQ10), so #4 can fill it without a breaking DTO change.**

- [x] **PQ7 — Scope: #3 and #7a bundling:** Should backlog #3 (CancellationToken auto-bind) and/or #7a (documentation fixes) be merged into this PR, or remain separate changes? Note that #3 touches `AspNetCore.Http/ResultExtensions.cs` (same file this change rewrites) and #7a touches the package READMEs (same files this change must update). Options: (a) all three in one PR, (b) this change + #7a together, #3 separate, (c) this change only — #3 and #7a remain independent, (d) this change + #3 together, #7a separate.
  **Answer: (b) This change + #7a together, #3 separate. #7a is docs on the same READMEs this change already rewrites (trivial to include); #3 (`bind-cancellation-token`) is a distinct behavioral capability run as the next sequential change. In scope for THIS change: the envelope + #7a doc fixes. Out of scope: #3.**

- [x] **PQ8 — ResultFilter scope:** The `AspNetCore.Mvc` README describes a `ResultFilter` and `AddMiddleManZeroResults()` extension that do not exist in the current source. Is implementing this filter in scope for this change (enabling controllers to return `Result<T>` directly without calling `.ToActionResult()`)? Options: (a) yes — implement filter + `AddMiddleManZeroResults()` as part of this change, (b) no — fix only the README to remove the phantom reference, (c) no — leave the README as-is and track the filter as a separate backlog item.
  **Answer: (b) Fix the README to remove the phantom `ResultFilter`/`AddMiddleManZeroResults()` reference (docs we're already updating). No new code scope. Implementing the filter is NOT in scope.**

- [x] **PQ9 — Version bump:** The library is at `2.0.0-rc2`. The new envelope DTO is a breaking JSON contract change for consumers parsing the current `{ messages: [...] }` body. Does this change require a version bump (e.g. `2.0.0-rc3` or `2.1.0-rc1`), and should the CHANGELOG document the breaking body-shape change? Options: (a) bump to `2.0.0-rc3` + CHANGELOG entry, (b) bump to `2.1.0-rc1` + CHANGELOG entry, (c) no version bump needed — pre-release consumers accept breakage, but add CHANGELOG entry, (d) no version bump and no CHANGELOG entry.
  **Answer: (a) Bump `src/Directory.Build.props` `<Version>` to `2.0.0-rc3` and add a CHANGELOG entry documenting the breaking body-shape change. (Reminder: the version bump on `main` triggers the auto-tag + NuGet publish workflows — see CLAUDE.md "Versioning and release flow".)**

- [x] **PQ10 — traceId source vs `MessageBase.CorrelationId` (emergent):** The human asked whether `MessageBase.CorrelationId` should become the trace id (from `Activity.Current` or `HttpContext.TraceIdentifier`). How much of that unification bleeds into THIS change? Options: (a) defer to #4, type the slot `string?`; (b) pull population into #2 now; (c) keep envelope `traceId` and `MessageBase.CorrelationId` as separate concepts.
  **Answer: (a) Defer population to #4; this change only types the envelope `traceId` slot as `string?` and leaves it null (per PQ6). Design constraints recorded for #4: (1) if the value is to live on `MessageBase` (in `Abstractions`), the source MUST be `Activity.Current` (`System.Diagnostics`, no ASP.NET dep) — `HttpContext.TraceIdentifier` is only reachable at the HTTP-mapper boundary and cannot be read from core; (2) unifying `CorrelationId` with a trace id means changing its type from `Guid` to `string` — a breaking `Abstractions` contract change #4 owns; (3) #4 should source both the envelope `traceId` and `MessageBase.CorrelationId` from the same `Activity` trace id, with a generated fallback for non-HTTP callers.**
