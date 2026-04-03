using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Tools;
using SmrtDoodle.Models;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tests;

[TestClass]
public class ToolTests
{
    [TestMethod]
    public void PencilTool_HasCorrectName()
    {
        var tool = new PencilTool();
        Assert.AreEqual("Pencil", tool.Name);
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void BrushTool_HasCorrectName()
    {
        var tool = new BrushTool();
        Assert.AreEqual("Brush", tool.Name);
    }

    [TestMethod]
    public void EraserTool_HasCorrectName()
    {
        var tool = new EraserTool();
        Assert.AreEqual("Eraser", tool.Name);
    }

    [TestMethod]
    public void LineTool_HasCorrectName()
    {
        var tool = new LineTool();
        Assert.AreEqual("Line", tool.Name);
    }

    [TestMethod]
    public void FillTool_HasCorrectName()
    {
        var tool = new FillTool();
        Assert.AreEqual("Fill", tool.Name);
    }

    [TestMethod]
    public void TextTool_HasCorrectName()
    {
        var tool = new TextTool();
        Assert.AreEqual("Text", tool.Name);
    }

    [TestMethod]
    public void EyedropperTool_HasCorrectName()
    {
        var tool = new EyedropperTool();
        Assert.AreEqual("Color Picker", tool.Name);
    }

    [TestMethod]
    public void ShapeTool_HasCorrectName()
    {
        var tool = new ShapeTool();
        Assert.AreEqual("Shape", tool.Name);
    }

    [TestMethod]
    public void SelectionTool_HasCorrectName()
    {
        var tool = new SelectionTool();
        Assert.AreEqual("Select", tool.Name);
    }

    [TestMethod]
    public void ShapeTool_DefaultShapeIsRectangle()
    {
        var tool = new ShapeTool();
        Assert.AreEqual(ShapeType.Rectangle, tool.CurrentShapeType);
        Assert.AreEqual(ShapeFillMode.Outline, tool.FillMode);
        Assert.IsFalse(tool.Filled);
    }

    [TestMethod]
    public void ShapeTool_CanChangeShapeType()
    {
        var tool = new ShapeTool();
        tool.CurrentShapeType = ShapeType.Ellipse;
        tool.Filled = true;
        Assert.AreEqual(ShapeType.Ellipse, tool.CurrentShapeType);
        Assert.IsTrue(tool.Filled);
    }

    [TestMethod]
    public void ShapeTool_FillModeOutlineAndFill()
    {
        var tool = new ShapeTool();
        tool.FillMode = ShapeFillMode.OutlineAndFill;
        Assert.AreEqual(ShapeFillMode.OutlineAndFill, tool.FillMode);
        Assert.IsTrue(tool.Filled);
    }

    [TestMethod]
    public void ShapeTool_FilledPropertyMapsFillMode()
    {
        var tool = new ShapeTool();
        tool.Filled = false;
        Assert.AreEqual(ShapeFillMode.Outline, tool.FillMode);
        tool.Filled = true;
        Assert.AreEqual(ShapeFillMode.Fill, tool.FillMode);
    }

    [TestMethod]
    public void SelectionTool_SelectionRectIsSettable()
    {
        var tool = new SelectionTool();
        var rect = new Windows.Foundation.Rect(10, 20, 100, 200);
        tool.SelectionRect = rect;
        Assert.AreEqual(rect, tool.SelectionRect);
    }

    [TestMethod]
    public void SelectionTool_HasFloatingSelection_DefaultFalse()
    {
        var tool = new SelectionTool();
        Assert.IsFalse(tool.HasFloatingSelection);
    }

    [TestMethod]
    public void SelectionTool_InitialModeIsNone()
    {
        var tool = new SelectionTool();
        Assert.AreEqual(SelectionMode.None, tool.Mode);
    }

    [TestMethod]
    public void SelectionTool_ResetClearsState()
    {
        var tool = new SelectionTool();
        tool.Mode = SelectionMode.Selecting;
        tool.Reset();
        Assert.AreEqual(SelectionMode.None, tool.Mode);
    }

    [TestMethod]
    public void ShapeTool_NormalizeRect_HandlesNegativeDirection()
    {
        var a = new Vector2(100, 100);
        var b = new Vector2(50, 50);
        var rect = ShapeTool.NormalizeRect(a, b);
        Assert.AreEqual(50, rect.X);
        Assert.AreEqual(50, rect.Y);
        Assert.AreEqual(50, rect.Width);
        Assert.AreEqual(50, rect.Height);
    }

    [TestMethod]
    public void ShapeTool_NormalizeRect_PositiveDirection()
    {
        var a = new Vector2(10, 20);
        var b = new Vector2(110, 120);
        var rect = ShapeTool.NormalizeRect(a, b);
        Assert.AreEqual(10, rect.X);
        Assert.AreEqual(20, rect.Y);
        Assert.AreEqual(100, rect.Width);
        Assert.AreEqual(100, rect.Height);
    }

    [TestMethod]
    public void ShapeTool_NormalizeRect_SamePoint()
    {
        var p = new Vector2(50, 50);
        var rect = ShapeTool.NormalizeRect(p, p);
        Assert.AreEqual(0, rect.Width);
        Assert.AreEqual(0, rect.Height);
    }

    [TestMethod]
    public void CurveTool_HasCorrectName()
    {
        var tool = new CurveTool();
        Assert.AreEqual("Curve", tool.Name);
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void FreeFormSelectionTool_HasCorrectName()
    {
        var tool = new FreeFormSelectionTool();
        Assert.AreEqual("Free-Form Select", tool.Name);
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void FreeFormSelectionTool_InitialStateHasNoSelection()
    {
        var tool = new FreeFormSelectionTool();
        Assert.IsFalse(tool.HasSelection);
        Assert.AreEqual(0, tool.Points.Count);
    }

    [TestMethod]
    public void FreeFormSelectionTool_ResetClearsState()
    {
        var tool = new FreeFormSelectionTool();
        tool.Reset();
        Assert.IsFalse(tool.HasSelection);
        Assert.AreEqual(0, tool.Points.Count);
    }

    [TestMethod]
    public void AllTools_ImplementITool()
    {
        ITool[] tools =
        {
            new PencilTool(),
            new BrushTool(),
            new EraserTool(),
            new LineTool(),
            new FillTool(),
            new TextTool(),
            new EyedropperTool(),
            new ShapeTool(),
            new SelectionTool(),
            new CurveTool(),
            new FreeFormSelectionTool()
        };

        foreach (var tool in tools)
        {
            Assert.IsNotNull(tool.Name, $"{tool.GetType().Name} has null name");
            Assert.IsNotNull(tool.Icon, $"{tool.GetType().Name} has null icon");
        }
    }

    [TestMethod]
    public void LineTool_TracksStartAndEndPoints()
    {
        var tool = new LineTool();
        Assert.AreEqual(Vector2.Zero, tool.StartPoint);
        Assert.AreEqual(Vector2.Zero, tool.EndPoint);
    }

    [TestMethod]
    public void TextTool_TracksInsertionPoint()
    {
        var tool = new TextTool();
        Assert.AreEqual(Vector2.Zero, tool.InsertionPoint);
    }

    [TestMethod]
    public void EyedropperTool_PickedColorIsNull()
    {
        var tool = new EyedropperTool();
        Assert.IsNull(tool.PickedColor);
    }

    [TestMethod]
    public void EyedropperTool_CanSetPickedColor()
    {
        var tool = new EyedropperTool();
        var red = Color.FromArgb(255, 255, 0, 0);
        tool.PickedColor = red;
        Assert.AreEqual(red, tool.PickedColor);
    }

    [TestMethod]
    public void ToolBase_ResetClearsDrawingState()
    {
        var tool = new PencilTool();
        tool.Reset();
        // After reset, calling OnPointerMoved should be a no-op (not drawing)
    }
}
