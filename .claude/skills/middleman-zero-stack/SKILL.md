---
name: middleman-zero-stack
description: Stack cheatsheet for MiddleMan.Zero — languages, runtime versions, frameworks, key libraries, project layout, testing, and coding conventions. This is the QRSPI stack-cheatsheet skill for this repo; load it whenever you need the project's tech stack or conventions.
---

## Languages & runtime

- **C#** with `<LangVersion>latest</LangVersion>`, `ImplicitUsings=enable`, `Nullable=enable`.
- **Multi-targeted**: `net8.0;net9.0;net10.0` (set once in the root `Directory.Build.props` `TargetFrameworks`). Every production change must compile clean on all three TFMs.
- CI builds each TFM in a separate matrix leg (`8.0.x`, `9.0.x`, `10.0.x`) with `dotnet build -f <tfm>`.

## Frameworks & key libraries

MiddleMan.Zero is a lightweight **mediator / CQRS handler** library (5 NuGet packages) plus an ASP.NET Core HTTP mapping layer. It has almost no runtime dependencies of its own.

- **Microsoft.Extensions.DependencyInjection.Abstractions** (10.0.9) — the only production dependency; `AddMiddleManZero()` registers handlers via assembly scanning.
- **Microsoft.SourceLink.GitHub** (10.0.203) — deterministic/SourceLink builds; `PrivateAssets=All`.
- **ASP.NET Core** (framework reference) — the `AspNetCore.Mvc` and `AspNetCore.Http` packages map `ResultStatus` → `IActionResult` / `IResult`.
- Test-only: **xUnit v3** (3.2.2), **Shouldly** (4.3.0), **FakeItEasy** (9.0.1), **coverlet** (10.0.0), **Microsoft.AspNetCore.Mvc.Testing** (TFM-matched: 8.0.26 / 9.0.15 / 10.0.7).

## Project layout

- `src/` — the five shipped packages:
  - `MiddleMan.Zero.Abstractions` — contract: `IHandleAsync<TRequest>` / `IHandleAsync<TRequest,TResponse>`, `ResultBase`/`ResultBase<T>`, `ResultStatus` enum, `MessageBase[]`.
  - `MiddleMan.Zero` — `HandlerBase<…>` template, `HandlerContext` state accumulator, `Messages/` typed messages.
  - `MiddleMan.Zero.DependencyInjection` — `AddMiddleManZero()` assembly scanning.
  - `MiddleMan.Zero.AspNetCore.Mvc` — `ToActionResult` / `ToTypedActionResult`.
  - `MiddleMan.Zero.AspNetCore.Http` — `ToResult` for Minimal APIs.
- `tests/` — one test project per package + `IceCreamTruck(.WebApi).Tests`.
- `samples/IceCreamTruck` (handlers) and `samples/IceCreamTruck.WebApi` (controllers/endpoints) — canonical end-to-end example; WebApi.Tests runs the real pipeline via `Mvc.Testing`.
- Build config is layered: root `Directory.Build.props` (strict flags), `src/` (package metadata, `<Version>`), `tests/` (test flags, global usings). Central Package Management via `Directory.Packages.props` at root/`src`/`tests`.

## Conventions

- **Build is strict — warnings are errors.** `TreatWarningsAsErrors=true`, `Nullable=enable`, `EnforceCodeStyleInBuild=true`, `GenerateDocumentationFile=true`.
  - Public API additions **need XML doc comments** or the build fails (CS1591). Production projects do not suppress it; test projects whitelist CS1591.
  - Nullable annotations enforced everywhere.
- **EditorConfig is enforced** (`.editorconfig`): `indent_size = 4`, `insert_final_newline = false`, `dotnet_separate_import_directive_groups = true`, `dotnet_sort_system_directives_first = true`. Style violations surface as build errors.
- **Error handling is message-driven, not exceptions.** Handlers never return errors — they `context.Log(...)` a typed message; the message *type* mutates `HandlerContext` flags. Adding a status/message type touches many files in lockstep (see Gotchas).
- `BuildInParallel=false` is deliberate — targets must build sequentially. Do not re-enable.

## Testing

