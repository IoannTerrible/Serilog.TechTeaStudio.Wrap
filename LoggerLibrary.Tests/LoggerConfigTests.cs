using LoggerLibrary;
using Serilog.Events;

namespace LoggerLibrary.Tests;

public class LoggerConfigTests : IDisposable
{
    private readonly string _tempDir;

    public LoggerConfigTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "loglib-" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public void Build_NullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LoggerConfig().Build(null!));
    }

    [Fact]
    public void Build_EmptyLogDirectory_Throws()
    {
        var options = new LoggerOptions { LogDirectory = "   " };
        Assert.Throws<ArgumentException>(() => new LoggerConfig().Build(options));
    }

    [Fact]
    public void Build_SingleFile_CreatesDirectory()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            EnableConsole = false,
            LogFileName = "app.txt",
        };

        using var logger = (IDisposable)new LoggerConfig().Build(options);

        Assert.True(Directory.Exists(_tempDir));
    }

    [Fact]
    public void Build_SeparateFilesPerLevel_EmptyLevels_Throws()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            SeparateFilesPerLevel = true,
            LogEventLevels = Array.Empty<LogEventLevel>(),
        };

        Assert.Throws<ArgumentException>(() => new LoggerConfig().Build(options));
    }

    [Fact]
    public void Build_WritesToFile()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            EnableConsole = false,
            LogFileName = "app.txt",
            MinimumLevel = LogEventLevel.Verbose,
        };

        var serilog = new LoggerConfig().Build(options);
        var wrapper = new Logger(serilog);
        wrapper.LogEvent(LogEventLevel.Information, "hello {Name}", "world");
        ((IDisposable)serilog).Dispose();

        var files = Directory.GetFiles(_tempDir, "app*.txt");
        Assert.NotEmpty(files);
        var content = File.ReadAllText(files[0]);
        Assert.Contains("hello", content);
        Assert.Contains("world", content);
    }
}
