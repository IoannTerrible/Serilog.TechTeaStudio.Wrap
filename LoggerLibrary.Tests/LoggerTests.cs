using LoggerLibrary;
using Serilog.Events;

namespace LoggerLibrary.Tests;

public class LoggerTests
{
    [Fact]
    public void Ctor_NullLogger_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Logger((Serilog.ILogger)null!));
    }

    [Fact]
    public void LogEvent_EmptyMessage_Throws()
    {
        var logger = new Logger(new Serilog.LoggerConfiguration().CreateLogger());
        Assert.Throws<ArgumentException>(() => logger.LogEvent(LogEventLevel.Information, ""));
    }

    [Fact]
    public void LogEvent_NullException_Throws()
    {
        var logger = new Logger(new Serilog.LoggerConfiguration().CreateLogger());
        Assert.Throws<ArgumentNullException>(() => logger.LogEvent(LogEventLevel.Error, (Exception)null!, "msg"));
    }
}
