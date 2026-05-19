using LoggerLibrary;
using Serilog;
using Serilog.Events;

namespace LoggerLibrary.Tests;

public class LoggerOptionsTests
{
    [Fact]
    public void Defaults_AreSensible()
    {
        var o = new LoggerOptions();

        Assert.Equal("logs", o.LogDirectory);
        Assert.False(o.SeparateFilesPerLevel);
        Assert.Equal("log.txt", o.LogFileName);
        Assert.Equal(RollingInterval.Day, o.RollingInterval);
        Assert.Equal(LogEventLevel.Information, o.MinimumLevel);
        Assert.True(o.EnableConsole);
        Assert.Equal(31, o.RetainedFileCountLimit);
        Assert.Equal(1L * 1024 * 1024 * 1024, o.FileSizeLimitBytes);
        Assert.False(o.RollOnFileSizeLimit);
        Assert.False(o.UseJsonFormatter);
        Assert.Null(o.ConfigureLogger);
        Assert.Contains(LogEventLevel.Error, o.LogEventLevels);
    }
}
