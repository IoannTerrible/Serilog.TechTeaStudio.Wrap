using LoggerLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoggerLibrary.Tests;

public class LoggerServiceExtensionsTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "loglib-ext-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public void AddLogger_RegistersAllServices()
    {
        var services = new ServiceCollection();
        services.AddLogger(o =>
        {
            o.LogDirectory = _tempDir;
            o.EnableConsole = false;
        });

        using var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetRequiredService<LoggerOptions>());
        Assert.NotNull(sp.GetRequiredService<ILoggerConfig>());
        Assert.NotNull(sp.GetRequiredService<Serilog.ILogger>());
        Assert.NotNull(sp.GetRequiredService<Logger>());
        Assert.NotNull(sp.GetRequiredService<ILogger<LoggerServiceExtensionsTests>>());
    }

    [Fact]
    public void AddLogger_NullServices_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => LoggerServiceExtensions.AddLogger(null!));
    }

    [Fact]
    public void AddLogger_InvalidDirectory_Throws()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(() => services.AddLogger(o => o.LogDirectory = ""));
    }

    [Fact]
    public void AddLogger_BindsFromConfiguration()
    {
        var data = new Dictionary<string, string?>
        {
            ["LogDirectory"] = _tempDir,
            ["EnableConsole"] = "false",
            ["MinimumLevel"] = "Warning",
        };
        IConfiguration cfg = new ConfigurationBuilder().AddInMemoryCollection(data).Build();

        var services = new ServiceCollection();
        services.AddLogger(cfg);

        using var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<LoggerOptions>();

        Assert.Equal(_tempDir, options.LogDirectory);
        Assert.False(options.EnableConsole);
        Assert.Equal(Serilog.Events.LogEventLevel.Warning, options.MinimumLevel);
    }

    [Fact]
    public void AddLogger_MicrosoftLoggerWritesThroughSerilog()
    {
        var services = new ServiceCollection();
        services.AddLogger(o =>
        {
            o.LogDirectory = _tempDir;
            o.EnableConsole = false;
            o.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
        });

        using var sp = services.BuildServiceProvider();
        var msLogger = sp.GetRequiredService<ILogger<LoggerServiceExtensionsTests>>();
        msLogger.LogInformation("hello {Name}", "ms-ext");

        var serilog = sp.GetRequiredService<Serilog.ILogger>();
        ((IDisposable)serilog).Dispose();

        var files = Directory.GetFiles(_tempDir, "log*.txt");
        Assert.NotEmpty(files);
        var content = File.ReadAllText(files[0]);
        Assert.Contains("ms-ext", content);
    }
}
