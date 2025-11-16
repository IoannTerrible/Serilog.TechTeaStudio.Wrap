using Serilog;

namespace LoggerLibrary;

/// <summary>Default implementation of ILoggerConfig that configures Serilog with file sinks.</summary>
public class LoggerConfig : ILoggerConfig
{
    /// <summary>Configures the logger with the specified options.</summary>
    /// <param name="options">The logger configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options contain invalid values.</exception>
    public void Configure(LoggerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        if (string.IsNullOrWhiteSpace(options.LogDirectory))
        {
            throw new ArgumentException("LogDirectory cannot be null or whitespace.", nameof(options));
        }
        
        if (options.LogEventLevels == null || options.LogEventLevels.Length == 0)
        {
            throw new ArgumentException("LogEventLevels cannot be null or empty.", nameof(options));
        }

        if (!Directory.Exists(options.LogDirectory))
        {
            Directory.CreateDirectory(options.LogDirectory);
            Log.Information("Created logs directory: {LogDirectory}", options.LogDirectory);
        }

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(options.MinimumLevel)
            .Enrich.FromLogContext();

        foreach (var logEventLevel in options.LogEventLevels)
        {
            string logFileName = $"{logEventLevel.ToString().ToLower()}Log.txt";
            loggerConfig.WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => evt.Level == logEventLevel)
                .WriteTo.File(
                    Path.Combine(options.LogDirectory, logFileName),
                    rollingInterval: options.RollingInterval,
                    outputTemplate: options.OutputTemplate,
                    retainedFileCountLimit: null)
            );
        }

        Log.Logger = loggerConfig.CreateLogger();
    }
}
