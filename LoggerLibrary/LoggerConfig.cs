using System.Globalization;
using System.Reflection;
using Serilog;
using Serilog.Configuration;
using Serilog.Formatting;
using Serilog.Formatting.Compact;

namespace LoggerLibrary;

/// <summary>Default implementation of <see cref="ILoggerConfig"/> that builds a Serilog logger with file and console sinks.</summary>
public class LoggerConfig : ILoggerConfig
{
    private static readonly string? AssemblyVersion = ResolveAssemblyVersion();

    /// <summary>Builds a Serilog <see cref="Serilog.ILogger"/> from <paramref name="options"/>. Does not mutate <see cref="Log.Logger"/>.</summary>
    /// <param name="options">The logger configuration options.</param>
    /// <returns>A configured <see cref="Serilog.ILogger"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options contain invalid values.</exception>
    public Serilog.ILogger Build(LoggerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.LogDirectory))
        {
            throw new ArgumentException("LogDirectory cannot be null or whitespace.", nameof(options));
        }

        if (options.SeparateFilesPerLevel && (options.LogEventLevels == null || options.LogEventLevels.Length == 0))
        {
            throw new ArgumentException("LogEventLevels cannot be null or empty when SeparateFilesPerLevel is true.", nameof(options));
        }

        if (!options.SeparateFilesPerLevel && string.IsNullOrWhiteSpace(options.LogFileName))
        {
            throw new ArgumentException("LogFileName cannot be null or whitespace when SeparateFilesPerLevel is false.", nameof(options));
        }

        if (options.FileSizeLimitBytes is <= 0)
        {
            throw new ArgumentException("FileSizeLimitBytes must be positive or null.", nameof(options));
        }

        Directory.CreateDirectory(options.LogDirectory);

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(options.MinimumLevel)
            .Enrich.FromLogContext();

        if (!string.IsNullOrEmpty(AssemblyVersion))
        {
            loggerConfig = loggerConfig.Enrich.WithProperty("AssemblyVersion", AssemblyVersion);
        }

        ITextFormatter? jsonFormatter = options.UseJsonFormatter ? new CompactJsonFormatter() : null;

        if (options.EnableConsole)
        {
            loggerConfig = jsonFormatter is not null
                ? loggerConfig.WriteTo.Console(jsonFormatter)
                : loggerConfig.WriteTo.Console(outputTemplate: options.OutputTemplate);
        }

        void AttachFile(LoggerSinkConfiguration sink, string path)
        {
            if (jsonFormatter is not null)
            {
                sink.File(
                    jsonFormatter,
                    path,
                    rollingInterval: options.RollingInterval,
                    retainedFileCountLimit: options.RetainedFileCountLimit,
                    fileSizeLimitBytes: options.FileSizeLimitBytes,
                    rollOnFileSizeLimit: options.RollOnFileSizeLimit);
            }
            else
            {
                sink.File(
                    path,
                    rollingInterval: options.RollingInterval,
                    outputTemplate: options.OutputTemplate,
                    retainedFileCountLimit: options.RetainedFileCountLimit,
                    fileSizeLimitBytes: options.FileSizeLimitBytes,
                    rollOnFileSizeLimit: options.RollOnFileSizeLimit);
            }
        }

        if (options.SeparateFilesPerLevel)
        {
            foreach (var level in options.LogEventLevels)
            {
                var fileName = $"{level.ToString().ToLowerInvariant()}Log.txt";
                var path = Path.Combine(options.LogDirectory, fileName);
                var captured = level;
                loggerConfig.WriteTo.Logger(lc =>
                {
                    lc.Filter.ByIncludingOnly(evt => evt.Level == captured);
                    AttachFile(lc.WriteTo, path);
                });
            }
        }
        else
        {
            var path = Path.Combine(options.LogDirectory, options.LogFileName);
            AttachFile(loggerConfig.WriteTo, path);
        }

        options.ConfigureLogger?.Invoke(loggerConfig);

        return loggerConfig.CreateLogger();
    }

    /// <inheritdoc />
    [Obsolete("Use Build(LoggerOptions) and inject the returned ILogger via DI instead of mutating the global Log.Logger.")]
    public void Configure(LoggerOptions options)
    {
        Log.Logger = Build(options);
    }

    private static string? ResolveAssemblyVersion()
    {
        try
        {
            return Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        }
        catch
        {
            return null;
        }
    }
}
