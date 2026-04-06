using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Services;

namespace SmrtDoodle.Tests;

[TestClass]
public class LoggingServiceTests
{
    [TestMethod]
    public void Instance_ReturnsSingleton()
    {
        var a = LoggingService.Instance;
        var b = LoggingService.Instance;
        Assert.AreSame(a, b);
    }

    [TestMethod]
    public void MinimumLevel_DefaultIsInfo()
    {
        Assert.AreEqual(LogLevel.Info, LoggingService.Instance.MinimumLevel);
    }

    [TestMethod]
    public void MinimumLevel_CanBeSet()
    {
        var original = LoggingService.Instance.MinimumLevel;
        try
        {
            LoggingService.Instance.MinimumLevel = LogLevel.Warning;
            Assert.AreEqual(LogLevel.Warning, LoggingService.Instance.MinimumLevel);
        }
        finally
        {
            LoggingService.Instance.MinimumLevel = original;
        }
    }

    [TestMethod]
    public void Debug_DoesNotThrow()
    {
        LoggingService.Instance.Debug("test message");
    }

    [TestMethod]
    public void Info_DoesNotThrow()
    {
        LoggingService.Instance.Info("info message");
    }

    [TestMethod]
    public void Warning_DoesNotThrow()
    {
        LoggingService.Instance.Warning("warning message");
    }

    [TestMethod]
    public void Error_WithException_DoesNotThrow()
    {
        LoggingService.Instance.Error("error message", new InvalidOperationException("test"));
    }

    [TestMethod]
    public void Error_WithoutException_DoesNotThrow()
    {
        LoggingService.Instance.Error("error message");
    }

    [TestMethod]
    public void Fatal_DoesNotThrow()
    {
        LoggingService.Instance.Fatal("fatal message", new Exception("test"));
    }

    [TestMethod]
    public void AllLogLevels_Defined()
    {
        var values = Enum.GetValues(typeof(LogLevel));
        Assert.AreEqual(5, values.Length);
    }

    [TestMethod]
    public void LogLevel_Ordering()
    {
        Assert.IsTrue(LogLevel.Debug < LogLevel.Info);
        Assert.IsTrue(LogLevel.Info < LogLevel.Warning);
        Assert.IsTrue(LogLevel.Warning < LogLevel.Error);
        Assert.IsTrue(LogLevel.Error < LogLevel.Fatal);
    }
}