- **xUnit v3** is the runner. Test projects auto-import `Xunit` and `Shouldly` as global usings (in `tests/Directory.Build.props`) — do **not** add `using Xunit;` / `using Shouldly;`.
- **FakeItEasy** for mocks; **Shouldly** for assertions.
- Pass `TestContext.Current.CancellationToken` (not `default`) to async APIs, matching xUnit v3's cancellation model.
- New production code generally needs accompanying tests — coverage is a hard gate (see below).

## Build, lint & test commands

```bash
dotnet restore
dotnet build                                  # all TFMs
dotnet build -f net10.0                        # single TFM (CI matrix style)
dotnet test --settings coverlet.runsettings    # ALWAYS pass the runsettings
dotnet pack --configuration Release             # produces the 5 NuGet packages
```

Filter a single test:

```bash
dotnet test --filter "FullyQualifiedName~MiddleManTests"
dotnet test tests/MiddleMan.Zero.Tests/MiddleMan.Zero.Tests.csproj -f net10.0
```

Run the sample API: `dotnet run --project samples/IceCreamTruck.WebApi -f net10.0`.

## PR & git workflow

- **Host:** GitHub (`git@github.com:lotea-be/MiddleMan.Zero.git`). PR CLI: **`gh`** (v2.92+ installed).
  - Create a PR: `gh pr create --base main --head features/<change-id> --title "…" --body "…"`.
  - Check merge state: `gh pr view <N> --json state,mergedAt`.
- **Source-branch naming:** `features/<change-id>` (e.g. `features/add-conflict-status`), where `<change-id>` is the kebab-case QRSPI change id.
- **Default target branch:** `main`. CI (`ci.yml`) runs on `push`/`pull_request` to `main`.
- No hard PR-description size cap, but keep it focused and reference the change folder.

## Dependency policy

- **Prefer stable releases.** Routine dependency version bumps arrive as **Dependabot** PRs — don't hand-bump unless needed for a change.
- **Central Package Management** (`ManagePackageVersionsCentrally=true`): versions live in `src/Directory.Packages.props`, `tests/Directory.Packages.props`, and root `Directory.Packages.props`; project files reference packages **without** versions.
- The library's *own* package version may be a prerelease (currently `2.0.0-rc2`, in `src/Directory.Build.props`); that is separate from the stable-only policy for *dependencies*.

## Gotchas / house rules

- **The handler pipeline is load-bearing — change it in lockstep.** A behavioral change in `HandlerBase` or `HandlerContext` ripples through every consumer.
  - Status precedence in `HandlerBase.CreateResult`: `Forbidden` > `Invalid` > `Successful` > `NotFound` > `Failure`.
  - Pipeline order: null-check request → `ValidateAsync` (if invalid, **stop** — `HandleAsync` is not called) → `HandleAsync` → synthesize `Result`.
  - `ResultBase<TResponse>` throws `ArgumentNullException` if `Successful` is paired with a null response — this invariant is load-bearing; keep it.
- **Adding a new `ResultStatus`** means editing: both `HandlerBase` overloads, both `ResultExtensions` files, and the two ASP.NET Core mappings (`Mvc` + `Http`) so their status→HTTP codes (200/400/403/404/500) stay in sync.
- **Adding a new message type** means: new class in `src/MiddleMan.Zero/Messages/`, new `Log(...)` overload on `HandlerContext`, new branch in `HandlerBase.CreateResult`, and new mappings in both `ResultExtensions`.
- **Coverage gate is hard: 95% line coverage.** `coverlet.runsettings` sets `Threshold=95`; CI re-checks via ReportGenerator on `tests/**/coverage.opencover.xml`. Both local `dotnet test --settings coverlet.runsettings` and CI fail below 95%.
- **Versioning/release:** package `<Version>` is centralized in `src/Directory.Build.props`; all 5 packages share it. Bumping it on `main` auto-tags (`ci.yml`) and publishes to NuGet + GitHub Packages (`publish-nuget.yml`). Update `CHANGELOG.md` in the **same commit** as the version bump. See `PUBLISHING.md`.
- Contributor guidance lives in `CLAUDE.md` at the repo root.
