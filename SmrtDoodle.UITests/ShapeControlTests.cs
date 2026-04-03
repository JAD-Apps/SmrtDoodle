using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for the Shapes ribbon group controls:
/// ShapeTypeCombo (12 shapes) and FillModeCombo (3 fill modes).
/// </summary>
[TestClass]
public class ShapeControlTests : AppiumTestBase
{
    private static readonly string[] ShapeTypes =
    [
        "Rectangle", "Ellipse", "Rounded Rect", "Triangle", "Right Triangle",
        "Diamond", "Pentagon", "Hexagon", "Arrow", "Star", "Heart", "Lightning"
    ];

    private static readonly string[] FillModes = ["Outline", "Fill", "Outline + Fill"];

    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region ShapeTypeCombo — Existence & Default

    [TestMethod]
    public void ShapeTypeCombo_Exists()
    {
        var combo = FindByAutomationId("ShapeTypeCombo");
        Assert.IsNotNull(combo);
        Assert.IsTrue(combo.Displayed);
    }

    [TestMethod]
    public void ShapeTypeCombo_IsEnabled()
    {
        var combo = FindByAutomationId("ShapeTypeCombo");
        Assert.IsTrue(combo.Enabled);
    }

    [TestMethod]
    public void ShapeTypeCombo_DefaultIsRectangle()
    {
        ResetCanvas();
        var combo = FindByAutomationId("ShapeTypeCombo");
        Assert.AreEqual("Rectangle", combo.Text, "Default shape type should be Rectangle");
    }

    #endregion

    #region ShapeTypeCombo — All 12 Shapes Selection

