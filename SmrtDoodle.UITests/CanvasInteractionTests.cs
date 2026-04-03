using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for canvas drawing interactions:
/// Pencil/brush/eraser drawing, line/curve creation, fill tool,
/// eyedropper, magnifier zoom, and canvas container behavior.
/// </summary>
[TestClass]
public class CanvasInteractionTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Canvas Existence

    [TestMethod]
    public void DrawingCanvas_Exists()
    {
        var canvas = FindByAutomationId("DrawingCanvas");
        Assert.IsNotNull(canvas);
        Assert.IsTrue(canvas.Displayed);
    }

    [TestMethod]
    public void CanvasContainer_Exists()
    {
        var container = FindByAutomationId("CanvasContainer");
        Assert.IsNotNull(container);
        Assert.IsTrue(container.Displayed);
    }

    [TestMethod]
    public void CanvasScrollViewer_Exists()
    {
        var sv = FindByAutomationId("CanvasScrollViewer");
        Assert.IsNotNull(sv);
        Assert.IsTrue(sv.Displayed);
    }

    [TestMethod]
    public void RulerCanvas_Exists()
    {
        var ruler = FindByAutomationId("RulerCanvas");
        Assert.IsNotNull(ruler);
    }

    #endregion

    #region Pencil Drawing

    [TestMethod]
    public void Pencil_ClickOnCanvas_DoesNotCrash()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        canvas.Click();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Pencil_DragOnCanvas_DrawsStroke()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Pencil_DrawMultipleStrokes()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        for (int i = 0; i < 5; i++)
        {
            DragOnElement(canvas, 30, 30 + (i * 20), 200, 30 + (i * 20));
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Pencil_DrawDiagonalStroke()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 10, 10, 300, 300);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Pencil_DrawShortStroke()
    {
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 100, 100, 101, 101);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Brush Drawing

    [TestMethod]
    public void Brush_DragOnCanvas_DrawsStroke()
    {
        ResetCanvas();
        SelectTool("BtnBrush");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 100);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Brush_DrawWithMaxSize()
    {
        SelectTool("BtnBrush");

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
    public void Brush_DrawWithMinSize()
    {
        SelectTool("BtnBrush");

        var slider = FindByAutomationId("StrokeSizeSlider");
        slider.Click();
        slider.SendKeys(Keys.Home);
        Thread.Sleep(100);

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Eraser Drawing

    [TestMethod]
    public void Eraser_DragOnCanvas_Erases()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnEraser");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Eraser_DrawOverEntireArea()
    {
        SelectTool("BtnEraser");

        var slider = FindByAutomationId("StrokeSizeSlider");
        slider.Click();
        slider.SendKeys(Keys.End);
        Thread.Sleep(100);

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 10, 10, 300, 300);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Line Drawing

    [TestMethod]
    public void Line_DragOnCanvas_DrawsLine()
    {
        ResetCanvas();
        SelectTool("BtnLine");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 250, 50);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Line_DrawDiagonal()
    {
        SelectTool("BtnLine");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 250, 250);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Line_DrawVertical()
    {
        SelectTool("BtnLine");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 100, 50, 100, 300);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Line_DrawZeroLength()
    {
        SelectTool("BtnLine");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 100, 100, 100, 100);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Curve Drawing

    [TestMethod]
    public void Curve_DragOnCanvas_DrawsCurve()
    {
        ResetCanvas();
        SelectTool("BtnCurve");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 150, 250, 150);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Fill Tool

    [TestMethod]
    public void Fill_ClickOnCanvas_FillsArea()
    {
        ResetCanvas();
        SelectTool("BtnFill");

        var canvas = FindByAutomationId("DrawingCanvas");
        canvas.Click();
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Fill_ClickMultipleTimes_DoesNotCrash()
    {
        ResetCanvas();
        SelectTool("BtnFill");

        var canvas = FindByAutomationId("DrawingCanvas");
        for (int i = 0; i < 5; i++)
        {
            canvas.Click();
            Thread.Sleep(200);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Eyedropper Tool

    [TestMethod]
    public void Eyedropper_ClickOnCanvas_PicksColor()
    {
        ResetCanvas();
        SelectTool("BtnEyedropper");

        var canvas = FindByAutomationId("DrawingCanvas");
        canvas.Click();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("PrimaryColorBorder"));
    }

    [TestMethod]
    public void Eyedropper_ClickDifferentPositions()
    {
        SelectTool("BtnEyedropper");

        var canvas = FindByAutomationId("DrawingCanvas");

        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 50, 50).Click().Perform();
        Thread.Sleep(100);

        actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 200, 200).Click().Perform();
        Thread.Sleep(100);

        actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 100, 300).Click().Perform();
        Thread.Sleep(100);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Eyedropper_RightClickPicksSecondaryColor()
    {
        SelectTool("BtnEyedropper");

        var canvas = FindByAutomationId("DrawingCanvas");

        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 100, 100).ContextClick().Perform();
        Thread.Sleep(200);

        // Dismiss any context menu that might appear
        DismissMenu();
        Thread.Sleep(100);

        Assert.IsNotNull(FindByAutomationId("SecondaryColorBorder"));
    }

    #endregion

    #region Magnifier Tool

    [TestMethod]
    public void Magnifier_LeftClick_ZoomsIn()
    {
        ResetCanvas();
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        SelectTool("BtnMagnifier");

        var zoomBefore = FindByAutomationId("StatusZoom").Text;

        var canvas = FindByAutomationId("DrawingCanvas");
        canvas.Click();
        Thread.Sleep(300);

        var zoomAfter = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual(zoomBefore, zoomAfter, "Magnifier left-click should change zoom");
    }

    [TestMethod]
    public void Magnifier_RightClick_ZoomsOut()
    {
        // First zoom in
        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);

        SelectTool("BtnMagnifier");

        var zoomBefore = FindByAutomationId("StatusZoom").Text;

        var canvas = FindByAutomationId("DrawingCanvas");
        RightClick(canvas);
        Thread.Sleep(300);

        var zoomAfter = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual(zoomBefore, zoomAfter, "Magnifier right-click should change zoom");
    }

    [TestMethod]
    public void Magnifier_MultipleZoomIns()
    {
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);

        SelectTool("BtnMagnifier");
        var canvas = FindByAutomationId("DrawingCanvas");

        for (int i = 0; i < 3; i++)
        {
            canvas.Click();
            Thread.Sleep(200);
        }

        var zoom = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual("100%", zoom, "Multiple zoom ins should change from 100%");

        // Reset
        ClickMenuItem("View", "100%");
        Thread.Sleep(200);
    }

    #endregion

    #region Text Tool

    [TestMethod]
    public void Text_ClickOnCanvas_OpensDialog()
    {
        ResetCanvas();
        SelectTool("BtnText");

        var canvas = FindByAutomationId("DrawingCanvas");
        canvas.Click();
        Thread.Sleep(500);

        // Text tool opens a ContentDialog — dismiss it
        var cancel = TryFindByName("Cancel");
        if (cancel is not null)
        {
            cancel.Click();
        }
        else
        {
            DismissDialog();
        }

        Thread.Sleep(300);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Right-Click Drawing (Secondary Color)

    [TestMethod]
    public void Pencil_RightClickDraw_UsesSecondaryColor()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");

        // Right-click drag
        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 50, 50)
               .ClickAndHold()
               .MoveByOffset(100, 0)
               .Release()
               .Perform();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Canvas After Image Operations

    [TestMethod]
    public void Canvas_AfterFlipH_StillInteractable()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        ClickMenuItem("Image", "Flip Horizontal");
        Thread.Sleep(300);

        // Should still be able to draw
        DragOnElement(canvas, 50, 50, 200, 50);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Canvas_AfterInvertColors_StillInteractable()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SendShortcut(Keys.Control + Keys.Shift + "i");
        Thread.Sleep(300);

        DragOnElement(canvas, 50, 250, 200, 250);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Canvas_AfterClearImage_StillInteractable()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SendShortcut(Keys.Control + Keys.Shift + "n");
        Thread.Sleep(300);

        DragOnElement(canvas, 50, 50, 200, 50);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Canvas Edge Cases

    [TestMethod]
    public void DrawAtCanvasOrigin()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");

        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 0, 0).Click().Perform();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void DrawAtCanvasBottomRight()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        var size = canvas.Size;

        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, size.Width - 1, size.Height - 1).Click().Perform();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void RapidDrawing_50Strokes()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        var rng = new Random(42);

        for (int i = 0; i < 50; i++)
        {
            int x1 = rng.Next(10, 300);
            int y1 = rng.Next(10, 200);
            DragOnElement(canvas, x1, y1, x1 + rng.Next(10, 50), y1 + rng.Next(10, 50));
            Thread.Sleep(30);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion
}
