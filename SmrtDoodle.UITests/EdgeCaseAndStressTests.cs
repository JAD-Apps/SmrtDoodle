using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace SmrtDoodle.UITests;

/// <summary>
/// Edge case and stress tests for application hardening:
/// Rapid tool clicking, slider boundary values, layer add/delete cycles,
/// undo/redo rapid cycling, window title verification, and comprehensive workflows.
/// </summary>
[TestClass]
public class EdgeCaseAndStressTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Window Title

    [TestMethod]
    public void WindowTitle_IsSmrtDoodle()
    {
        var title = Driver!.Title;
        Assert.IsTrue(title.Contains("SmrtDoodle"), $"Window title should contain 'SmrtDoodle', got: '{title}'");
    }

    [TestMethod]
    public void WindowTitle_AfterNewCanvas()
    {
        ResetCanvas();
        var title = Driver!.Title;
        Assert.IsTrue(title.Contains("SmrtDoodle"), $"Window title should still contain 'SmrtDoodle' after new canvas");
    }

    #endregion

    #region Rapid Tool Clicking

    [TestMethod]
    public void RapidToolClick_AllTools10Times()
    {
        var tools = new[]
        {
            "BtnPencil", "BtnBrush", "BtnEraser", "BtnFill", "BtnText", "BtnEyedropper",
            "BtnLine", "BtnCurve", "BtnShape", "BtnSelect", "BtnFreeSelect", "BtnMagnifier"
        };

        for (int round = 0; round < 10; round++)
        {
            foreach (var toolId in tools)
            {
                SelectTool(toolId);
                Thread.Sleep(20);
            }
        }

        // App should still be responsive
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
        Assert.IsNotNull(FindByAutomationId("StatusTool"));
    }

    [TestMethod]
    public void RapidToolClick_SameToolRepeated()
    {
        for (int i = 0; i < 50; i++)
        {
            SelectTool("BtnPencil");
            Thread.Sleep(10);
        }

        Assert.IsTrue(IsToggled(FindByAutomationId("BtnPencil")));
    }

    [TestMethod]
    public void RapidToolClick_AlternatingTwoTools()
    {
        for (int i = 0; i < 30; i++)
        {
            SelectTool("BtnPencil");
            Thread.Sleep(10);
            SelectTool("BtnBrush");
            Thread.Sleep(10);
        }

        Assert.IsTrue(IsToggled(FindByAutomationId("BtnBrush")));
        Assert.IsFalse(IsToggled(FindByAutomationId("BtnPencil")));
    }

    #endregion

    #region Slider Boundary Values

    [TestMethod]
    public void StrokeSlider_Min_DrawDoesNotCrash()
    {
        SelectTool("BtnPencil");

        var slider = FindByAutomationId("StrokeSizeSlider");
        slider.Click();
        slider.SendKeys(Keys.Home);
        Thread.Sleep(100);

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void StrokeSlider_Max_DrawDoesNotCrash()
    {
        SelectTool("BtnPencil");

        var slider = FindByAutomationId("StrokeSizeSlider");
        slider.Click();
        slider.SendKeys(Keys.End);
        Thread.Sleep(100);

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ZoomSlider_Min_CanvasStillVisible()
    {
        var slider = FindByAutomationId("ZoomSlider");
        slider.Click();
        slider.SendKeys(Keys.Home);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        ClickMenuItem("View", "100%");
        Thread.Sleep(200);
    }

    [TestMethod]
    public void ZoomSlider_Max_CanvasStillVisible()
    {
        var slider = FindByAutomationId("ZoomSlider");
        slider.Click();
        slider.SendKeys(Keys.End);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        ClickMenuItem("View", "100%");
        Thread.Sleep(200);
    }

    [TestMethod]
    public void StrokeSlider_RapidMinMaxCycle()
    {
        var slider = FindByAutomationId("StrokeSizeSlider");

        for (int i = 0; i < 10; i++)
        {
            slider.Click();
            slider.SendKeys(Keys.Home);
            Thread.Sleep(20);
            slider.SendKeys(Keys.End);
            Thread.Sleep(20);
        }

        Assert.IsNotNull(FindByAutomationId("StrokeSizeSlider"));
    }

    #endregion

    #region Layer Add/Delete Cycles

    [TestMethod]
    public void LayerAddDelete_10Cycles()
    {
        ResetCanvas();

        for (int i = 0; i < 10; i++)
        {
            ClickMenuItem("Layers", "Add Layer");
            Thread.Sleep(100);
            ClickMenuItem("Layers", "Delete Layer");
            Thread.Sleep(100);
        }

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count, "After 10 add/delete cycles, should have 1 layer");
    }

    [TestMethod]
    public void LayerAdd_20Layers_ThenFlatten()
    {
        ResetCanvas();

        for (int i = 0; i < 20; i++)
        {
            ClickMenuItem("Layers", "Add Layer");
            Thread.Sleep(50);
        }

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(21, list.FindElements(By.ClassName("ListViewItem")).Count);

        ClickMenuItem("Layers", "Flatten Image");
        Thread.Sleep(300);

        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);
    }

    [TestMethod]
    public void LayerDuplicate_10Times_ThenDeleteAll()
    {
        ResetCanvas();

        for (int i = 0; i < 10; i++)
        {
            ClickMenuItem("Layers", "Duplicate Layer");
            Thread.Sleep(50);
        }

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(11, list.FindElements(By.ClassName("ListViewItem")).Count);

        // Delete all but last
        for (int i = 0; i < 10; i++)
        {
            ClickMenuItem("Layers", "Delete Layer");
            Thread.Sleep(50);
        }

        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);
    }

    #endregion

    #region Undo/Redo Rapid Cycling

    [TestMethod]
    public void UndoRedo_RapidCycle_10Times()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        for (int i = 0; i < 10; i++)
        {
            SendShortcut(Keys.Control + "z");
            Thread.Sleep(50);
            SendShortcut(Keys.Control + "y");
            Thread.Sleep(50);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Undo_MultipleStrokes_ThenRedoAll()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");

        // Draw 5 strokes
        for (int i = 0; i < 5; i++)
        {
            DragOnElement(canvas, 30, 30 + (i * 30), 200, 30 + (i * 30));
            Thread.Sleep(100);
        }

        // Undo all 5
        for (int i = 0; i < 5; i++)
        {
            SendShortcut(Keys.Control + "z");
            Thread.Sleep(50);
        }

        // Redo all 5
        for (int i = 0; i < 5; i++)
        {
            SendShortcut(Keys.Control + "y");
            Thread.Sleep(50);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Undo_BeyondHistory_DoesNotCrash()
    {
        ResetCanvas();

        // Undo 50 times when there's nothing to undo
        for (int i = 0; i < 50; i++)
        {
            SendShortcut(Keys.Control + "z");
            Thread.Sleep(20);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Redo_BeyondHistory_DoesNotCrash()
    {
        ResetCanvas();

        // Redo 50 times when there's nothing to redo
        for (int i = 0; i < 50; i++)
        {
            SendShortcut(Keys.Control + "y");
            Thread.Sleep(20);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Undo_ViaMenu()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        ClickMenuItem("Edit", "Undo");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Redo_ViaMenu()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        ClickMenuItem("Edit", "Undo");
        Thread.Sleep(200);
        ClickMenuItem("Edit", "Redo");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Comprehensive Workflows

    [TestMethod]
    public void Workflow_DrawFlipRotateInvertUndoRedo()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 300, 300);
        Thread.Sleep(200);

        ClickMenuItem("Image", "Flip Horizontal");
        Thread.Sleep(200);

        ClickMenuItem("Image", "Rotate 90°");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + Keys.Shift + "i");
        Thread.Sleep(200);

        // Undo everything
        for (int i = 0; i < 4; i++)
        {
            SendShortcut(Keys.Control + "z");
            Thread.Sleep(100);
        }

        // Redo everything
        for (int i = 0; i < 4; i++)
        {
            SendShortcut(Keys.Control + "y");
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Workflow_AllToolsDrawOnSameCanvas()
    {
        ResetCanvas();

        var canvas = FindByAutomationId("DrawingCanvas");
        int y = 20;

        // Pencil
        SelectTool("BtnPencil");
        DragOnElement(canvas, 30, y, 200, y);
        y += 30;
        Thread.Sleep(50);

        // Brush
        SelectTool("BtnBrush");
        DragOnElement(canvas, 30, y, 200, y);
        y += 30;
        Thread.Sleep(50);

        // Line
        SelectTool("BtnLine");
        DragOnElement(canvas, 30, y, 200, y);
        y += 30;
        Thread.Sleep(50);

        // Shape
        SelectTool("BtnShape");
        DragOnElement(canvas, 30, y, 100, y + 20);
        y += 40;
        Thread.Sleep(50);

        // Curve
        SelectTool("BtnCurve");
        DragOnElement(canvas, 30, y, 200, y);
        y += 30;
        Thread.Sleep(50);

        // Eraser over part
        SelectTool("BtnEraser");
        DragOnElement(canvas, 100, 20, 100, y);
        Thread.Sleep(50);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Workflow_NewCanvasLayerDrawFlatten()
    {
        ResetCanvas();

        // Add layers and draw on each
        for (int i = 0; i < 3; i++)
        {
            ClickMenuItem("Layers", "Add Layer");
            Thread.Sleep(150);

            SelectTool("BtnPencil");
            var canvas = FindByAutomationId("DrawingCanvas");
            DragOnElement(canvas, 50 + (i * 30), 50, 200 + (i * 30), 200);
            Thread.Sleep(100);
        }

        // Flatten and verify
        ClickMenuItem("Layers", "Flatten Image");
        Thread.Sleep(300);

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);
    }

    [TestMethod]
    public void Workflow_ZoomDrawZoomBackVerify()
    {
        ResetCanvas();
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        // Zoom in
        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);

        // Draw
        SelectTool("BtnPencil");
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        // Zoom back to 100%
        ClickMenuItem("View", "100%");
        Thread.Sleep(300);

        Assert.AreEqual("100%", FindByAutomationId("StatusZoom").Text);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Workflow_SelectCopyPasteUndoNewCanvas()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "c");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "v");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "z");
        Thread.Sleep(200);

        ResetCanvas();

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Application Resilience

    [TestMethod]
    public void App_RemainsResponsive_AfterHeavyDrawing()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        var rng = new Random(123);

        for (int i = 0; i < 100; i++)
        {
            int x = rng.Next(10, 350);
            int y = rng.Next(10, 250);
            DragOnElement(canvas, x, y, x + rng.Next(5, 30), y + rng.Next(5, 30));
        }

        Thread.Sleep(500);

        // Verify app is still responsive
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
        Assert.IsNotNull(FindByAutomationId("StatusTool"));
        Assert.IsNotNull(FindByAutomationId("StatusZoom"));
    }

    [TestMethod]
    public void App_HandlesRapidMenuAccess()
    {
        var menus = new[] { "File", "Edit", "Image", "View", "Layers" };

        for (int round = 0; round < 3; round++)
        {
            foreach (var menuName in menus)
            {
                FindByName(menuName).Click();
                Thread.Sleep(100);
                DismissMenu();
                Thread.Sleep(50);
            }
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void App_HandlesRapidComboBoxChanges()
    {
        var shapes = new[] { "Rectangle", "Ellipse", "Triangle", "Star", "Heart", "Lightning" };

        SelectTool("BtnShape");

        foreach (var shape in shapes)
        {
            var combo = FindByAutomationId("ShapeTypeCombo");
            combo.Click();
            Thread.Sleep(100);
            FindByName(shape).Click();
            Thread.Sleep(50);
        }

        Assert.AreEqual("Lightning", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void App_HandlesRapidColorPaletteClicks()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));

        // Click all 28 colors rapidly
        foreach (var item in items)
        {
            item.Click();
            Thread.Sleep(20);
        }

        Assert.IsNotNull(FindByAutomationId("PrimaryColorBorder"));
    }

    #endregion

    #region Insert Button (Hidden by Default)

    [TestMethod]
    public void InsertButton_HiddenByDefault()
    {
        var btn = TryFindByAutomationId("InsertButton");
        if (btn is not null)
        {
            // It exists in DOM but should not be visible in normal mode
            Assert.IsFalse(btn.Displayed, "Insert button should be hidden in normal mode");
        }
    }

    #endregion

    #region Ribbon Bar

    [TestMethod]
    public void RibbonBar_IsDisplayed()
    {
        var ribbon = FindByAutomationId("RibbonBar");
        Assert.IsNotNull(ribbon);
        Assert.IsTrue(ribbon.Displayed);
    }

    [TestMethod]
    public void RibbonBar_HasCorrectHeight()
    {
        var ribbon = FindByAutomationId("RibbonBar");
        var height = ribbon.Size.Height;
        // Ribbon is set to Height="100" in XAML
        Assert.IsTrue(height >= 90 && height <= 110,
            $"Ribbon height should be ~100px, got {height}");
    }

    #endregion
}
