using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Helpers;
using SmrtDoodle.Models;
using System;
using System.Diagnostics;
using Windows.Foundation;

namespace SmrtDoodle.Tests.Helpers;

/// <summary>
/// Performance benchmark tests for critical operations.
/// These tests verify that operations complete within reasonable time bounds
/// and validate scalability of data structures at various canvas sizes.
/// </summary>
[TestClass]
public class PerformanceBenchmarkTests
{
    #region DirtyRectTracker Performance

    [TestMethod]
    public void DirtyRectTracker_1000Invalidations_Under10ms()
    {
        var tracker = new DirtyRectTracker();
        tracker.Clear(); // Start clean

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            tracker.InvalidateCircle(i * 2f, i * 2f, 10f);
        }
        sw.Stop();

        Assert.IsTrue(sw.ElapsedMilliseconds < 10,
            $"1000 invalidations took {sw.ElapsedMilliseconds}ms, expected < 10ms");
        Assert.IsTrue(tracker.IsDirty);
    }

    #endregion

    #region TileGrid Performance

    [TestMethod]
    public void TileGrid_8KCanvas_ResizePerformance()
    {
        var grid = new TileGrid(512);
        var sw = Stopwatch.StartNew();
        grid.Resize(7680, 4320);
        sw.Stop();

        Assert.IsTrue(sw.ElapsedMilliseconds < 5,
            $"TileGrid resize for 8K took {sw.ElapsedMilliseconds}ms");
        Assert.AreEqual(15, grid.Columns); // 7680/512 = 15
        Assert.AreEqual(9, grid.Rows);     // 4320/512 = 8.4 → 9
    }

    [TestMethod]
    public void TileGrid_ViewportCulling_LargeCanvas()
    {
        var grid = new TileGrid(512);
        grid.Resize(7680, 4320); // 15x9 = 135 tiles

        // Viewport is 1920x1080 in the center
        var viewport = new Rect(2000, 1000, 1920, 1080);
        var visible = 0;
        foreach (var _ in grid.GetVisibleTiles(viewport))
            visible++;

        // Should be much less than 135
        Assert.IsTrue(visible < 135, $"Expected viewport culling to filter tiles, got {visible}");
        Assert.IsTrue(visible > 0, "Should have at least 1 visible tile");
    }

    [TestMethod]
    public void TileGrid_DirtyVisibleCombo_Efficient()
    {
        var grid = new TileGrid(512);
        grid.Resize(7680, 4320); // 135 tiles
        grid.ClearAll();

        // Only mark a small rect dirty
        grid.InvalidateRect(new Rect(100, 100, 50, 50));

        // Full viewport
        var count = 0;
        foreach (var _ in grid.GetDirtyVisibleTiles(new Rect(0, 0, 7680, 4320)))
            count++;

        Assert.AreEqual(1, count, "Only 1 dirty tile should be returned");
    }

    #endregion

    #region MemoryMonitor Benchmarks

    [TestMethod]
    public void MemoryEstimate_4K_5Layers()
    {
        long bytes = MemoryMonitor.EstimateMemoryBytes(3840, 2160, 5);
        double mb = bytes / (1024.0 * 1024.0);
        // 3840*2160*4*5 + 3840*2160*4*2 = ~232 MB
        Assert.IsTrue(mb > 200 && mb < 300, $"Expected ~230 MB, got {mb:F1} MB");
    }

    [TestMethod]
    public void MemoryEstimate_8K_10Layers()
    {
        long bytes = MemoryMonitor.EstimateMemoryBytes(7680, 4320, 10);
        double mb = bytes / (1024.0 * 1024.0);
        // Should be ~1.6 GB
        Assert.IsTrue(mb > 1400 && mb < 1700, $"Expected ~1590 MB, got {mb:F1} MB");
    }

    #endregion

    #region RenderThrottler Timing

    [TestMethod]
    public void RenderThrottler_FirstCallAlwaysRenders()
    {
        var throttler = new RenderThrottler(60);
        Assert.IsTrue(throttler.ShouldRender(), "First call should always render");
    }

    [TestMethod]
    public void RenderThrottler_ImmediateSecondCall_Throttled()
    {
        var throttler = new RenderThrottler(60);
        throttler.ShouldRender(); // First call
        Assert.IsFalse(throttler.ShouldRender(), "Immediate second call should be throttled");
        Assert.IsTrue(throttler.HasPendingRender);
    }

    [TestMethod]
    public void RenderThrottler_ForceNextRender_Resets()
    {
        var throttler = new RenderThrottler(60);
        throttler.ShouldRender();
        throttler.ForceNextRender();
        Assert.IsTrue(throttler.ShouldRender(), "After ForceNextRender, ShouldRender should return true");
    }

    #endregion

    #region UndoRedoManager Capacity

    [TestMethod]
    public void UndoRedoManager_CanPushManyItems_NoThrow()
    {
        var manager = new UndoRedoManager();
        for (int i = 0; i < 100; i++)
        {
            manager.Push(new TestUndoAction($"Action {i}"));
        }
        Assert.IsTrue(manager.CanUndo);
    }

    [TestMethod]
    public void UndoRedoManager_UndoRedo50Cycles()
    {
        var manager = new UndoRedoManager();
        for (int i = 0; i < 50; i++)
            manager.Push(new TestUndoAction($"Action {i}"));

        // Undo all
        while (manager.CanUndo)
            manager.Undo();
        Assert.IsFalse(manager.CanUndo);
        Assert.IsTrue(manager.CanRedo);

        // Redo all
        while (manager.CanRedo)
            manager.Redo();
        Assert.IsTrue(manager.CanUndo);
        Assert.IsFalse(manager.CanRedo);
    }

    private class TestUndoAction : IUndoRedoAction
    {
        public string Description { get; }
        public long EstimatedBytes => 100;
        public TestUndoAction(string desc) => Description = desc;
        public void Undo() { }
        public void Redo() { }
        public void Dispose() { }
    }

    #endregion
}
