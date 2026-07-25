# Publishing to NuGet

This guide explains how to publish MiddleMan.Zero packages to NuGet.org.

## Prerequisites

1. **NuGet API Key**: Get your API key from [nuget.org](https://www.nuget.org/account/apikeys)
2. **GitHub Repository Secrets**: Add your NuGet API key as a secret named `NUGET_API_KEY`

## Setting Up GitHub Secrets

1. Go to your repository on GitHub
2. Navigate to Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Name: `NUGET_API_KEY`
5. Value: Your NuGet.org API key
6. Click "Add secret"

## Publishing Process

### Automated Publishing (Recommended)

The repository includes GitHub Actions workflows that automatically create version tags and publish packages when you push version changes to main.

#### Step 1: Update Version Number

Edit `src/Directory.Build.props`:

```xml
<Version>0.2.0</Version>  <!-- Update this -->
```

#### Step 2: Update CHANGELOG.md

Add your changes to the changelog following the [Keep a Changelog](https://keepachangelog.com/) format.

#### Step 3: Commit and Push

```bash
git add .
git commit -m "Release v0.2.0"
git push origin main
```

**That's it!** The automation will:
1. **CI Workflow** detects the version change and automatically creates tag `v0.2.0`
2. **Publish Workflow** is triggered by the new tag and:
   - Builds all projects
   - Runs all tests
   - Creates NuGet packages
   - Publishes to NuGet.org
   - Publishes to GitHub Packages
   - Creates a GitHub Release with the packages attached

### Manual Publishing

If you need to publish manually:

#### Step 1: Build and Pack

```bash
# Clean previous builds
dotnet clean

# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release --no-build

# Create packages
dotnet pack --configuration Release --no-build --output ./artifacts
```

#### Step 2: Publish to NuGet.org

```bash
# Set your API key (only needed once)
dotnet nuget push ./artifacts/*.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate
```

## Public API surface & the CD compat gate

Two mechanisms guard the public API of the five packages. They are complementary and
both run inside the normal build/pack, so CI and the publish workflow already enforce them.

| Guard | What it checks | Fires as | When |
| --- | --- | --- | --- |
| **PublicAPI ledger** (`Microsoft.CodeAnalysis.PublicApiAnalyzers`) | *Source* surface, including nullability annotations. Every public member must be listed in a `PublicAPI.*.txt`. | `RS0016` (undeclared add) / `RS0017` (declared member removed) — build **errors** (warnings-as-errors) | every `dotnet build` (local + CI) |
| **Package Validation** (SDK `ApiCompat`) | *Binary* compat vs. the last published package, plus cross-TFM surface consistency. | `CP****`/`PKV****` — `dotnet pack` **errors** | every `dotnet pack` (local + publish workflow) |

Boundary to keep in mind: nullability-only or `set`→`init` changes are **binary-compatible**, so
Package Validation passes them — but they *are* source changes, so the PublicAPI ledger catches
them. Neither detects *behavioral* breaks (same signature, changed semantics).

### The `PublicAPI.{Shipped,Unshipped}.txt` files

One pair lives next to each `src/*` project. `Shipped.txt` = the surface of the **last stable
release**; `Unshipped.txt` = everything added/removed since. A new public member appends a line
to `Unshipped.txt` (or the build breaks); a removed member is recorded as a `*REMOVED*<sig>` line.

Currently `Shipped.txt` is **empty on purpose** — v2.0.0 has not shipped yet, so the entire
current surface sits in `Unshipped.txt`. It is frozen into `Shipped.txt` at the v2.0.0 release
(next step).

### Per-release step: "mark shipped"

When you cut **any stable release**, fold `Unshipped` into `Shipped` and clear it, in the same
commit as the version bump. This snapshots "this is the surface we just shipped":

```bash
# From the repo root. Applies *REMOVED* deletions, promotes additions, clears Unshipped.
python3 - <<'PY'
import pathlib
for d in pathlib.Path("src").iterdir():
    if not d.is_dir(): continue
    sh, un = d/"PublicAPI.Shipped.txt", d/"PublicAPI.Unshipped.txt"
    if not un.exists(): continue
    H="#nullable enable"
    shipped = {l for l in sh.read_text().splitlines() if l.strip() and l.strip()!=H} if sh.exists() else set()
    for l in (x for x in un.read_text().splitlines() if x.strip() and x.strip()!=H):
        if l.startswith("*REMOVED*"): shipped.discard(l[len("*REMOVED*"):])
        else: shipped.add(l)
    sh.write_text(H+"\n"+"\n".join(sorted(shipped))+("\n" if shipped else ""))
    un.write_text(H+"\n")
PY
```

### One-time step at the v2.0.0 release: turn on the baseline gate

Package Validation's baseline diff is intentionally **off** until 2.0.0 is on NuGet (v2
deliberately breaks vs 1.2.0, and the baseline package must already be published). Right after
2.0.0 publishes, uncomment and set the baseline in `src/Directory.Build.props`:

```xml
<PackageValidationBaselineVersion>2.0.0</PackageValidationBaselineVersion>
```

From then on, `dotnet pack` **fails** on any binary-breaking change vs. 2.0.0 — so a breaking
change can never ship as a 2.0.x patch or 2.1.x minor. **Bump the baseline only when you
intentionally release a new major** (e.g. to `3.0.0` when you cut 3.0.0).

### Shipping an *intentional* breaking change

1. Bump `<Version>` to a new **major**.
2. Record removals/signature changes as `*REMOVED*` lines in the relevant `Unshipped.txt`
   (the analyzer's code fix does this; or `dotnet format analyzers --diagnostics RS0016 RS0017`).
3. After release, raise `PackageValidationBaselineVersion` to the new major.

## Package Details

The following packages will be published:

1. **MiddleMan.Zero.Abstractions** - Core interfaces and base types
2. **MiddleMan.Zero** - Core implementation
3. **MiddleMan.Zero.DependencyInjection** - DI extensions
4. **MiddleMan.Zero.AspNetCore.Mvc** - ASP.NET Core MVC integration
5. **MiddleMan.Zero.AspNetCore.Http** - ASP.NET Core Minimal API integration

Each package includes:
- Multi-targeting: net8.0, net9.0, net10.0
- XML documentation
- Symbol packages (.snupkg) for debugging
- README.md
- Package icon
- SourceLink integration

## Verification

After publishing, verify your packages:

1. Visit https://www.nuget.org/packages/MiddleMan.Zero
2. Check that all 5 packages are published
3. Verify the version number
4. Confirm the icon and README display correctly

## Troubleshooting

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build --configuration Release
```

### Symbol Package Issues

Symbol packages (.snupkg) are automatically created and include source code for debugging. They're pushed separately to the symbol server.

### Version Conflicts

If you get a version conflict error:
- Ensure all package versions match in `src/Directory.Build.props`
- Update the `<Version>` tag to a new version number
- Packages cannot be overwritten once published

## Best Practices

1. **Always update CHANGELOG.md** before releasing
2. **Run tests** before publishing: `dotnet test --configuration Release`
3. **Use semantic versioning**: MAJOR.MINOR.PATCH
4. **Tag releases** in git for traceability
5. **Test locally** before pushing to NuGet:
   ```bash
   # Create a local NuGet feed
   dotnet pack --output ./local-feed
   
   # Test installation
   dotnet new console -n TestProject
   cd TestProject
   dotnet add package MiddleMan.Zero --source ../local-feed
   ```

## Package Release Checklist

- [ ] Update version in `src/Directory.Build.props`
- [ ] Update `CHANGELOG.md` with changes
- [ ] **Mark shipped**: fold `Unshipped` → `Shipped` (see "Per-release step" above), same commit
- [ ] Run all tests locally: `dotnet test --configuration Release`
- [ ] Commit and push changes to main
- [ ] Wait for CI workflow to auto-create tag
- [ ] Wait for Publish workflow to complete
- [ ] Verify packages on NuGet.org
- [ ] Update GitHub Release notes if needed
- [ ] **At the 2.0.0 release only**: set `PackageValidationBaselineVersion` to `2.0.0` (turns on the binary CD gate for 2.0.x/2.1.x)
