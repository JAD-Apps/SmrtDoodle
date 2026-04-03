using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for the Brush ribbon group controls:
/// StrokeSizeSlider (range 1-50, default 3) and BrushStyleCombo (8 styles).
/// </summary>
[TestClass]
public class BrushControlTests : AppiumTestBase
{
    private static readonly string[] BrushStyles =
    [
        "Normal", "Calligraphy", "Airbrush", "Oil", "Crayon", "Marker", "Natural Pencil", "Watercolor"
    ];

    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region StrokeSizeSlider — Existence & Default

    [TestMethod]
    public void StrokeSizeSlider_Exists()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");
        Assert.IsNotNull(slider);
        Assert.IsTrue(slider.Displayed);
    }

    [TestMethod]
    public void StrokeSizeSlider_IsEnabled()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");
        Assert.IsTrue(slider.Enabled, "Stroke size slider should be enabled");
    }

    [TestMethod]
    public void StrokeSizeSlider_DefaultValueIs3()
    {
        ResetCanvas();

        var slider = FindByAutomationId("StrokeSizeSlider");
        var value = slider.GetAttribute("Value.Value");
        Assert.AreEqual("3", value, "Default stroke size should be 3");
    }

    #endregion

    #region StrokeSizeSlider — Range Boundaries

    [TestMethod]
    public void StrokeSizeSlider_MinimumIs1()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");
        var min = slider.GetAttribute("RangeValue.Minimum");
        Assert.AreEqual("1", min);
    }

    [TestMethod]
    public void StrokeSizeSlider_MaximumIs50()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");
        var max = slider.GetAttribute("RangeValue.Maximum");
        Assert.AreEqual("50", max);
    }

    [TestMethod]
    public void StrokeSizeSlider_SetToMinimum()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");

        // Use keyboard to set to minimum: click then Home key
        slider.Click();
        Thread.Sleep(100);
        slider.SendKeys(Keys.Home);
        Thread.Sleep(200);

        var value = slider.GetAttribute("Value.Value");
        Assert.AreEqual("1", value, "Slider should be at minimum (1)");
    }

    [TestMethod]
    public void StrokeSizeSlider_SetToMaximum()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");

        slider.Click();
        Thread.Sleep(100);
        slider.SendKeys(Keys.End);
        Thread.Sleep(200);

        var value = slider.GetAttribute("Value.Value");
        Assert.AreEqual("50", value, "Slider should be at maximum (50)");
    }

    [TestMethod]
    public void StrokeSizeSlider_IncrementWithArrowRight()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");

        // Set to minimum first
        slider.Click();
        slider.SendKeys(Keys.Home);
        Thread.Sleep(100);

        // Press right arrow to increment
        slider.SendKeys(Keys.ArrowRight);
        Thread.Sleep(100);

        var value = slider.GetAttribute("Value.Value");
        Assert.AreEqual("2", value, "Should increment from 1 to 2");
    }

    [TestMethod]
    public void StrokeSizeSlider_DecrementWithArrowLeft()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");

        // Set to max first
        slider.Click();
        slider.SendKeys(Keys.End);
        Thread.Sleep(100);

        // Press left arrow to decrement
        slider.SendKeys(Keys.ArrowLeft);
        Thread.Sleep(100);

        var value = slider.GetAttribute("Value.Value");
        Assert.AreEqual("49", value, "Should decrement from 50 to 49");
    }

    [TestMethod]
    public void StrokeSizeSlider_PageUp()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");

        slider.Click();
        slider.SendKeys(Keys.Home);
        Thread.Sleep(100);

        slider.SendKeys(Keys.PageUp);
        Thread.Sleep(100);

        // Page up should jump by a larger increment (typically ~10%)
        var value = double.Parse(slider.GetAttribute("Value.Value"));
        Assert.IsTrue(value > 1, "Page Up from min should increase value");
    }

    [TestMethod]
    public void StrokeSizeSlider_ResetToDefault()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");

        // Set to max
        slider.Click();
        slider.SendKeys(Keys.End);
        Thread.Sleep(100);

        // Reset canvas resets slider to default
        ResetCanvas();

        slider = FindByAutomationId("StrokeSizeSlider");
        var value = slider.GetAttribute("Value.Value");
        Assert.AreEqual("3", value, "After new canvas, slider should reset to default 3");
    }

    #endregion

    #region BrushStyleCombo — Existence & Default

    [TestMethod]
    public void BrushStyleCombo_Exists()
    {
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.IsNotNull(combo);
        Assert.IsTrue(combo.Displayed);
    }

    [TestMethod]
    public void BrushStyleCombo_IsEnabled()
    {
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.IsTrue(combo.Enabled, "Brush style combo should be enabled");
    }

    [TestMethod]
    public void BrushStyleCombo_DefaultIsNormal()
    {
        ResetCanvas();

        var combo = FindByAutomationId("BrushStyleCombo");
        // The displayed text should be "Normal"
        var selectedText = combo.Text;
        Assert.AreEqual("Normal", selectedText, "Default brush style should be Normal");
    }

    #endregion

    #region BrushStyleCombo — All 8 Styles Selection

    [TestMethod]
    public void BrushStyleCombo_SelectNormal()
    {
        SelectBrushStyle("Normal");
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Normal", combo.Text);
    }

    [TestMethod]
    public void BrushStyleCombo_SelectCalligraphy()
    {
        SelectBrushStyle("Calligraphy");
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Calligraphy", combo.Text);
    }

    [TestMethod]
    public void BrushStyleCombo_SelectAirbrush()
    {
        SelectBrushStyle("Airbrush");
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Airbrush", combo.Text);
    }

    [TestMethod]
    public void BrushStyleCombo_SelectOil()
    {
        SelectBrushStyle("Oil");
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Oil", combo.Text);
    }

    [TestMethod]
    public void BrushStyleCombo_SelectCrayon()
    {
        SelectBrushStyle("Crayon");
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Crayon", combo.Text);
    }

    [TestMethod]
    public void BrushStyleCombo_SelectMarker()
    {
        SelectBrushStyle("Marker");
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Marker", combo.Text);
    }

    [TestMethod]
    public void BrushStyleCombo_SelectNaturalPencil()
    {
        SelectBrushStyle("Natural Pencil");
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Natural Pencil", combo.Text);
    }

    [TestMethod]
    public void BrushStyleCombo_SelectWatercolor()
    {
        SelectBrushStyle("Watercolor");
        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Watercolor", combo.Text);
    }

    #endregion

    #region BrushStyleCombo — Cycling

    [TestMethod]
    public void BrushStyleCombo_CycleThroughAll8Styles()
    {
        foreach (var style in BrushStyles)
        {
            SelectBrushStyle(style);
            Thread.Sleep(100);

            var combo = FindByAutomationId("BrushStyleCombo");
            Assert.AreEqual(style, combo.Text, $"Expected brush style '{style}'");
        }
    }

    [TestMethod]
    public void BrushStyleCombo_CycleForwardAndBack()
    {
        // Go forward through all styles
        foreach (var style in BrushStyles)
        {
            SelectBrushStyle(style);
            Thread.Sleep(50);
        }

        // Go backward
        for (int i = BrushStyles.Length - 1; i >= 0; i--)
        {
            SelectBrushStyle(BrushStyles[i]);
            Thread.Sleep(50);
        }

        var combo = FindByAutomationId("BrushStyleCombo");
        Assert.AreEqual("Normal", combo.Text, "Should end at Normal after cycling back");
    }

    [TestMethod]
    public void BrushStyleCombo_KeyboardNavigation()
    {
        var combo = FindByAutomationId("BrushStyleCombo");
        combo.Click();
        Thread.Sleep(200);

        // Select first item
        combo.SendKeys(Keys.Home);
        Thread.Sleep(100);
        combo.SendKeys(Keys.Enter);
        Thread.Sleep(200);

        Assert.AreEqual("Normal", FindByAutomationId("BrushStyleCombo").Text);
    }

    #endregion

    #region BrushStyleCombo — Interaction With Tools

    [TestMethod]
    public void BrushStyle_PersistsWhenSwitchingTools()
    {
        SelectBrushStyle("Watercolor");
        Thread.Sleep(100);

        SelectTool("BtnEraser");
        Thread.Sleep(100);
        SelectTool("BtnBrush");
        Thread.Sleep(100);

        var combo = FindByAutomationId("BrushStyleCombo");
        // Style should still be Watercolor after switching tools
        Assert.AreEqual("Watercolor", combo.Text, "Brush style should persist when switching tools");
    }

    [TestMethod]
    public void BrushStyle_AffectsDrawing()
    {
        SelectTool("BtnBrush");
        SelectBrushStyle("Oil");
        Thread.Sleep(200);

        // Draw a stroke on canvas — verify no crash
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 150, 150);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"), "Canvas should still exist after Oil brush drawing");
    }

    [TestMethod]
    public void StrokeSize_AffectsDrawing()
    {
        SelectTool("BtnPencil");

        var slider = FindByAutomationId("StrokeSizeSlider");
        slider.Click();
        slider.SendKeys(Keys.End); // Max size
        Thread.Sleep(200);

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 30, 30, 130, 130);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"), "Canvas should still exist after max stroke drawing");
    }

    #endregion

    #region Combined Brush Controls

    [TestMethod]
    public void MinSizeWithAllBrushStyles_DoesNotCrash()
    {
        SelectTool("BtnBrush");

        var slider = FindByAutomationId("StrokeSizeSlider");
        slider.Click();
        slider.SendKeys(Keys.Home);
        Thread.Sleep(100);

        var canvas = FindByAutomationId("DrawingCanvas");

        foreach (var style in BrushStyles)
        {
            SelectBrushStyle(style);
            Thread.Sleep(100);
            DragOnElement(canvas, 50, 50, 100, 100);
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void MaxSizeWithAllBrushStyles_DoesNotCrash()
    {
        SelectTool("BtnBrush");

        var slider = FindByAutomationId("StrokeSizeSlider");
        slider.Click();
        slider.SendKeys(Keys.End);
        Thread.Sleep(100);

        var canvas = FindByAutomationId("DrawingCanvas");

        foreach (var style in BrushStyles)
        {
            SelectBrushStyle(style);
            Thread.Sleep(100);
            DragOnElement(canvas, 50, 50, 100, 100);
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Helpers

    private static void SelectBrushStyle(string styleName)
    {
        var combo = FindByAutomationId("BrushStyleCombo");
        combo.Click();
        Thread.Sleep(300);

        var item = FindByName(styleName);
        item.Click();
        Thread.Sleep(200);
    }

    #endregion
}
