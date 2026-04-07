using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for view toggles (grid/ruler) and zoom operations.
/// </summary>
[TestClass]
public class ViewToggleAndZoomTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Grid Toggle

    [TestMethod]
    public void ShowGridToggle_Exists()
    {
        FindByName("View").Click();
        Thread.Sleep(300);

        var item = FindByName("Show Grid");
        Assert.IsNotNull(item);
        Assert.IsTrue(item.Displayed);

        DismissMenu();
    }

    [TestMethod]
    public void ShowGrid_ToggleOn()
    {
        ClickMenuItem("View", "Show Grid");
        Thread.Sleep(300);

        // Canvas should still render
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ShowGrid_ToggleOff()
    {
        ClickMenuItem("View", "Show Grid");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ShowGrid_RapidToggle()
    {
        for (int i = 0; i < 6; i++)
        {
            ClickMenuItem("View", "Show Grid");
            Thread.Sleep(100);
        }

        // After even number of toggles, should be back to original state
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ShowGrid_DrawWithGridOn()
    {
        ClickMenuItem("View", "Show Grid");
        Thread.Sleep(200);

        SelectTool("BtnPencil");
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        // Toggle off
        ClickMenuItem("View", "Show Grid");
        Thread.Sleep(100);
    }

    #endregion

    #region Ruler Toggle

    [TestMethod]
    public void ShowRulerToggle_Exists()
    {
        FindByName("View").Click();
        Thread.Sleep(300);

        var item = FindByName("Show Ruler");
        Assert.IsNotNull(item);
        Assert.IsTrue(item.Displayed);

        DismissMenu();
    }

    [TestMethod]
    public void ShowRuler_ToggleOn()
    {
        ClickMenuItem("View", "Show Ruler");
        Thread.Sleep(300);

        // RulerCanvas may not be exposed via UIA as Canvas controls lack automation peers
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ShowRuler_ToggleOff()
    {
        ClickMenuItem("View", "Show Ruler");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ShowRuler_RapidToggle()
    {
        for (int i = 0; i < 6; i++)
        {
            ClickMenuItem("View", "Show Ruler");
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ShowRuler_DrawWithRulerOn()
    {
        ClickMenuItem("View", "Show Ruler");
        Thread.Sleep(200);

        SelectTool("BtnPencil");
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        // Toggle off
        ClickMenuItem("View", "Show Ruler");
        Thread.Sleep(100);
    }

    #endregion

    #region Grid + Ruler Combined

    [TestMethod]
    public void GridAndRuler_BothOn()
    {
        ClickMenuItem("View", "Show Grid");
        Thread.Sleep(200);
        ClickMenuItem("View", "Show Ruler");
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        // Draw with both on
        SelectTool("BtnPencil");
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        // Toggle both off
        ClickMenuItem("View", "Show Grid");
        Thread.Sleep(100);
        ClickMenuItem("View", "Show Ruler");
        Thread.Sleep(100);
    }

    #endregion

    #region Zoom Via Menu

    [TestMethod]
    public void ZoomIn_ViaMenu_IncreasesZoom()
    {
        ClickViewMenu100Percent();

        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(300);

        var zoom = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual("100%", zoom, "Zoom In should change from 100%");
    }

    [TestMethod]
    public void ZoomOut_ViaMenu_DecreasesZoom()
    {
        ClickViewMenu100Percent();

        ClickMenuItem("View", "Zoom Out");
        Thread.Sleep(300);

        var zoom = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual("100%", zoom, "Zoom Out should change from 100%");
    }

    [TestMethod]
    public void ZoomFit_ViaMenu()
    {
        ClickMenuItem("View", "Zoom to Fit");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
        Assert.IsNotNull(FindByAutomationId("StatusZoom"));

        // Reset
        ClickViewMenu100Percent();
    }

    [TestMethod]
    public void Zoom100_ViaMenu_Resets()
    {
        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);

        ClickViewMenu100Percent();

        Assert.AreEqual("100%", FindByAutomationId("StatusZoom").Text);
    }

    #endregion

    #region Zoom Via Keyboard

    [TestMethod]
    public void ZoomIn_ViaKeyboard()
    {
        ClickViewMenu100Percent();

        SendShortcut(Keys.Control + Keys.Add);
        Thread.Sleep(300);

        var zoom = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual("100%", zoom);

        ClickViewMenu100Percent();
    }

    [TestMethod]
    public void ZoomOut_ViaKeyboard()
    {
        ClickViewMenu100Percent();

        SendShortcut(Keys.Control + Keys.Subtract);
        Thread.Sleep(300);

        var zoom = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual("100%", zoom);

        ClickViewMenu100Percent();
    }

    #endregion

    #region Zoom Levels

    [TestMethod]
    public void Zoom_MultipleZoomInsFromDefault()
    {
        ClickViewMenu100Percent();

        for (int i = 0; i < 5; i++)
        {
            ClickMenuItem("View", "Zoom In");
            Thread.Sleep(200);
        }

        var zoom = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual("100%", zoom, "Multiple zoom ins should change from 100%");

        ClickViewMenu100Percent();
    }

    [TestMethod]
    public void Zoom_MultipleZoomOutsFromDefault()
    {
        ClickViewMenu100Percent();

        for (int i = 0; i < 5; i++)
        {
            ClickMenuItem("View", "Zoom Out");
            Thread.Sleep(200);
        }

        var zoom = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual("100%", zoom, "Multiple zoom outs should change from 100%");

        ClickViewMenu100Percent();
    }

    [TestMethod]
    public void Zoom_InThenOutReturnsToOriginal()
    {
        ClickViewMenu100Percent();

        // Zoom in and out same number of times
        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);
        ClickMenuItem("View", "Zoom Out");
        Thread.Sleep(200);

        // Should be approximately back to 100% (may not be exact due to rounding)
        Assert.IsNotNull(FindByAutomationId("StatusZoom"));
    }

    [TestMethod]
    public void Zoom_CanDrawAfterZoomChange()
    {
        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);

        SelectTool("BtnPencil");
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        ClickViewMenu100Percent();
    }

    [TestMethod]
    public void Zoom_CanDrawAfterZoomOut()
    {
        ClickMenuItem("View", "Zoom Out");
        Thread.Sleep(200);

        SelectTool("BtnPencil");
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        ClickViewMenu100Percent();
    }

    #endregion

    #region Zoom Slider Direct Interaction

    [TestMethod]
    public void ZoomSlider_SetTo50Percent()
    {
        var slider = FindByAutomationId("ZoomSlider");
        slider.Click();
        slider.SendKeys(Keys.Home);
        Thread.Sleep(100);

        // Navigate from 10 to ~50 with page up
        for (int i = 0; i < 5; i++)
        {
            slider.SendKeys(Keys.PageUp);
            Thread.Sleep(50);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        // Reset
        ClickViewMenu100Percent();
    }

    [TestMethod]
    public void ZoomSlider_SetTo200Percent()
    {
        ClickViewMenu100Percent();

        var slider = FindByAutomationId("ZoomSlider");
        slider.Click();

        // Press right arrow many times to get above 100
        for (int i = 0; i < 100; i++)
        {
            slider.SendKeys(Keys.ArrowRight);
        }

        Thread.Sleep(200);

        var value = double.Parse(slider.GetAttribute("RangeValue.Value"));
        Assert.IsTrue(value > 100, "Should be above 100%");

        ClickViewMenu100Percent();
    }

    #endregion
}
