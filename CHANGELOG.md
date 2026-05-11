# Changelog

All notable changes to this package are documented here.
Format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] — 2026-05-11

Major refactor. Public API is mostly backward compatible at the call-site level (`AddLogger` + `Logger.LogEvent`), but a handful of breaking changes apply — see below.

### Added
- **`Microsoft.Extensions.Logging` integration.** `ILogger<T>` is now resolvable from DI and writes through Serilog. No extra setup required.
- **`IConfiguration` overload.** `services.AddLogger(configuration)` binds `LoggerOptions` from `appsettings.json`. Optional `postConfigure` callback for overrides.
- **Console sink** (`LoggerOptions.EnableConsole`, on by default).
- **Single-file mode.** New default: one rolling file (`log.txt`) instead of per-level files. Enable the old layout with `LoggerOptions.SeparateFilesPerLevel = true`.
- **`LoggerOptions.RetainedFileCountLimit`** (default `31`) — bounds disk usage; previously unlimited.
- **`LoggerOptions.LogFileName`** for single-file mode.
- **Global `AssemblyVersion` enricher** — version is attached to every event (previously only Debug), resolved once and cached.
- **`Logger.ForContext`** passthrough for ad-hoc enrichment.
- **`LoggerLibrary.Tests`** xUnit project covering options validation, DI registration, configuration binding, and end-to-end file writes.

### Changed
- **No more global `Log.Logger` mutation.** `LoggerConfig.Build(options)` returns a `Serilog.ILogger` and DI registers it as a singleton. Safe to use multiple times and in parallel tests.
- **`Logger` is now `sealed`** and depends only on `Serilog.ILogger` (the unused `ILoggerConfig` field is gone).
- **`Flush` → `Shutdown` / `ShutdownAsync`.** The old name implied a non-destructive flush; the call actually closes the pipeline. `ShutdownAsync` now uses `Log.CloseAndFlushAsync()` (no `Task.Run` wrapper).
- **Invariant culture** for generated filenames (`ToLowerInvariant`) — fixes locale-specific filename corruption (e.g. Turkish `I → ı`).
- **Output template** uses `[{Level:u3}]` for fixed-width level rendering.
- **Default `MinimumLevel`** lowered noise: `Verbose` → `Information`.
- **Stable dependencies pinned**: `Serilog 4.2.0`, `Serilog.Sinks.File 6.0.0`, `Serilog.Sinks.Console 6.0.0`, `Serilog.Extensions.Logging 9.0.0`. No more `-dev-*` prereleases in the dependency graph.
- **XML documentation** is now packed into the `.nupkg` (`GenerateDocumentationFile=true`).
- Solution renamed `Serilog.TeachTeaStudio.Wrap.sln` → `Serilog.TechTeaStudio.Wrap.sln` (typo fix).

### Deprecated
- **`ILoggerConfig.Configure(options)`** — marked `[Obsolete]`. Use `Build(options)` and inject the returned `ILogger`. The legacy method still works and sets `Log.Logger` for compatibility.

### Removed
- **Per-level file output is no longer the default.** Set `SeparateFilesPerLevel = true` to restore.
- **`Logger.Flush()` / `Logger.FlushAsync()`** as instance methods → replaced by **static** `Logger.Shutdown()` / `Logger.ShutdownAsync()`.
- **Two-arg `Logger` constructor** `(ILoggerConfig, ILogger)` removed; the single-arg `(ILogger)` constructor is the only one now.

### Fixed
- Eliminated unbounded log growth (`retainedFileCountLimit: null` → configurable default 31).
- Per-event reflection lookup of `AssemblyVersion` replaced with a one-time cached read.

### Migration notes (from 0.1.x)

```diff
- logger.Flush();
+ Logger.Shutdown();

- logger.FlushAsync();
+ Logger.ShutdownAsync();
```

If you depended on per-level files:

```csharp
services.AddLogger(o => o.SeparateFilesPerLevel = true);
```

If you bind from configuration:

```csharp
services.AddLogger(builder.Configuration.GetSection("Logging:File"));
```

---

## [0.1.6] — Previous release

- Migrated target frameworks to `net8.0;net9.0;net10.0`.
- Switched CI to the shared TechTeaStudio NuGet publish workflow.
- Dependency bumps and minor bug fixes.

## [0.1.0] — Initial release

- Initial Serilog wrapper with DI integration, file sinks, and per-level file output.

[0.2.0]: https://github.com/IoannTerrible/Serilog.TechTeaStudio.Wrap/releases/tag/v0.2.0
[0.1.6]: https://github.com/IoannTerrible/Serilog.TechTeaStudio.Wrap/releases/tag/v0.1.6
[0.1.0]: https://github.com/IoannTerrible/Serilog.TechTeaStudio.Wrap/releases/tag/v0.1.0
