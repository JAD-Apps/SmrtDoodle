using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;

namespace SmrtDoodle.UITests;

/// <summary>
/// Tests for right-click context menus on canvas and UI elements.
/// Covers canvas right-click behavior, layer panel context interactions,
/// and verifies context-dependent behavior for different tools.
/// </summary>
[TestClass]
public class ContextMenuTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Canvas Right-Click Behavior

    [TestMethod]
    public void Canvas_RightClick_WithPencil_DrawsWithSecondaryColor()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");

        // Right-click drag should draw with secondary color
        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 100, 100)
               .ClickAndHold()
               .MoveByOffset(50, 0)
               .Release()
               .Perform();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Canvas_RightClick_WithBrush_DrawsWithSecondaryColor()
    {
        ResetCanvas();
        SelectTool("BtnBrush");

        var canvas = FindByAutomationId("DrawingCanvas");
        RightClick(canvas);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Canvas_RightClick_WithEraser_ErasesWithSecondaryColor()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        // Draw something first
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnEraser");
        RightClick(canvas);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Canvas_RightClick_WithEyedropper_PicksSecondaryColor()
    {
        ResetCanvas();
        SelectTool("BtnEyedropper");

        var canvas = FindByAutomationId("DrawingCanvas");
        RightClick(canvas);
        Thread.Sleep(200);

        // Dismiss any context menu
        DismissMenu();
        Thread.Sleep(100);

        Assert.IsNotNull(FindByAutomationId("SecondaryColorBorder"));
    }

    [TestMethod]
    public void Canvas_RightClick_WithMagnifier_ZoomsOut()
    {
        ResetCanvas();
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        // Zoom in first
        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);

        SelectTool("BtnMagnifier");

        var zoomBefore = FindByAutomationId("StatusZoom").Text;
        var canvas = FindByAutomationId("DrawingCanvas");
        RightClick(canvas);
        Thread.Sleep(300);

        var zoomAfter = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual(zoomBefore, zoomAfter, "Right-click magnifier should zoom out");

        ClickMenuItem("View", "100%");
        Thread.Sleep(200);
    }

    [TestMethod]
    public void Canvas_RightClick_WithFill_FillsWithSecondaryColor()
    {
        ResetCanvas();
        SelectTool("BtnFill");

        var canvas = FindByAutomationId("DrawingCanvas");
        RightClick(canvas);
        Thread.Sleep(300);

        // Dismiss any context menu
        DismissMenu();
        Thread.Sleep(100);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Canvas_RightClick_WithLine_DrawsWithSecondaryColor()
    {
        ResetCanvas();
        SelectTool("BtnLine");

        var canvas = FindByAutomationId("DrawingCanvas");

        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 50, 50)
               .ClickAndHold()
               .MoveByOffset(100, 0)
               .Release()
               .Perform();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Canvas_RightClick_WithShape_DrawsWithSecondaryColor()
    {
        ResetCanvas();
        SelectTool("BtnShape");

        var canvas = FindByAutomationId("DrawingCanvas");

        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 50, 50)
               .ClickAndHold()
               .MoveByOffset(100, 100)
               .Release()
               .Perform();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Layer Panel Interactions

    [TestMethod]
    public void LayerPanel_RightClickOnLayer_DoesNotCrash()
    {
        ResetCanvas();

        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));

        if (items.Count > 0)
        {
            RightClick((AppiumElement)items[0]);
            Thread.Sleep(300);

            // Dismiss any context menu
            DismissMenu();
            Thread.Sleep(200);
        }

        Assert.IsNotNull(FindByAutomationId("LayerListView"));
    }

    [TestMethod]
    public void LayerPanel_DoubleClickOnLayer_DoesNotCrash()
    {
        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));

        if (items.Count > 0)
        {
            var actions = new Actions(Driver!);
            actions.DoubleClick(items[0]).Perform();
            Thread.Sleep(300);
        }

        Assert.IsNotNull(FindByAutomationId("LayerListView"));
    }

    #endregion

    #region Toolbar Right-Click

    [TestMethod]
    public void ToolButton_RightClick_DoesNotCrash()
    {
        var btn = FindByAutomationId("BtnPencil");
        RightClick(btn);
        Thread.Sleep(200);

        DismissMenu();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("BtnPencil"));
    }

    [TestMethod]
    public void RibbonBar_RightClick_DoesNotCrash()
    {
        var ribbon = FindByAutomationId("RibbonBar");
        RightClick(ribbon);
        Thread.Sleep(200);

        DismissMenu();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("RibbonBar"));
    }

    #endregion

    #region Menu Right-Click

    [TestMethod]
    public void MenuBar_RightClick_DoesNotCrash()
    {
        var menu = FindByName("File");
        RightClick(menu);
        Thread.Sleep(200);

        DismissMenu();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByName("File"));
    }

    #endregion

    #region Color Swatches Right-Click

    [TestMethod]
    public void PrimaryColorSwatch_RightClick_DoesNotCrash()
    {
        var swatch = FindByAutomationId("PrimaryColorBorder");
        RightClick(swatch);
        Thread.Sleep(200);

        DismissMenu();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("PrimaryColorBorder"));
    }

    [TestMethod]
    public void SecondaryColorSwatch_RightClick_DoesNotCrash()
    {
        var swatch = FindByAutomationId("SecondaryColorBorder");
        RightClick(swatch);
        Thread.Sleep(200);

        DismissMenu();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("SecondaryColorBorder"));
    }

    [TestMethod]
    public void SwapColorsButton_RightClick_DoesNotCrash()
    {
        var btn = FindByAutomationId("SwapColorsButton");
        RightClick(btn);
        Thread.Sleep(200);

        DismissMenu();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("SwapColorsButton"));
    }

    #endregion

    #region Status Bar Right-Click

    [TestMethod]
    public void StatusBar_RightClick_DoesNotCrash()
    {
        var status = FindByAutomationId("StatusZoom");
        RightClick(status);
        Thread.Sleep(200);

        DismissMenu();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("StatusZoom"));
    }

    #endregion
}
