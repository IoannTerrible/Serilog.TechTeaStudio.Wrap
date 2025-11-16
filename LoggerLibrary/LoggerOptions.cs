using Serilog;
using Serilog.Events;

namespace LoggerLibrary;

/// <summary>Configuration options for the logger.</summary>
public class LoggerOptions
{
    /// <summary>Gets or sets the directory where log files will be written. Defaults to "logs".</summary>
    public string LogDirectory { get; set; } = "logs";
    
    /// <summary>Gets or sets the log event levels to write to separate files. Defaults to Information, Warning, Error, and Fatal.</summary>
    public LogEventLevel[] LogEventLevels { get; set; } = new[]
    {
        LogEventLevel.Information,
        LogEventLevel.Warning,
        LogEventLevel.Error,
        LogEventLevel.Fatal
    };
    
    /// <summary>Gets or sets the rolling interval for log files. Defaults to Day.</summary>
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
    
    /// <summary>Gets or sets the output template for log entries.</summary>
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message:lj} {NewLine}{Exception}";
    
    /// <summary>Gets or sets whether to include full stack traces for exceptions. Defaults to false.</summary>
    public bool IncludeFullStackTraces { get; set; } = false;
    
    /// <summary>Gets or sets the minimum log level. Defaults to Verbose.</summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Verbose;
}
