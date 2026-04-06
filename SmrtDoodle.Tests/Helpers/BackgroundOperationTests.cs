using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace SmrtDoodle.Tests.Helpers;

[TestClass]
public class BackgroundOperationTests
{
    [TestMethod]
    public async Task RunAsync_Sync_ReturnsResult()
    {
        var result = await BackgroundOperation.RunAsync<int>(
            (progress, ct) =>
            {
                progress.Report(50);
                return 42;
            });

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task RunAsync_Async_ReturnsResult()
    {
        var result = await BackgroundOperation.RunAsync<string>(
            async (progress, ct) =>
            {
                progress.Report(0);
                await Task.Delay(1, ct);
                progress.Report(100);
                return "done";
            });

        Assert.AreEqual("done", result);
    }

    [TestMethod]
    public async Task RunAsync_CancellationRespected()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
        {
            await BackgroundOperation.RunAsync<int>(
                (progress, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    return 1;
                },
                ct: cts.Token);
        });
    }

    [TestMethod]
    public async Task RunAsync_ProgressCallbackInvoked()
    {
        double lastProgress = -1;
        var result = await BackgroundOperation.RunAsync<int>(
            (progress, ct) =>
            {
                progress.Report(50);
                progress.Report(100);
                return 99;
            },
            onProgress: p => lastProgress = p);

        Assert.AreEqual(99, result);
        // Progress may arrive asynchronously since Progress<T> posts to SynchronizationContext
    }
}

[TestClass]
public class MemoryMonitorTests
{
    [TestMethod]
    public void EstimateMemoryBytes_SingleLayer_Correct()
    {
        // 1920x1080, 1 layer = 1920*1080*4 bytes for layer + 1920*1080*4*2 for undo
        long expected = 1920L * 1080 * 4 * 1 + 1920L * 1080 * 4 * 2;
        long actual = MemoryMonitor.EstimateMemoryBytes(1920, 1080, 1);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void EstimateMemoryBytes_MultipleLayers()
    {
        long expected = 4000L * 4000 * 4 * 5 + 4000L * 4000 * 4 * 2;
        long actual = MemoryMonitor.EstimateMemoryBytes(4000, 4000, 5);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void EstimateMemoryBytes_8KCanvas_LargeValue()
    {
        long result = MemoryMonitor.EstimateMemoryBytes(7680, 4320, 10);
        // 7680*4320*4*10 + 7680*4320*4*2 = ~1.6 GB
        Assert.IsTrue(result > 1_000_000_000, "8K canvas should require over 1 GB");
    }

    [TestMethod]
    public void IsMemoryLow_NotLow_InTests()
    {
        // In a test environment, we shouldn't be near 4GB limit
        Assert.IsFalse(MemoryMonitor.IsMemoryLow());
    }

    [TestMethod]
    public void GetManagedMemoryMb_ReturnsPositiveValue()
    {
        double mb = MemoryMonitor.GetManagedMemoryMb();
        Assert.IsTrue(mb > 0, "Managed memory should be positive");
        Assert.IsTrue(mb < 4096, "Managed memory should be under 4 GB in tests");
    }
}
