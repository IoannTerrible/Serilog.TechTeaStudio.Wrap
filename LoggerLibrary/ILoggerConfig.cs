namespace LoggerLibrary;

/// <summary>Interface for configuring the logger. </summary>
public interface ILoggerConfig
{
    /// <summary>Configures the logger with the specified options.</summary>
    /// <param name="options">The logger configuration options.</param>
    void Configure(LoggerOptions options);
}
