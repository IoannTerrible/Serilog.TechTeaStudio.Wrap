# Serilog.TechTeaStudio.Wrap

A thin Serilog wrapper with DI integration for .NET 8 / 9 / 10. Configure file-based logging with one line of startup code and get a level-per-file layout out of the box.

[![NuGet](https://img.shields.io/nuget/v/Serilog.TechTeaStudio.Wrap.svg)](https://www.nuget.org/packages/Serilog.TechTeaStudio.Wrap)

## Install

```bash
dotnet add package Serilog.TechTeaStudio.Wrap
```

## Quick start

```csharp
using LoggerLibrary;
using Serilog.Events;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogger(options =>
{
    options.LogDirectory   = "logs";
    options.MinimumLevel   = LogEventLevel.Information;
    options.RollingInterval = RollingInterval.Day;
});

using var host = builder.Build();

var logger = host.Services.GetRequiredService<Logger>();
logger.LogEvent(LogEventLevel.Information, "App started with {User}", Environment.UserName);
```

## What you get

- **One file per level** — `informationLog.txt`, `warningLog.txt`, `errorLog.txt`, `fatalLog.txt` (configurable).
- **Daily rolling** by default.
- **Structured logging** — pass property values, Serilog renders them.
- **Auto `AssemblyVersion` enrichment** on `Debug` entries.
- **DI-first** — `Logger` is registered as a singleton.

## Configuration

`LoggerOptions`:

| Property                 | Default                                                                  | Notes                                                                    |
|--------------------------|--------------------------------------------------------------------------|--------------------------------------------------------------------------|
| `LogDirectory`           | `"logs"`                                                                 | Created if missing.                                                      |
| `SeparateFilesPerLevel`  | `false`                                                                  | When true, writes one file per level (e.g. `informationLog.txt`).         |
| `LogFileName`            | `"log.txt"`                                                              | Used in single-file mode.                                                |
| `LogEventLevels`         | `Information, Warning, Error, Fatal`                                     | Levels emitted when `SeparateFilesPerLevel` is true.                     |
| `RollingInterval`        | `RollingInterval.Day`                                                    | Serilog rolling interval (`Infinite`, `Year`, `Month`, `Day`, `Hour`, `Minute`). |
| `FileSizeLimitBytes`     | `1 GB`                                                                   | Max bytes per file. `null` = unlimited.                                  |
| `RollOnFileSizeLimit`    | `false`                                                                  | When true, a new file is started once the size limit is hit.             |
| `RetainedFileCountLimit` | `31`                                                                     | Max number of rolled files to keep. `null` = unlimited.                  |
| `OutputTemplate`         | `"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine}{Exception}"` | Standard Serilog template. Ignored when `UseJsonFormatter` is true.      |
| `UseJsonFormatter`       | `false`                                                                  | Switch built-in sinks to `CompactJsonFormatter` for log aggregators.     |
| `EnableConsole`          | `true`                                                                   | Also writes to console.                                                  |
| `MinimumLevel`           | `LogEventLevel.Information`                                              | Global minimum.                                                          |
| `ConfigureLogger`        | `null`                                                                   | Post-configure `Action<LoggerConfiguration>` for extra sinks/enrichers.  |

### Size-based rolling

Serilog's file sink silently stops writing when `FileSizeLimitBytes` is reached unless `RollOnFileSizeLimit` is true. To cap each file at 50 MB and roll instead of dropping:

```csharp
services.AddLogger(o =>
{
    o.FileSizeLimitBytes  = 50L * 1024 * 1024;
    o.RollOnFileSizeLimit = true;
});
```

Time-based and size-based rolling combine — set `RollingInterval = RollingInterval.Infinite` if you only want size-based.

### JSON output

For shipping to Seq, Elastic, Loki, etc., switch built-in sinks to compact JSON:

```csharp
services.AddLogger(o => o.UseJsonFormatter = true);
```

Each event becomes one `CompactJsonFormatter` line (`{"@t":"…","@mt":"…","User":"tester"}`). `OutputTemplate` is ignored in this mode.

### Adding more sinks

Use `ConfigureLogger` to attach any sink the wrapper does not expose directly — runs after the built-in console/file sinks, before `CreateLogger()`:

```csharp
services.AddLogger(o =>
{
    o.UseJsonFormatter = true;
    o.ConfigureLogger  = lc => lc
        .WriteTo.Seq("http://localhost:5341")
        .Enrich.WithMachineName();
});
```

Same callback works with the `IConfiguration` overload via `postConfigure`:

```csharp
services.AddLogger(builder.Configuration.GetSection("Logging:File"),
    o => o.ConfigureLogger = lc => lc.WriteTo.Seq("http://localhost:5341"));
```

## API

```csharp
logger.LogEvent(LogEventLevel.Information, "User {UserId} logged in", userId);
logger.LogEvent(LogEventLevel.Error, exception, "Failed to process {OrderId}", orderId);

// At app shutdown — flush and close the global Serilog pipeline.
Logger.Shutdown();             // synchronous
await Logger.ShutdownAsync();  // async
```

## Build & test

```bash
dotnet build Serilog.TechTeaStudio.Wrap.sln
dotnet test  Serilog.TechTeaStudio.Wrap.sln
```

## Release

Bump `<Version>` in `LoggerLibrary/LoggerLibrary.csproj`, commit, push to `main`. CI builds, packs, and publishes to nuget.org automatically.

## Licenses

This project is distributed under the Apache License 2.0 (see `LICENSE.txt`).
It depends on [Serilog](https://serilog.net/), also licensed under Apache 2.0.
