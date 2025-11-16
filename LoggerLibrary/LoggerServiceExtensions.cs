using Microsoft.Extensions.DependencyInjection;

namespace LoggerLibrary;

/// <summary>Extension methods for adding logger services to the dependency injection container.</summary>
public static class LoggerServiceExtensions
{
    /// <summary>Adds logger services to the specified IServiceCollection.</summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configureOptions">Optional action to configure LoggerOptions.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddLogger(this IServiceCollection services, Action<LoggerOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new LoggerOptions();
        configureOptions?.Invoke(options);

        // Validate options
        if (string.IsNullOrWhiteSpace(options.LogDirectory))
        {
            throw new ArgumentException("LogDirectory cannot be null or whitespace.", nameof(configureOptions));
        }

        if (options.LogEventLevels == null || options.LogEventLevels.Length == 0)
        {
            throw new ArgumentException("LogEventLevels cannot be null or empty.", nameof(configureOptions));
        }

        services.AddSingleton<ILoggerConfig>(provider =>
        {
            var loggerConfig = new LoggerConfig();
            loggerConfig.Configure(options);
            return loggerConfig;
        });

        services.AddSingleton<Logger>();

        return services;
    }
}
