# Serilog.TechTeaStudio.Wrap

NuGet library: Serilog wrapper with DI integration for .NET apps.
Package: `Serilog.TechTeaStudio.Wrap`. Publisher: Tech Tea Studio.
Pushed automatically on push to `main`.

## Structure

Solution: `Serilog.TechTeaStudio.Wrap.sln`
Package: `LoggerLibrary/LoggerLibrary.csproj`
Targets: `net8.0;net9.0;net10.0`

## Build & Test

```bash
dotnet build Serilog.TechTeaStudio.Wrap.sln
dotnet test Serilog.TechTeaStudio.Wrap.sln
```

## Release flow

1. Bump `<Version>` in `LoggerLibrary/LoggerLibrary.csproj`
2. Commit and push to `main`
3. CI builds, packs, and pushes to nuget.org automatically (`--skip-duplicate` is set)

**Never push to nuget.org manually.**

## Commit Convention

Format: `vX.Y.Z <description>` — see global CLAUDE.md.
**Update `<Version>` in `LoggerLibrary.csproj` before committing.**
