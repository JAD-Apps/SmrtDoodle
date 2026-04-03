using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;

namespace SmrtDoodle.Tests;

[TestClass]
public class CanvasSettingsTests
{
    [TestMethod]
    public void DefaultSettings_AreCorrect()
    {
        var settings = new CanvasSettings();
        Assert.AreEqual(800, settings.Width);
        Assert.AreEqual(600, settings.Height);
        Assert.AreEqual(72f, settings.Dpi);
        Assert.IsFalse(settings.ShowGrid);
        Assert.IsFalse(settings.ShowRuler);
        Assert.AreEqual(20, settings.GridSpacing);
    }

    [TestMethod]
    public void DefaultSettings_WhiteBackground()
    {
        var settings = new CanvasSettings();
        Assert.AreEqual(255, settings.BackgroundColor.R);
        Assert.AreEqual(255, settings.BackgroundColor.G);
        Assert.AreEqual(255, settings.BackgroundColor.B);
        Assert.AreEqual(255, settings.BackgroundColor.A);
    }

    [TestMethod]
    public void Settings_CanBeModified()
    {
        var settings = new CanvasSettings
        {
            Width = 1920,
            Height = 1080,
            Dpi = 300f,
            ShowGrid = true,
            ShowRuler = true,
            GridSpacing = 50
        };
        Assert.AreEqual(1920, settings.Width);
        Assert.AreEqual(1080, settings.Height);
        Assert.AreEqual(300f, settings.Dpi);
        Assert.IsTrue(settings.ShowGrid);
        Assert.IsTrue(settings.ShowRuler);
        Assert.AreEqual(50, settings.GridSpacing);
    }
}

[TestClass]
public class EnumTests
{
    [TestMethod]
    public void DrawingTool_HasAllExpectedValues()
    {
        var values = Enum.GetValues<DrawingTool>();
        Assert.IsTrue(values.Contains(DrawingTool.Pencil));
        Assert.IsTrue(values.Contains(DrawingTool.Brush));
        Assert.IsTrue(values.Contains(DrawingTool.Eraser));
        Assert.IsTrue(values.Contains(DrawingTool.Fill));
        Assert.IsTrue(values.Contains(DrawingTool.Text));
        Assert.IsTrue(values.Contains(DrawingTool.Eyedropper));
        Assert.IsTrue(values.Contains(DrawingTool.Line));
        Assert.IsTrue(values.Contains(DrawingTool.Curve));
        Assert.IsTrue(values.Contains(DrawingTool.Shape));
        Assert.IsTrue(values.Contains(DrawingTool.Selection));
        Assert.IsTrue(values.Contains(DrawingTool.FreeFormSelection));
        Assert.IsTrue(values.Contains(DrawingTool.Crop));
        Assert.IsTrue(values.Contains(DrawingTool.Magnifier));
    }

    [TestMethod]
    public void ShapeType_HasAllExpectedValues()
    {
        var values = Enum.GetValues<ShapeType>();
        Assert.IsTrue(values.Length >= 12);
        Assert.IsTrue(values.Contains(ShapeType.Rectangle));
        Assert.IsTrue(values.Contains(ShapeType.Ellipse));
        Assert.IsTrue(values.Contains(ShapeType.Triangle));
        Assert.IsTrue(values.Contains(ShapeType.Star));
        Assert.IsTrue(values.Contains(ShapeType.Arrow));
        Assert.IsTrue(values.Contains(ShapeType.Heart));
        Assert.IsTrue(values.Contains(ShapeType.Lightning));
    }

    [TestMethod]
    public void ShapeFillMode_HasExpectedValues()
    {
        var values = Enum.GetValues<ShapeFillMode>();
        Assert.AreEqual(3, values.Length);
        Assert.AreEqual(0, (int)ShapeFillMode.Outline);
        Assert.AreEqual(1, (int)ShapeFillMode.Fill);
        Assert.AreEqual(2, (int)ShapeFillMode.OutlineAndFill);
    }

    [TestMethod]
    public void SelectionMode_HasExpectedValues()
    {
        Assert.AreEqual(0, (int)SelectionMode.None);
        Assert.AreEqual(1, (int)SelectionMode.Selecting);
        Assert.AreEqual(2, (int)SelectionMode.Moving);
    }

    [TestMethod]
    public void BlendMode_HasExpectedValues()
    {
        var values = Enum.GetValues<BlendMode>();
        Assert.AreEqual(4, values.Length);
    }

    [TestMethod]
    public void BrushStyle_HasAllExpectedValues()
    {
        var values = Enum.GetValues<BrushStyle>();
        Assert.AreEqual(8, values.Length);
        Assert.AreEqual(0, (int)BrushStyle.Normal);
        Assert.AreEqual(1, (int)BrushStyle.Calligraphy);
        Assert.AreEqual(2, (int)BrushStyle.Airbrush);
        Assert.AreEqual(3, (int)BrushStyle.Oil);
        Assert.AreEqual(4, (int)BrushStyle.Crayon);
        Assert.AreEqual(5, (int)BrushStyle.Marker);
        Assert.AreEqual(6, (int)BrushStyle.NaturalPencil);
        Assert.AreEqual(7, (int)BrushStyle.Watercolor);
    }
}
