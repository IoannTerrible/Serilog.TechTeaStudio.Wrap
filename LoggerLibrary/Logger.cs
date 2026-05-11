using Serilog.Events;

namespace LoggerLibrary;

/// <summary>Convenience wrapper around an injected Serilog <see cref="Serilog.ILogger"/>.</summary>
public sealed class Logger
{
    private readonly Serilog.ILogger _logger;

    /// <summary>Initializes a new instance of the <see cref="Logger"/> class.</summary>
    /// <param name="logger">The Serilog logger instance to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public Logger(Serilog.ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Logs a message at the specified level.</summary>
    public void LogEvent(LogEventLevel level, string message, Exception? ex = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
        }

        if (ex is null)
        {
            _logger.Write(level, message);
        }
        else
        {
            _logger.Write(level, ex, message);
        }
    }

    /// <summary>Logs a structured message at the specified level.</summary>
    public void LogEvent(LogEventLevel level, string messageTemplate, params object?[] propertyValues)
    {
        if (string.IsNullOrWhiteSpace(messageTemplate))
        {
            throw new ArgumentException("Message template cannot be null or whitespace.", nameof(messageTemplate));
        }

        _logger.Write(level, messageTemplate, propertyValues);
    }

    /// <summary>Logs a structured message with an exception.</summary>
    public void LogEvent(LogEventLevel level, Exception ex, string messageTemplate, params object?[] propertyValues)
    {
        ArgumentNullException.ThrowIfNull(ex);

        if (string.IsNullOrWhiteSpace(messageTemplate))
        {
            throw new ArgumentException("Message template cannot be null or whitespace.", nameof(messageTemplate));
        }

        _logger.Write(level, ex, messageTemplate, propertyValues);
    }

    /// <summary>Returns a new logger enriched with the specified property.</summary>
    public Serilog.ILogger ForContext(string propertyName, object? value, bool destructureObjects = false) =>
        _logger.ForContext(propertyName, value, destructureObjects);

    /// <summary>Shuts down the global Serilog pipeline, flushing all pending log entries. Call once on application exit.</summary>
    public static void Shutdown() => Serilog.Log.CloseAndFlush();

    /// <summary>Asynchronously shuts down the global Serilog pipeline, flushing all pending log entries.</summary>
    public static Task ShutdownAsync() => Serilog.Log.CloseAndFlushAsync().AsTask();
}