    [TestMethod]
    public void ShapeTypeCombo_SelectRectangle()
    {
        SelectShapeType("Rectangle");
        Assert.AreEqual("Rectangle", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectEllipse()
    {
        SelectShapeType("Ellipse");
        Assert.AreEqual("Ellipse", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectRoundedRect()
    {
        SelectShapeType("Rounded Rect");
        Assert.AreEqual("Rounded Rect", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectTriangle()
    {
        SelectShapeType("Triangle");
        Assert.AreEqual("Triangle", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectRightTriangle()
    {
        SelectShapeType("Right Triangle");
        Assert.AreEqual("Right Triangle", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectDiamond()
    {
        SelectShapeType("Diamond");
        Assert.AreEqual("Diamond", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectPentagon()
    {
        SelectShapeType("Pentagon");
        Assert.AreEqual("Pentagon", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectHexagon()
    {
        SelectShapeType("Hexagon");
        Assert.AreEqual("Hexagon", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectArrow()
    {
        SelectShapeType("Arrow");
        Assert.AreEqual("Arrow", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectStar()
    {
        SelectShapeType("Star");
        Assert.AreEqual("Star", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectHeart()
    {
        SelectShapeType("Heart");
        Assert.AreEqual("Heart", FindByAutomationId("ShapeTypeCombo").Text);
    }

    [TestMethod]
    public void ShapeTypeCombo_SelectLightning()
    {
        SelectShapeType("Lightning");
        Assert.AreEqual("Lightning", FindByAutomationId("ShapeTypeCombo").Text);
    }

    #endregion

    #region ShapeTypeCombo — Cycling

    [TestMethod]
    public void ShapeTypeCombo_CycleThroughAll12Shapes()
    {
        foreach (var shape in ShapeTypes)
        {
            SelectShapeType(shape);
            Thread.Sleep(100);
            Assert.AreEqual(shape, FindByAutomationId("ShapeTypeCombo").Text, $"Expected shape '{shape}'");
        }
    }

    [TestMethod]
    public void ShapeTypeCombo_CycleBackward()
    {
        for (int i = ShapeTypes.Length - 1; i >= 0; i--)
        {
            SelectShapeType(ShapeTypes[i]);
            Thread.Sleep(50);
        }

        Assert.AreEqual("Rectangle", FindByAutomationId("ShapeTypeCombo").Text);
    }

    #endregion

    #region FillModeCombo — Existence & Default

    [TestMethod]
    public void FillModeCombo_Exists()
    {
        var combo = FindByAutomationId("FillModeCombo");
        Assert.IsNotNull(combo);
        Assert.IsTrue(combo.Displayed);
    }

    [TestMethod]
    public void FillModeCombo_IsEnabled()
    {
        var combo = FindByAutomationId("FillModeCombo");
        Assert.IsTrue(combo.Enabled);
    }

    [TestMethod]
    public void FillModeCombo_DefaultIsOutline()
    {
        ResetCanvas();
        var combo = FindByAutomationId("FillModeCombo");
        Assert.AreEqual("Outline", combo.Text, "Default fill mode should be Outline");
    }

    #endregion

    #region FillModeCombo — All 3 Modes Selection

    [TestMethod]
    public void FillModeCombo_SelectOutline()
    {
        SelectFillMode("Outline");
        Assert.AreEqual("Outline", FindByAutomationId("FillModeCombo").Text);
    }

    [TestMethod]
    public void FillModeCombo_SelectFill()
    {
        SelectFillMode("Fill");
        Assert.AreEqual("Fill", FindByAutomationId("FillModeCombo").Text);
    }

    [TestMethod]
    public void FillModeCombo_SelectOutlinePlusFill()
    {
        SelectFillMode("Outline + Fill");
        Assert.AreEqual("Outline + Fill", FindByAutomationId("FillModeCombo").Text);
    }

    [TestMethod]
    public void FillModeCombo_CycleThroughAll3Modes()
    {
        foreach (var mode in FillModes)
        {
            SelectFillMode(mode);
            Thread.Sleep(100);
            Assert.AreEqual(mode, FindByAutomationId("FillModeCombo").Text);
        }
    }

    #endregion

    #region Shape Drawing — All Shapes × All Fill Modes

    [TestMethod]
    public void DrawRectangle_Outline_DoesNotCrash()
    {
        DrawShapeWithMode("Rectangle", "Outline");
    }

    [TestMethod]
    public void DrawRectangle_Fill_DoesNotCrash()
    {
        DrawShapeWithMode("Rectangle", "Fill");
    }

    [TestMethod]
    public void DrawRectangle_OutlinePlusFill_DoesNotCrash()
    {
        DrawShapeWithMode("Rectangle", "Outline + Fill");
    }

    [TestMethod]
    public void DrawEllipse_Outline_DoesNotCrash()
    {
        DrawShapeWithMode("Ellipse", "Outline");
    }

    [TestMethod]
    public void DrawEllipse_Fill_DoesNotCrash()
    {
        DrawShapeWithMode("Ellipse", "Fill");
    }

    [TestMethod]
    public void DrawTriangle_Outline_DoesNotCrash()
    {
        DrawShapeWithMode("Triangle", "Outline");
    }

    [TestMethod]
    public void DrawStar_Fill_DoesNotCrash()
    {
        DrawShapeWithMode("Star", "Fill");
    }

    [TestMethod]
    public void DrawHeart_OutlinePlusFill_DoesNotCrash()
    {
        DrawShapeWithMode("Heart", "Outline + Fill");
    }

    [TestMethod]
    public void DrawLightning_Fill_DoesNotCrash()
    {
        DrawShapeWithMode("Lightning", "Fill");
    }

    [TestMethod]
    public void DrawDiamond_Outline_DoesNotCrash()
    {
        DrawShapeWithMode("Diamond", "Outline");
    }

    [TestMethod]
    public void DrawPentagon_Fill_DoesNotCrash()
    {
        DrawShapeWithMode("Pentagon", "Fill");
    }

    [TestMethod]
    public void DrawHexagon_OutlinePlusFill_DoesNotCrash()
    {
        DrawShapeWithMode("Hexagon", "Outline + Fill");
    }

    [TestMethod]
    public void DrawArrow_Outline_DoesNotCrash()
    {
        DrawShapeWithMode("Arrow", "Outline");
    }

    [TestMethod]
    public void DrawRoundedRect_Fill_DoesNotCrash()
    {
        DrawShapeWithMode("Rounded Rect", "Fill");
    }

    [TestMethod]
    public void DrawRightTriangle_OutlinePlusFill_DoesNotCrash()
    {
        DrawShapeWithMode("Right Triangle", "Outline + Fill");
    }

    #endregion

    #region Shape Combos — Persistence

    [TestMethod]
    public void ShapeType_PersistsAcrossToolSwitches()
    {
        SelectShapeType("Star");
        SelectTool("BtnPencil");
        Thread.Sleep(100);
        SelectTool("BtnShape");
        Thread.Sleep(100);

        Assert.AreEqual("Star", FindByAutomationId("ShapeTypeCombo").Text, "Shape type should persist");
    }

    [TestMethod]
    public void FillMode_PersistsAcrossToolSwitches()
    {
        SelectFillMode("Outline + Fill");
        SelectTool("BtnEraser");
        Thread.Sleep(100);
        SelectTool("BtnShape");
        Thread.Sleep(100);

        Assert.AreEqual("Outline + Fill", FindByAutomationId("FillModeCombo").Text, "Fill mode should persist");
    }

    #endregion

    #region Comprehensive Shape Drawing

    [TestMethod]
    public void DrawAllShapes_WithOutline()
    {
        SelectTool("BtnShape");
        SelectFillMode("Outline");

        var canvas = FindByAutomationId("DrawingCanvas");
        int yOffset = 30;

        foreach (var shape in ShapeTypes)
        {
            SelectShapeType(shape);
            Thread.Sleep(100);
            DragOnElement(canvas, 30, yOffset, 130, yOffset + 30);
            yOffset += 40;
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void DrawAllShapes_WithFill()
    {
        ResetCanvas();
        SelectTool("BtnShape");
        SelectFillMode("Fill");

        var canvas = FindByAutomationId("DrawingCanvas");
        int yOffset = 30;

        foreach (var shape in ShapeTypes)
        {
            SelectShapeType(shape);
            Thread.Sleep(100);
            DragOnElement(canvas, 30, yOffset, 130, yOffset + 30);
            yOffset += 40;
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Helpers

    private static void SelectShapeType(string shapeName)
    {
        var combo = FindByAutomationId("ShapeTypeCombo");
        combo.Click();
        Thread.Sleep(300);

        var item = FindByName(shapeName);
        item.Click();
        Thread.Sleep(200);
    }

    private static void SelectFillMode(string modeName)
    {
        var combo = FindByAutomationId("FillModeCombo");
        combo.Click();
        Thread.Sleep(300);

        var item = FindByName(modeName);
        item.Click();
        Thread.Sleep(200);
    }

    private static void DrawShapeWithMode(string shape, string fillMode)
    {
        SelectTool("BtnShape");
        SelectShapeType(shape);
        SelectFillMode(fillMode);
        Thread.Sleep(100);

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"), $"Canvas should exist after drawing {shape} with {fillMode}");
    }

    #endregion
}
