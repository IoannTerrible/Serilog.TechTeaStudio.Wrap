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
        Assert.Contains(LogEventLevel.Error, o.LogEventLevels);
    }
}
