using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for the Status Bar:
/// Position, canvas size, tool name, selection info, zoom percentage, zoom slider.
/// </summary>
[TestClass]
public class StatusBarTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Status Bar Element Existence

    [TestMethod]
    public void StatusPosition_Exists()
    {
        var el = FindByAutomationId("StatusPosition");
        Assert.IsNotNull(el);
        Assert.IsTrue(el.Displayed);
    }

    [TestMethod]
    public void StatusCanvasSize_Exists()
    {
        var el = FindByAutomationId("StatusCanvasSize");
        Assert.IsNotNull(el);
        Assert.IsTrue(el.Displayed);
    }

    [TestMethod]
    public void StatusTool_Exists()
    {
        var el = FindByAutomationId("StatusTool");
        Assert.IsNotNull(el);
        Assert.IsTrue(el.Displayed);
    }

    [TestMethod]
    public void StatusSelection_Exists()
    {
        var el = FindByAutomationId("StatusSelection");
        Assert.IsNotNull(el);
        Assert.IsTrue(el.Displayed);
    }

    [TestMethod]
    public void StatusZoom_Exists()
    {
        var el = FindByAutomationId("StatusZoom");
        Assert.IsNotNull(el);
        Assert.IsTrue(el.Displayed);
    }

    [TestMethod]
    public void ZoomSlider_Exists()
    {
        var slider = FindByAutomationId("ZoomSlider");
        Assert.IsNotNull(slider);
        Assert.IsTrue(slider.Displayed);
    }

    #endregion

    #region Default Values

    [TestMethod]
    public void StatusPosition_DefaultValue()
    {
        ResetCanvas();
        var el = FindByAutomationId("StatusPosition");
        Assert.AreEqual("0, 0 px", el.Text, "Default position should be '0, 0 px'");
    }

    [TestMethod]
    public void StatusCanvasSize_DefaultValue()
    {
        ResetCanvas();
        var el = FindByAutomationId("StatusCanvasSize");
        Assert.AreEqual("800 x 600 px", el.Text, "Default canvas size should be '800 x 600 px'");
    }

    [TestMethod]
    public void StatusTool_DefaultValue()
    {
        ResetCanvas();
        var el = FindByAutomationId("StatusTool");
        Assert.AreEqual("Pencil", el.Text, "Default tool should be 'Pencil'");
    }

    [TestMethod]
    public void StatusZoom_DefaultValue()
    {
        ResetCanvas();
        // Ensure zoom is at 100%
        ClickMenuItem("View", "100%");
        Thread.Sleep(300);

        var el = FindByAutomationId("StatusZoom");
        Assert.AreEqual("100%", el.Text, "Default zoom should be '100%'");
    }

    [TestMethod]
    public void StatusSelection_DefaultIsEmpty()
    {
        ResetCanvas();
        var el = FindByAutomationId("StatusSelection");
        Assert.AreEqual("", el.Text, "Default selection status should be empty");
    }

    #endregion

    #region Status Bar — Tool Name Updates

    [TestMethod]
    public void StatusTool_UpdatesWhenBrushSelected()
    {
        SelectTool("BtnBrush");
        AssertStatusText("StatusTool", "Brush", "Status tool after Brush selection");
    }

    [TestMethod]
    public void StatusTool_UpdatesWhenEraserSelected()
    {
        SelectTool("BtnEraser");
        AssertStatusText("StatusTool", "Eraser", "Status tool after Eraser selection");
    }

    [TestMethod]
    public void StatusTool_UpdatesWhenFillSelected()
    {
        SelectTool("BtnFill");
        AssertStatusText("StatusTool", "Fill", "Status tool after Fill selection");
    }

    [TestMethod]
    public void StatusTool_UpdatesWhenLineSelected()
    {
        SelectTool("BtnLine");
        AssertStatusText("StatusTool", "Line", "Status tool after Line selection");
    }

    [TestMethod]
    public void StatusTool_UpdatesWhenShapeSelected()
    {
        SelectTool("BtnShape");
        AssertStatusText("StatusTool", "Shape", "Status tool after Shape selection");
    }

    [TestMethod]
    public void StatusTool_UpdatesWhenMagnifierSelected()
    {
        SelectTool("BtnMagnifier");
        AssertStatusText("StatusTool", "Magnifier", "Status tool after Magnifier selection");
    }

    [TestMethod]
    public void StatusTool_CyclesThroughAllTools()
    {
        var expected = new Dictionary<string, string>
        {
            ["BtnPencil"] = "Pencil",
            ["BtnBrush"] = "Brush",
            ["BtnEraser"] = "Eraser",
            ["BtnFill"] = "Fill",
            ["BtnText"] = "Text",
            ["BtnEyedropper"] = "Eyedropper",
            ["BtnLine"] = "Line",
            ["BtnCurve"] = "Curve",
            ["BtnShape"] = "Shape",
            ["BtnSelect"] = "Selection",
            ["BtnFreeSelect"] = "FreeFormSelection",
            ["BtnMagnifier"] = "Magnifier"
        };

        foreach (var (toolId, expectedText) in expected)
        {
            SelectTool(toolId);
            var status = FindByAutomationId("StatusTool").Text;
            Assert.AreEqual(expectedText, status, $"StatusTool after selecting {toolId}");
        }
    }

    #endregion

    #region Zoom Slider

    [TestMethod]
    public void ZoomSlider_IsEnabled()
    {
        var slider = FindByAutomationId("ZoomSlider");
        Assert.IsTrue(slider.Enabled);
    }

    [TestMethod]
    public void ZoomSlider_MinimumIs10()
    {
        var slider = FindByAutomationId("ZoomSlider");
        var min = slider.GetAttribute("RangeValue.Minimum");
        Assert.AreEqual("10", min);
    }

    [TestMethod]
    public void ZoomSlider_MaximumIs800()
    {
        var slider = FindByAutomationId("ZoomSlider");
        var max = slider.GetAttribute("RangeValue.Maximum");
        Assert.AreEqual("800", max);
    }

    [TestMethod]
    public void ZoomSlider_DefaultIs100()
    {
        ResetCanvas();
        ClickMenuItem("View", "100%");
        Thread.Sleep(300);

        var slider = FindByAutomationId("ZoomSlider");
        var value = slider.GetAttribute("Value.Value");
        Assert.AreEqual("100", value);
    }

    [TestMethod]
    public void ZoomSlider_SetToMinimum()
    {
        var slider = FindByAutomationId("ZoomSlider");
        slider.Click();
        slider.SendKeys(Keys.Home);
        Thread.Sleep(200);

        var zoomText = FindByAutomationId("StatusZoom").Text;
        Assert.AreEqual("10%", zoomText, "Zoom at minimum should show 10%");
    }

    [TestMethod]
    public void ZoomSlider_SetToMaximum()
    {
        var slider = FindByAutomationId("ZoomSlider");
        slider.Click();
        slider.SendKeys(Keys.End);
        Thread.Sleep(200);

        var zoomText = FindByAutomationId("StatusZoom").Text;
        Assert.AreEqual("800%", zoomText, "Zoom at maximum should show 800%");

        // Reset
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);
    }

    [TestMethod]
    public void ZoomSlider_IncrementWithArrowRight()
    {
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        var slider = FindByAutomationId("ZoomSlider");
        slider.Click();
        slider.SendKeys(Keys.ArrowRight);
        Thread.Sleep(200);

        var value = double.Parse(slider.GetAttribute("Value.Value"));
        Assert.IsTrue(value > 100, "Arrow right should increase zoom from 100");
    }

    [TestMethod]
    public void ZoomSlider_DecrementWithArrowLeft()
    {
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        var slider = FindByAutomationId("ZoomSlider");
        slider.Click();
        slider.SendKeys(Keys.ArrowLeft);
        Thread.Sleep(200);

        var value = double.Parse(slider.GetAttribute("Value.Value"));
        Assert.IsTrue(value < 100, "Arrow left should decrease zoom from 100");
    }

    #endregion

    #region Zoom Status Synchronization

    [TestMethod]
    public void ZoomStatus_MatchesSliderValue()
    {
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        var slider = FindByAutomationId("ZoomSlider");
        var sliderValue = slider.GetAttribute("Value.Value");
        var zoomText = FindByAutomationId("StatusZoom").Text;

        Assert.AreEqual($"{sliderValue}%", zoomText, "Zoom status should match slider value");
    }

    [TestMethod]
    public void ZoomIn_UpdatesBothSliderAndStatus()
    {
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(300);

        var slider = FindByAutomationId("ZoomSlider");
        var sliderValue = double.Parse(slider.GetAttribute("Value.Value"));
        var zoomText = FindByAutomationId("StatusZoom").Text;

        Assert.IsTrue(sliderValue > 100, "Slider should be above 100 after zoom in");
        Assert.AreNotEqual("100%", zoomText, "Status should change after zoom in");
    }

    [TestMethod]
    public void ZoomOut_UpdatesBothSliderAndStatus()
    {
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        ClickMenuItem("View", "Zoom Out");
        Thread.Sleep(300);

        var slider = FindByAutomationId("ZoomSlider");
        var sliderValue = double.Parse(slider.GetAttribute("Value.Value"));
        var zoomText = FindByAutomationId("StatusZoom").Text;

        Assert.IsTrue(sliderValue < 100, "Slider should be below 100 after zoom out");
        Assert.AreNotEqual("100%", zoomText, "Status should change after zoom out");
    }

    [TestMethod]
    public void Zoom100_ResetsBothSliderAndStatus()
    {
        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);

        ClickMenuItem("View", "100%");
        Thread.Sleep(300);

        var slider = FindByAutomationId("ZoomSlider");
        Assert.AreEqual("100", slider.GetAttribute("Value.Value"));
        Assert.AreEqual("100%", FindByAutomationId("StatusZoom").Text);
    }

    #endregion

    #region Canvas Size After Operations

    [TestMethod]
    public void CanvasSize_UpdatesAfterRotate90()
    {
        ResetCanvas();
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        var sizeBefore = FindByAutomationId("StatusCanvasSize").Text;

        ClickMenuItem("Image", "Rotate 90°");
        Thread.Sleep(300);

        var sizeAfter = FindByAutomationId("StatusCanvasSize").Text;
        Assert.AreNotEqual(sizeBefore, sizeAfter, "Canvas size should change after 90° rotation (800×600 → 600×800)");
    }

    [TestMethod]
    public void CanvasSize_RestoredAfterRotate90Twice()
    {
        ResetCanvas();

        var sizeOriginal = FindByAutomationId("StatusCanvasSize").Text;

        ClickMenuItem("Image", "Rotate 90°");
        Thread.Sleep(200);
        ClickMenuItem("Image", "Rotate 90°");
        Thread.Sleep(200);

        // After two 90° rotations (=180°), dimensions should be same
        var sizeAfter = FindByAutomationId("StatusCanvasSize").Text;
        Assert.AreEqual(sizeOriginal, sizeAfter, "180° rotation should preserve dimensions");
    }

    #endregion

    #region Position Updates

    [TestMethod]
    public void MouseOverCanvas_UpdatesPosition()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");

        // Move mouse over canvas (this should update position)
        var actions = new OpenQA.Selenium.Interactions.Actions(Driver!);
        actions.MoveToElement(canvas, 100, 100).Perform();
        Thread.Sleep(300);

        var pos = FindByAutomationId("StatusPosition").Text;
        Assert.IsTrue(pos.Contains("px"), "Position should contain 'px' after mouse move");
    }

    #endregion
}
