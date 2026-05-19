using Serilog;
using Serilog.Events;

namespace LoggerLibrary;

/// <summary>Configuration options for the logger.</summary>
public class LoggerOptions
{
    /// <summary>Directory where log files are written. Created if missing. Defaults to "logs".</summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>
    /// When true, writes one file per level using <see cref="LogEventLevels"/> (e.g. <c>informationLog.txt</c>).
    /// When false, writes a single rolling file named <see cref="LogFileName"/>. Defaults to false.
    /// </summary>
    public bool SeparateFilesPerLevel { get; set; } = false;

    /// <summary>Filename for single-file mode. Ignored when <see cref="SeparateFilesPerLevel"/> is true. Defaults to "log.txt".</summary>
    public string LogFileName { get; set; } = "log.txt";

    /// <summary>Levels to emit separate files for when <see cref="SeparateFilesPerLevel"/> is true.</summary>
    public LogEventLevel[] LogEventLevels { get; set; } = new[]
    {
        LogEventLevel.Information,
        LogEventLevel.Warning,
        LogEventLevel.Error,
        LogEventLevel.Fatal
    };

    /// <summary>Rolling interval for log files. Defaults to <see cref="RollingInterval.Day"/>.</summary>
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    /// <summary>
    /// Maximum size of a single log file in bytes. Null means unlimited.
    /// Defaults to 1 GB (matches Serilog's default). When the limit is reached and
    /// <see cref="RollOnFileSizeLimit"/> is false, the sink silently stops writing —
    /// set <see cref="RollOnFileSizeLimit"/> to true to roll instead.
    /// </summary>
    public long? FileSizeLimitBytes { get; set; } = 1L * 1024 * 1024 * 1024;

    /// <summary>
    /// When true, a new file is started once <see cref="FileSizeLimitBytes"/> is reached
    /// (independent of <see cref="RollingInterval"/>). Defaults to false to match Serilog's default.
    /// </summary>
    public bool RollOnFileSizeLimit { get; set; } = false;

    /// <summary>Output template for log entries. Ignored when <see cref="UseJsonFormatter"/> is true.</summary>
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine}{Exception}";

    /// <summary>
    /// When true, built-in file and console sinks emit one JSON document per event via
    /// <c>Serilog.Formatting.Compact.CompactJsonFormatter</c>. Ideal for shipping to log aggregators
    /// (Seq, Elastic, Loki). When true, <see cref="OutputTemplate"/> is ignored. Defaults to false.
    /// </summary>
    public bool UseJsonFormatter { get; set; } = false;

    /// <summary>Maximum number of rolled files to keep. Null means unlimited. Defaults to 31.</summary>
    public int? RetainedFileCountLimit { get; set; } = 31;

    /// <summary>Whether to also write logs to the console. Defaults to true.</summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>Minimum level emitted by the logger. Defaults to <see cref="LogEventLevel.Information"/>.</summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Optional post-configure callback invoked after the built-in console/file sinks are wired,
    /// but before <c>CreateLogger()</c>. Use it to attach additional sinks (Seq, Elastic, Application Insights),
    /// extra enrichers, or any other <see cref="Serilog.LoggerConfiguration"/> customization the wrapper does not expose directly.
    /// Cannot be bound from <c>IConfiguration</c> — set it in code via the
    /// <c>postConfigure</c> callback of <c>AddLogger(IConfiguration, ...)</c>.
    /// </summary>
    public Action<LoggerConfiguration>? ConfigureLogger { get; set; }
}
