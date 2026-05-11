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

    /// <summary>Output template for log entries.</summary>
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine}{Exception}";

    /// <summary>Maximum number of rolled files to keep. Null means unlimited. Defaults to 31.</summary>
    public int? RetainedFileCountLimit { get; set; } = 31;

    /// <summary>Whether to also write logs to the console. Defaults to true.</summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>Minimum level emitted by the logger. Defaults to <see cref="LogEventLevel.Information"/>.</summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
}
