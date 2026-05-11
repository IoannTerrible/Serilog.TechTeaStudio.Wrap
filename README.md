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

| Property              | Default                                                                  | Notes                              |
|-----------------------|--------------------------------------------------------------------------|------------------------------------|
| `LogDirectory`        | `"logs"`                                                                 | Created if missing.                |
| `LogEventLevels`      | `Information, Warning, Error, Fatal`                                     | One output file per level.         |
| `RollingInterval`     | `RollingInterval.Day`                                                    | Serilog rolling interval.          |
| `OutputTemplate`      | `"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message:lj} {NewLine}{Exception}"` | Standard Serilog template. |
| `MinimumLevel`        | `LogEventLevel.Verbose`                                                  | Global minimum.                    |

## API

```csharp
logger.LogEvent(LogEventLevel.Information, "User {UserId} logged in", userId);
logger.LogEvent(LogEventLevel.Error, exception, "Failed to process {OrderId}", orderId);
logger.Flush();         // synchronous flush + close
await logger.FlushAsync();
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
