using System.Reflection;
using Serilog;
using Serilog.Events;

namespace LoggerLibrary;

/// <summary>Provides logging functionality using Serilog with structured logging support.</summary>
public class Logger
{
	private readonly Serilog.ILogger _logger;
	private readonly ILoggerConfig _loggerConfig;

	/// <summary>Initializes a new instance of the Logger class.</summary>
	/// <param name="loggerConfig">The logger configuration.</param>
	/// <exception cref="ArgumentNullException">Thrown when loggerConfig is null.</exception>
	public Logger(ILoggerConfig loggerConfig)
	{
		_loggerConfig = loggerConfig ?? throw new ArgumentNullException(nameof(loggerConfig));
		_logger = Log.Logger;
	}

	/// <summary>Initializes a new instance of the Logger class with a specific Serilog logger.</summary>
	/// <param name="loggerConfig">The logger configuration.</param>
	/// <param name="logger">The Serilog logger instance to use.</param>
	/// <exception cref="ArgumentNullException">Thrown when loggerConfig or logger is null.</exception>
	public Logger(ILoggerConfig loggerConfig, Serilog.ILogger logger)
	{
		_loggerConfig = loggerConfig ?? throw new ArgumentNullException(nameof(loggerConfig));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>Logs an event at the specified level.</summary>
	/// <param name="logEventLevel">The log event level.</param>
	/// <param name="message">The message to log.</param>
	/// <param name="ex">Optional exception to include in the log.</param>
	public void LogEvent(LogEventLevel logEventLevel, string message, Exception? ex = null)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
		}

		var logger = _logger;
		
		// Add assembly version for debug level
		if (logEventLevel == LogEventLevel.Debug)
		{
			var assemblyVersion = GetAssemblyVersion();
			if (!string.IsNullOrEmpty(assemblyVersion))
			{
				logger = logger.ForContext("AssemblyVersion", assemblyVersion);
			}
		}

		if (ex != null)
		{
			logger.Write(logEventLevel, ex, message);
		}
		else
		{
			logger.Write(logEventLevel, message);
		}
	}

	/// <summary>Logs an event at the specified level with structured properties.</summary>
	/// <param name="logEventLevel">The log event level.</param>
	/// <param name="message">The message template to log.</param>
	/// <param name="propertyValues">Optional property values for structured logging.</param>
	public void LogEvent(LogEventLevel logEventLevel, string message, params object[] propertyValues)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
		}

		var logger = _logger;
		
		if (logEventLevel == LogEventLevel.Debug)
		{
			var assemblyVersion = GetAssemblyVersion();
			if (!string.IsNullOrEmpty(assemblyVersion))
			{
				logger = logger.ForContext("AssemblyVersion", assemblyVersion);
			}
		}

		logger.Write(logEventLevel, message, propertyValues);
	}

	/// <summary>Logs an event at the specified level with an exception and structured properties.</summary>
	/// <param name="logEventLevel">The log event level.</param>
	/// <param name="ex">The exception to log.</param>
	/// <param name="message">The message template to log.</param>
	/// <param name="propertyValues">Optional property values for structured logging.</param>
	public void LogEvent(LogEventLevel logEventLevel, Exception ex, string message, params object[] propertyValues)
	{
		ArgumentNullException.ThrowIfNull(ex);
		
		if (string.IsNullOrWhiteSpace(message))
		{
			throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
		}

		var logger = _logger;
		
		if (logEventLevel == LogEventLevel.Debug)
		{
			var assemblyVersion = GetAssemblyVersion();
			if (!string.IsNullOrEmpty(assemblyVersion))
			{
				logger = logger.ForContext("AssemblyVersion", assemblyVersion);
			}
		}

		logger.Write(logEventLevel, ex, message, propertyValues);
	}

	/// <summary>Flushes the logger, ensuring all pending log entries are written.</summary>
	public void Flush()
	{
		Log.CloseAndFlush();
	}

	/// <summary>Flushes the logger asynchronously, ensuring all pending log entries are written.</summary>
	/// <returns>A task representing the asynchronous flush operation.</returns>
	public Task FlushAsync()
	{
		return Task.Run(() => Log.CloseAndFlush());
	}

	private static string? GetAssemblyVersion()
	{
		try
		{
			var assembly = Assembly.GetEntryAssembly();
			return assembly?.GetName().Version?.ToString();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to get assembly version");
			return null;
		}
	}
}

