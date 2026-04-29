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
- [ ] Run all tests locally: `dotnet test --configuration Release`
- [ ] Commit and push changes to main
- [ ] Wait for CI workflow to auto-create tag
- [ ] Wait for Publish workflow to complete
- [ ] Verify packages on NuGet.org
- [ ] Update GitHub Release notes if needed
