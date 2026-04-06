using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;
using Windows.UI;

namespace SmrtDoodle.Tests.Models;

[TestClass]
public class CanvasSettingsTests
{
    [TestMethod]
    public void DefaultWidth_Is800()
    {
        var settings = new CanvasSettings();
        Assert.AreEqual(800, settings.Width);
    }

    [TestMethod]
    public void DefaultHeight_Is600()
    {
        var settings = new CanvasSettings();
        Assert.AreEqual(600, settings.Height);
    }

    [TestMethod]
    public void DefaultDpi_Is72()
    {
        var settings = new CanvasSettings();
        Assert.AreEqual(72f, settings.Dpi);
    }

    [TestMethod]
    public void DefaultBackgroundColor_IsWhite()
    {
        var settings = new CanvasSettings();
        Assert.AreEqual(Color.FromArgb(255, 255, 255, 255), settings.BackgroundColor);
    }

    [TestMethod]
    public void DefaultShowGrid_IsFalse()
    {
        var settings = new CanvasSettings();
        Assert.IsFalse(settings.ShowGrid);
    }

    [TestMethod]
    public void DefaultShowRuler_IsFalse()
    {
        var settings = new CanvasSettings();
        Assert.IsFalse(settings.ShowRuler);
    }

    [TestMethod]
    public void DefaultGridSpacing_Is20()
    {
        var settings = new CanvasSettings();
        Assert.AreEqual(20, settings.GridSpacing);
    }

    [TestMethod]
    public void SetWidth_Persists()
    {
        var settings = new CanvasSettings { Width = 1920 };
        Assert.AreEqual(1920, settings.Width);
    }

    [TestMethod]
    public void SetHeight_Persists()
    {
        var settings = new CanvasSettings { Height = 1080 };
        Assert.AreEqual(1080, settings.Height);
    }

    [TestMethod]
    public void SetDpi_Persists()
    {
        var settings = new CanvasSettings { Dpi = 300f };
        Assert.AreEqual(300f, settings.Dpi);
    }

    [TestMethod]
    public void SetBackgroundColor_Persists()
    {
        var settings = new CanvasSettings();
        var blue = Color.FromArgb(255, 0, 0, 255);
        settings.BackgroundColor = blue;
        Assert.AreEqual(blue, settings.BackgroundColor);
    }

    [TestMethod]
    public void SetGridSpacing_Persists()
    {
        var settings = new CanvasSettings { GridSpacing = 50 };
        Assert.AreEqual(50, settings.GridSpacing);
    }
}
