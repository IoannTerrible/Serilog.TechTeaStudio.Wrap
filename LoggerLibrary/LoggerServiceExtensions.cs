using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace LoggerLibrary;

/// <summary>DI extensions for registering the logger.</summary>
public static class LoggerServiceExtensions
{
    /// <summary>Registers the logger and Microsoft.Extensions.Logging integration.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional callback to mutate <see cref="LoggerOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddLogger(this IServiceCollection services, Action<LoggerOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new LoggerOptions();
        configureOptions?.Invoke(options);
        return AddLoggerCore(services, options);
    }

    /// <summary>Registers the logger using settings bound from <paramref name="configuration"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration section containing <see cref="LoggerOptions"/> values.</param>
    /// <param name="postConfigure">Optional callback to override bound values.</param>
    public static IServiceCollection AddLogger(this IServiceCollection services, IConfiguration configuration, Action<LoggerOptions>? postConfigure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new LoggerOptions();
        configuration.Bind(options);
        postConfigure?.Invoke(options);
        return AddLoggerCore(services, options);
    }

    private static IServiceCollection AddLoggerCore(IServiceCollection services, LoggerOptions options)
    {
        ValidateOptions(options);

        var config = new LoggerConfig();
        var serilogLogger = config.Build(options);

        services.AddSingleton(options);
        services.AddSingleton<ILoggerConfig>(config);
        services.AddSingleton(serilogLogger);
        services.AddSingleton<Logger>();

        services.AddLogging(b => b.AddProvider(new SerilogLoggerProvider(serilogLogger, dispose: false)));

        return services;
    }

    private static void ValidateOptions(LoggerOptions options)
    {
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
    }
}
