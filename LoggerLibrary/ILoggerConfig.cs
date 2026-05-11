namespace LoggerLibrary;

/// <summary>Builds Serilog loggers from <see cref="LoggerOptions"/>.</summary>
public interface ILoggerConfig
{
    /// <summary>Builds a Serilog <see cref="Serilog.ILogger"/> instance from the supplied options.</summary>
    /// <param name="options">The logger configuration options.</param>
    /// <returns>A configured logger.</returns>
    Serilog.ILogger Build(LoggerOptions options);

    /// <summary>Legacy entry point that mutates the global <see cref="Serilog.Log.Logger"/>.</summary>
    /// <param name="options">The logger configuration options.</param>
    [Obsolete("Use Build(LoggerOptions) and inject the returned ILogger via DI instead of mutating the global Log.Logger.")]
    void Configure(LoggerOptions options);
}
