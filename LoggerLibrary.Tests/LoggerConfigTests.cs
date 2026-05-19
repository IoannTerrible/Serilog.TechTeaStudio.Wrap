using LoggerLibrary;
using Serilog;
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

    [Fact]
    public void Build_NegativeFileSizeLimit_Throws()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            FileSizeLimitBytes = -1,
        };

        Assert.Throws<ArgumentException>(() => new LoggerConfig().Build(options));
    }

    [Fact]
    public void Build_ZeroFileSizeLimit_Throws()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            FileSizeLimitBytes = 0,
        };

        Assert.Throws<ArgumentException>(() => new LoggerConfig().Build(options));
    }

    [Fact]
    public void Build_RollsOnFileSizeLimit_WhenEnabled()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            EnableConsole = false,
            LogFileName = "size.txt",
            MinimumLevel = LogEventLevel.Verbose,
            FileSizeLimitBytes = 512,
            RollOnFileSizeLimit = true,
            RollingInterval = RollingInterval.Infinite,
        };

        var serilog = new LoggerConfig().Build(options);
        var wrapper = new Logger(serilog);

        var payload = new string('x', 200);
        for (int i = 0; i < 20; i++)
        {
            wrapper.LogEvent(LogEventLevel.Information, "{Index} {Payload}", i, payload);
        }

        ((IDisposable)serilog).Dispose();

        var files = Directory.GetFiles(_tempDir, "size*.txt");
        Assert.True(files.Length >= 2, $"Expected at least 2 rolled files, got {files.Length}.");
    }

    [Fact]
    public void Build_NullFileSizeLimit_AcceptedAsUnlimited()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            EnableConsole = false,
            LogFileName = "unlimited.txt",
            FileSizeLimitBytes = null,
        };

        var serilog = new LoggerConfig().Build(options);
        var wrapper = new Logger(serilog);
        wrapper.LogEvent(LogEventLevel.Information, "ok");
        ((IDisposable)serilog).Dispose();

        var files = Directory.GetFiles(_tempDir, "unlimited*.txt");
        Assert.NotEmpty(files);
    }

    [Fact]
    public void Build_ConfigureLoggerCallback_IsInvokedWithLoggerConfiguration()
    {
        var captured = false;
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            EnableConsole = false,
            ConfigureLogger = lc =>
            {
                Assert.NotNull(lc);
                captured = true;
            }
        };

        using var logger = (IDisposable)new LoggerConfig().Build(options);
        Assert.True(captured);
    }

    [Fact]
    public void Build_ConfigureLoggerCallback_AttachedSinkReceivesEvents()
    {
        var auxPath = Path.Combine(_tempDir, "aux.txt");
        Directory.CreateDirectory(_tempDir);

        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            EnableConsole = false,
            LogFileName = "main.txt",
            ConfigureLogger = lc => lc.WriteTo.File(auxPath, rollingInterval: RollingInterval.Infinite)
        };

        var serilog = new LoggerConfig().Build(options);
        var wrapper = new Logger(serilog);
        wrapper.LogEvent(LogEventLevel.Information, "extra-sink-marker");
        ((IDisposable)serilog).Dispose();

        Assert.True(File.Exists(auxPath));
        Assert.Contains("extra-sink-marker", File.ReadAllText(auxPath));
    }

    [Fact]
    public void Build_UseJsonFormatter_FileContainsCompactJson()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            EnableConsole = false,
            LogFileName = "json.txt",
            UseJsonFormatter = true,
            MinimumLevel = LogEventLevel.Verbose,
        };

        var serilog = new LoggerConfig().Build(options);
        var wrapper = new Logger(serilog);
        wrapper.LogEvent(LogEventLevel.Information, "json-payload {User}", "tester");
        ((IDisposable)serilog).Dispose();

        var files = Directory.GetFiles(_tempDir, "json*.txt");
        Assert.NotEmpty(files);
        var content = File.ReadAllText(files[0]).TrimStart();
        Assert.StartsWith("{", content);
        Assert.Contains("\"@mt\"", content);
        Assert.Contains("\"User\":\"tester\"", content);
    }

    [Fact]
    public void Build_UseJsonFormatter_PerLevel_AllFilesAreJson()
    {
        var options = new LoggerOptions
        {
            LogDirectory = _tempDir,
            EnableConsole = false,
            SeparateFilesPerLevel = true,
            LogEventLevels = new[] { LogEventLevel.Information, LogEventLevel.Error },
            UseJsonFormatter = true,
            MinimumLevel = LogEventLevel.Verbose,
        };

        var serilog = new LoggerConfig().Build(options);
        var wrapper = new Logger(serilog);
        wrapper.LogEvent(LogEventLevel.Information, "info-line");
        wrapper.LogEvent(LogEventLevel.Error, "err-line");
        ((IDisposable)serilog).Dispose();

        foreach (var prefix in new[] { "information", "error" })
        {
            var files = Directory.GetFiles(_tempDir, prefix + "*.txt");
            Assert.NotEmpty(files);
            var content = File.ReadAllText(files[0]).TrimStart();
            Assert.StartsWith("{", content);
            Assert.Contains("\"@mt\"", content);
        }
    }
}
