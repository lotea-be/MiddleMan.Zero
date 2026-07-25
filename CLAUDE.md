# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build, test, run

The solution multi-targets `net8.0;net9.0;net10.0`. `BuildInParallel` is forced off in `Directory.Build.props` — do not re-enable it; targets must build sequentially.

```bash
dotnet restore
dotnet build                                  # all TFMs
dotnet build -f net10.0                       # single TFM (CI uses this in a matrix)
dotnet test --settings coverlet.runsettings   # always pass the runsettings — see below
dotnet pack --configuration Release           # produces the 5 NuGet packages
```

Run a single test or filter:

```bash
dotnet test --filter "FullyQualifiedName~MiddleManTests"
dotnet test --filter "FullyQualifiedName~MiddleManTests.MiddleMan_HandlesRequest_Successfully"
dotnet test tests/MiddleMan.Zero.Tests/MiddleMan.Zero.Tests.csproj -f net10.0
```

Sample API (`samples/IceCreamTruck.WebApi`) can be run with `dotnet run --project samples/IceCreamTruck.WebApi -f net10.0`.

## Build is strict — assume warnings break it

`Directory.Build.props` sets `TreatWarningsAsErrors=true`, `Nullable=enable`, `EnforceCodeStyleInBuild=true`, and `GenerateDocumentationFile=true`. Practically:

- Public API additions need XML doc comments or the build fails (CS1591).
- Nullable annotations are enforced everywhere.
- EditorConfig style violations surface as errors. The `.editorconfig` enforces things like `dotnet_separate_import_directive_groups = true` and `dotnet_sort_system_directives_first = true`.
- Test projects whitelist `CS1591` via `tests/Directory.Build.props` — production projects do not.
- **Public-API surface is tracked.** `Microsoft.CodeAnalysis.PublicApiAnalyzers` breaks the build (RS0016) on any new public member until it is declared in that package's `PublicAPI.Unshipped.txt`. Keep contracts strict — `internal`/`sealed`/`init` by default, `public`/open/settable only when a consumer needs it (the `HandlerBase` templates and the mutable `HandlerContext` are the deliberate exceptions). See the stack cheatsheet's "Public-API discipline" for the full rationale.

## Coverage threshold is a hard gate

`coverlet.runsettings` sets `Threshold=95` (line coverage). CI re-checks the same 95% number using ReportGenerator on `tests/**/coverage.opencover.xml`. Both `dotnet test` locally (with the runsettings) and CI will fail if a change drops total line coverage below 95%. New production code generally needs accompanying tests.

## Versioning and release flow

Package version is centralized in `src/Directory.Build.props` (`<Version>`). All five packages share that version. Bumping it on `main` triggers two automated workflows:

1. `ci.yml` auto-creates the matching `vX.Y.Z` tag.
2. `publish-nuget.yml` (tag-triggered) builds, packs, and pushes to NuGet.org + GitHub Packages.

Update `CHANGELOG.md` in the same commit as the version bump. See `PUBLISHING.md` for details.

Central Package Management is enabled (`ManagePackageVersionsCentrally=true`). Versions live in `src/Directory.Packages.props`, `tests/Directory.Packages.props`, and `samples/Directory.Packages.props`. Project files reference packages without versions.

## Architecture: how a handler turns into an HTTP response

The repo is five thin packages that compose into a request → result → HTTP pipeline. Understand the flow before editing any one piece — a behavioral change in `HandlerBase` or `HandlerContext` ripples through every consumer.

**1. Contract (`MiddleMan.Zero.Abstractions`).** `IHandleAsync<TRequest>` and `IHandleAsync<TRequest, TResponse>` are the only interfaces consumers depend on. They return `ResultBase` / `ResultBase<TResponse>`, which carry a `ResultStatus` enum (`Successful | Failure | Invalid | NotFound | Forbidden`) and a `MessageBase[]`. `ResultBase<TResponse>` throws `ArgumentNullException` if `Successful` is paired with a null response — this invariant is load-bearing.

**2. Template (`MiddleMan.Zero.HandlerBase`).** Concrete handlers extend `HandlerBase<TRequest>` or `HandlerBase<TRequest, TResponse>` and override two methods: `ValidateAsync` and `HandleAsync`. The base class enforces a fixed pipeline:

   1. Null-check the request → log `InvalidRequestMessage("Request is null.", "middleman_request_null")`.
   2. Call `ValidateAsync`. If `context.IsRequestValid` is false, **stop** — `HandleAsync` is not invoked.
   3. Call `HandleAsync` (and capture the response in the generic variant).
   4. Synthesize a `Result` from `HandlerContext` state.

   Status precedence in `CreateResult`: `Forbidden` > `Invalid` > `Successful` > `NotFound` > `Failure`. If you add a new status, edit both `HandlerBase` overloads and both `ResultExtensions` files.

**3. State accumulator (`HandlerContext`).** Handlers don't return errors — they `context.Log(...)` typed messages, and the message *type* mutates context flags:

   - `InvalidRequestMessage` → flips `IsRequestValid=false`, `IsSuccessful=false` (causes fail-fast).
   - `FailureMessage` → flips `IsSuccessful=false`.
   - `NotFoundMessage` → flips `IsSuccessful=false`, `IsNotFound=true`.
   - `ForbiddenMessage` → flips `IsSuccessful=false`, `IsForbidden=true`.
   - `DebugMessage` → no flag change, just logged.

   A new message type means: new class in `src/MiddleMan.Zero/Messages/`, new `Log(...)` overload on `HandlerContext`, new branch in `HandlerBase.CreateResult`, and new mappings in both `ResultExtensions`.

**4. Discovery (`MiddleMan.Zero.DependencyInjection`).** `services.AddMiddleManZero()` scans **all** loaded `AppDomain` assemblies for non-abstract types implementing `IHandleAsync<>` or `IHandleAsync<,>` and registers each closed interface → handler. Default lifetime is `Transient`. Note: this means handlers must be in an assembly that is loaded by the time `AddMiddleManZero` runs — usually fine because handler libraries are referenced by the host, but worth knowing if you add lazy-loaded plugins.

**5. HTTP mapping.** `MiddleMan.Zero.AspNetCore.Mvc` (`ToActionResult`/`ToTypedActionResult`) and `MiddleMan.Zero.AspNetCore.Http` (`ToResult`) translate `ResultStatus` to MVC `IActionResult` / Minimal API `IResult`. Both packages ship the same status → HTTP code mapping (200/400/403/404/500). Keep them in sync when the enum changes.

The `samples/IceCreamTruck` and `samples/IceCreamTruck.WebApi` projects are the canonical end-to-end example — handlers live in the library, controllers/endpoints in the WebApi, and `IceCreamTruck.WebApi.Tests` runs against the real pipeline via `Microsoft.AspNetCore.Mvc.Testing`.

## Testing conventions

xUnit v3 is the runner. Test projects auto-import `Xunit` and `Shouldly` as global usings (configured in `tests/Directory.Build.props`) — don't add `using` lines for them. `FakeItEasy` is available for mocks. Tests pass `TestContext.Current.CancellationToken` rather than `default` when calling async APIs, matching xUnit v3's cancellation model.
