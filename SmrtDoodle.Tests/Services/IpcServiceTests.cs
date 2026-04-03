using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Services;

namespace SmrtDoodle.Tests;

[TestClass]
public class IpcServiceTests
{
    [TestMethod]
    public void Default_NotLaunchedFromSmrtPad()
    {
        var svc = new IpcService();
        Assert.IsFalse(svc.IsLaunchedFromSmrtPad);
        Assert.IsNull(svc.TempFilePath);
    }

    [TestMethod]
    public void ParseArguments_SmrtPadFlag()
    {
        var svc = new IpcService();
        svc.ParseArguments(new[] { "--smrtpad" });
        Assert.IsTrue(svc.IsLaunchedFromSmrtPad);
    }

    [TestMethod]
    public void ParseArguments_TempFile()
    {
        var svc = new IpcService();
        svc.ParseArguments(new[] { "--smrtpad", "--temp-file", @"C:\temp\test.png" });
        Assert.IsTrue(svc.IsLaunchedFromSmrtPad);
        Assert.AreEqual(@"C:\temp\test.png", svc.TempFilePath);
    }

    [TestMethod]
    public void ParseArguments_NoArgs()
    {
        var svc = new IpcService();
        svc.ParseArguments(Array.Empty<string>());
        Assert.IsFalse(svc.IsLaunchedFromSmrtPad);
        Assert.IsNull(svc.TempFilePath);
    }

    [TestMethod]
    public void ParseArguments_OnlyTempFile()
    {
        var svc = new IpcService();
        svc.ParseArguments(new[] { "--temp-file", @"C:\output.png" });
        Assert.IsFalse(svc.IsLaunchedFromSmrtPad);
        Assert.AreEqual(@"C:\output.png", svc.TempFilePath);
    }

    [TestMethod]
    public void ParseArguments_TempFileAtEnd_IgnoresMissingValue()
    {
        var svc = new IpcService();
        svc.ParseArguments(new[] { "--temp-file" });
        Assert.IsNull(svc.TempFilePath);
    }

    [TestMethod]
    public void GetOrCreateTempFilePath_CreatesWhenNull()
    {
        var svc = new IpcService();
        var path = svc.GetOrCreateTempFilePath();
        Assert.IsNotNull(path);
        Assert.IsTrue(path.Contains("SmrtDoodle"));
        Assert.IsTrue(path.EndsWith(".png"));
    }

    [TestMethod]
    public void GetOrCreateTempFilePath_ReturnsExistingWhenSet()
    {
        var svc = new IpcService();
        svc.ParseArguments(new[] { "--temp-file", @"C:\existing.png" });
        var path = svc.GetOrCreateTempFilePath();
        Assert.AreEqual(@"C:\existing.png", path);
    }

    [TestMethod]
    public void CleanupTempFile_WhenNoFile_NoException()
    {
        var svc = new IpcService();
        svc.CleanupTempFile(); // Should not throw
    }

    [TestMethod]
    public void CleanupTempFile_WhenNonExistentPath_NoException()
    {
        var svc = new IpcService();
        svc.ParseArguments(new[] { "--temp-file", @"C:\nonexistent\path\test.png" });
        svc.CleanupTempFile(); // Should not throw
    }

    [TestMethod]
    public void ParseArguments_MultipleSmrtPadFlags()
    {
        var svc = new IpcService();
        svc.ParseArguments(new[] { "--smrtpad", "--smrtpad" });
        Assert.IsTrue(svc.IsLaunchedFromSmrtPad);
    }

    [TestMethod]
    public void ParseArguments_UnknownArgs_Ignored()
    {
        var svc = new IpcService();
        svc.ParseArguments(new[] { "--unknown", "value", "--smrtpad" });
        Assert.IsTrue(svc.IsLaunchedFromSmrtPad);
        Assert.IsNull(svc.TempFilePath);
    }
}
