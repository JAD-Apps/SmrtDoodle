using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Helpers;
using Windows.Foundation;

namespace SmrtDoodle.Tests;

[TestClass]
public class DirtyRectTrackerTests
{
    [TestMethod]
    public void Default_IsFullInvalidate()
    {
        var tracker = new DirtyRectTracker();
        Assert.IsTrue(tracker.IsDirty);
        Assert.IsTrue(tracker.IsFullInvalidate);
    }

    [TestMethod]
    public void Clear_ResetsDirtyState()
    {
        var tracker = new DirtyRectTracker();
        tracker.Clear();
        Assert.IsFalse(tracker.IsDirty);
        Assert.IsFalse(tracker.IsFullInvalidate);
    }

    [TestMethod]
    public void Invalidate_MarksDirty()
    {
        var tracker = new DirtyRectTracker();
        tracker.Clear();
        tracker.Invalidate(new Rect(10, 10, 50, 50));
        Assert.IsTrue(tracker.IsDirty);
        Assert.IsFalse(tracker.IsFullInvalidate);
    }

    [TestMethod]
    public void Invalidate_UnionsRects()
    {
        var tracker = new DirtyRectTracker();
        tracker.Clear();
        tracker.Invalidate(new Rect(0, 0, 10, 10));
        tracker.Invalidate(new Rect(20, 20, 10, 10));
        var region = tracker.DirtyRegion;
        Assert.IsTrue(region.Width >= 30);
        Assert.IsTrue(region.Height >= 30);
    }

    [TestMethod]
    public void InvalidateAll_SetsFullInvalidate()
    {
        var tracker = new DirtyRectTracker();
        tracker.Clear();
        tracker.InvalidateAll();
        Assert.IsTrue(tracker.IsFullInvalidate);
    }

    [TestMethod]
    public void InvalidateCircle_MarksDirty()
    {
        var tracker = new DirtyRectTracker();
        tracker.Clear();
        tracker.InvalidateCircle(50, 50, 10);
        Assert.IsTrue(tracker.IsDirty);
        var region = tracker.DirtyRegion;
        Assert.AreEqual(40, region.X);
        Assert.AreEqual(40, region.Y);
        Assert.AreEqual(20, region.Width);
        Assert.AreEqual(20, region.Height);
    }

    [TestMethod]
    public void Invalidate_WhileFullInvalidate_Ignored()
    {
        var tracker = new DirtyRectTracker();
        // Starts as full invalidate
        tracker.Invalidate(new Rect(10, 10, 5, 5));
        Assert.IsTrue(tracker.IsFullInvalidate);
    }

    [TestMethod]
    public void DirtyRegion_WhenFullInvalidate_ReturnsEmpty()
    {
        var tracker = new DirtyRectTracker();
        tracker.InvalidateAll();
        var region = tracker.DirtyRegion;
        Assert.IsTrue(region.IsEmpty);
    }
}

[TestClass]
public class RenderThrottlerTests
{
    [TestMethod]
    public void ShouldRender_FirstCall_ReturnsTrue()
    {
        var throttler = new RenderThrottler(60);
        Assert.IsTrue(throttler.ShouldRender());
    }

    [TestMethod]
    public void ShouldRender_ImmediateSecondCall_ReturnsFalse()
    {
        var throttler = new RenderThrottler(60);
        throttler.ShouldRender();
        Assert.IsFalse(throttler.ShouldRender());
    }

    [TestMethod]
    public void HasPendingRender_AfterDeferral()
    {
        var throttler = new RenderThrottler(60);
        throttler.ShouldRender();
        throttler.ShouldRender(); // deferred
        Assert.IsTrue(throttler.HasPendingRender);
    }

    [TestMethod]
    public void ForceNextRender_AllowsImmediate()
    {
        var throttler = new RenderThrottler(60);
        throttler.ShouldRender();
        throttler.ForceNextRender();
        Assert.IsTrue(throttler.ShouldRender());
    }

    [TestMethod]
    public void ForceNextRender_ClearsPending()
    {
        var throttler = new RenderThrottler(60);
        throttler.ShouldRender();
        throttler.ShouldRender(); // deferred
        Assert.IsTrue(throttler.HasPendingRender);
        throttler.ForceNextRender();
        Assert.IsFalse(throttler.HasPendingRender);
    }

    [TestMethod]
    public void Constructor_DefaultFps()
    {
        var throttler = new RenderThrottler();
        Assert.IsTrue(throttler.ShouldRender());
    }
}
