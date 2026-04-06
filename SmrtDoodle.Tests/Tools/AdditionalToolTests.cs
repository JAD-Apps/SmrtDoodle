using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Tools;
using SmrtDoodle.Models;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tests.Tools;

/// <summary>
/// Tests for tools added in Phase 4: Gradient, Blur, Sharpen, Smudge,
/// CloneStamp, PatternFill, Measure. Validates properties, defaults,
/// and state management without requiring a CanvasDevice.
/// </summary>
[TestClass]
public class AdditionalToolTests
{
    #region GradientTool

    [TestMethod]
    public void GradientTool_Name_IsGradient()
    {
        var tool = new GradientTool();
        Assert.AreEqual("Gradient", tool.Name);
    }

    [TestMethod]
    public void GradientTool_HasIcon()
    {
        var tool = new GradientTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void GradientTool_DefaultMode_IsLinear()
    {
        var tool = new GradientTool();
        Assert.AreEqual(GradientType.Linear, tool.GradientMode);
    }

    [TestMethod]
    public void GradientTool_CanChangeMode()
    {
        var tool = new GradientTool();
        tool.GradientMode = GradientType.Radial;
        Assert.AreEqual(GradientType.Radial, tool.GradientMode);
    }

    [DataTestMethod]
    [DataRow(GradientType.Linear)]
    [DataRow(GradientType.Radial)]
    [DataRow(GradientType.Angle)]
    [DataRow(GradientType.Reflected)]
    [DataRow(GradientType.Diamond)]
    public void GradientTool_AllGradientTypes_Valid(GradientType type)
    {
        var tool = new GradientTool { GradientMode = type };
        Assert.AreEqual(type, tool.GradientMode);
    }

    [TestMethod]
    public void GradientTool_SecondaryColor_DefaultWhite()
    {
        var tool = new GradientTool();
        Assert.AreEqual(Color.FromArgb(255, 255, 255, 255), tool.SecondaryColor);
    }

    [TestMethod]
    public void GradientTool_CanSetSecondaryColor()
    {
        var tool = new GradientTool();
        var red = Color.FromArgb(255, 255, 0, 0);
        tool.SecondaryColor = red;
        Assert.AreEqual(red, tool.SecondaryColor);
    }

    #endregion

    #region BlurTool

    [TestMethod]
    public void BlurTool_Name_IsBlur()
    {
        var tool = new BlurTool();
        Assert.AreEqual("Blur", tool.Name);
    }

    [TestMethod]
    public void BlurTool_HasIcon()
    {
        var tool = new BlurTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void BlurTool_DefaultStrength_Is3()
    {
        var tool = new BlurTool();
        Assert.AreEqual(3, tool.Strength);
    }

    [TestMethod]
    public void BlurTool_CanChangeStrength()
    {
        var tool = new BlurTool();
        tool.Strength = 7;
        Assert.AreEqual(7, tool.Strength);
    }

    #endregion

    #region SharpenTool

    [TestMethod]
    public void SharpenTool_Name_IsSharpen()
    {
        var tool = new SharpenTool();
        Assert.AreEqual("Sharpen", tool.Name);
    }

    [TestMethod]
    public void SharpenTool_HasIcon()
    {
        var tool = new SharpenTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void SharpenTool_DefaultStrength()
    {
        var tool = new SharpenTool();
        Assert.AreEqual(0.5f, tool.Strength);
    }

    [TestMethod]
    public void SharpenTool_CanChangeStrength()
    {
        var tool = new SharpenTool();
        tool.Strength = 0.8f;
        Assert.AreEqual(0.8f, tool.Strength);
    }

    #endregion

    #region SmudgeTool

    [TestMethod]
    public void SmudgeTool_Name_IsSmudge()
    {
        var tool = new SmudgeTool();
        Assert.AreEqual("Smudge", tool.Name);
    }

    [TestMethod]
    public void SmudgeTool_HasIcon()
    {
        var tool = new SmudgeTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void SmudgeTool_DefaultStrength()
    {
        var tool = new SmudgeTool();
        Assert.AreEqual(0.5f, tool.Strength);
    }

    #endregion

    #region CloneStampTool

    [TestMethod]
    public void CloneStampTool_Name_IsCloneStamp()
    {
        var tool = new CloneStampTool();
        Assert.AreEqual("Clone Stamp", tool.Name);
    }

    [TestMethod]
    public void CloneStampTool_HasIcon()
    {
        var tool = new CloneStampTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void CloneStampTool_InitiallyNoSource()
    {
        var tool = new CloneStampTool();
        Assert.IsFalse(tool.IsSourceSet);
    }

    [TestMethod]
    public void CloneStampTool_SetSource_MarksSourceSet()
    {
        var tool = new CloneStampTool();
        tool.SetSource(new Vector2(100, 200));
        Assert.IsTrue(tool.IsSourceSet);
        Assert.AreEqual(new Vector2(100, 200), tool.SourcePoint);
    }

    [TestMethod]
    public void CloneStampTool_SetSource_UpdatesPoint()
    {
        var tool = new CloneStampTool();
        tool.SetSource(new Vector2(50, 50));
        tool.SetSource(new Vector2(200, 300));
        Assert.AreEqual(new Vector2(200, 300), tool.SourcePoint);
    }

    #endregion

    #region PatternFillTool

    [TestMethod]
    public void PatternFillTool_Name_IsPatternFill()
    {
        var tool = new PatternFillTool();
        Assert.AreEqual("Pattern Fill", tool.Name);
    }

    [TestMethod]
    public void PatternFillTool_HasIcon()
    {
        var tool = new PatternFillTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void PatternFillTool_DefaultPattern_IsCheckerboard()
    {
        var tool = new PatternFillTool();
        Assert.AreEqual(PatternType.Checkerboard, tool.Pattern);
    }

    [TestMethod]
    public void PatternFillTool_DefaultTileSize_Is16()
    {
        var tool = new PatternFillTool();
        Assert.AreEqual(16, tool.TileSize);
    }

    [TestMethod]
    public void PatternFillTool_CanChangePattern()
    {
        var tool = new PatternFillTool();
        tool.Pattern = PatternType.DiagonalLines;
        Assert.AreEqual(PatternType.DiagonalLines, tool.Pattern);
    }

    [TestMethod]
    public void PatternFillTool_CanChangeTileSize()
    {
        var tool = new PatternFillTool();
        tool.TileSize = 32;
        Assert.AreEqual(32, tool.TileSize);
    }

    [DataTestMethod]
    [DataRow(PatternType.Checkerboard)]
    [DataRow(PatternType.DiagonalLines)]
    [DataRow(PatternType.Dots)]
    [DataRow(PatternType.Crosshatch)]
    [DataRow(PatternType.Brick)]
    public void PatternFillTool_AllPatternTypes_Valid(PatternType type)
    {
        var tool = new PatternFillTool { Pattern = type };
        Assert.AreEqual(type, tool.Pattern);
    }

    #endregion

    #region MeasureTool

    [TestMethod]
    public void MeasureTool_Name_IsMeasure()
    {
        var tool = new MeasureTool();
        Assert.AreEqual("Measure", tool.Name);
    }

    [TestMethod]
    public void MeasureTool_HasIcon()
    {
        var tool = new MeasureTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    [TestMethod]
    public void MeasureTool_InitialDistance_IsZero()
    {
        var tool = new MeasureTool();
        Assert.AreEqual(0f, tool.Distance);
    }

    [TestMethod]
    public void MeasureTool_InitialAngle_IsZero()
    {
        var tool = new MeasureTool();
        Assert.AreEqual(0f, tool.Angle);
    }

    [TestMethod]
    public void MeasureTool_InitialDeltas_AreZero()
    {
        var tool = new MeasureTool();
        Assert.AreEqual(0f, tool.DeltaX);
        Assert.AreEqual(0f, tool.DeltaY);
    }

    [TestMethod]
    public void MeasureTool_GetStatusText_DefaultPrompt()
    {
        var tool = new MeasureTool();
        var text = tool.GetStatusText();
        Assert.IsTrue(text.Contains("Measure"),
            "Default status text should contain 'Measure'");
    }

    #endregion

    #region CurveTool

    [TestMethod]
    public void CurveTool_Name_IsCurve()
    {
        var tool = new CurveTool();
        Assert.AreEqual("Curve", tool.Name);
    }

    [TestMethod]
    public void CurveTool_HasIcon()
    {
        var tool = new CurveTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    #endregion

    #region MagnifierTool

    [TestMethod]
    public void MagnifierTool_Name_IsMagnifier()
    {
        var tool = new MagnifierTool();
        Assert.AreEqual("Magnifier", tool.Name);
    }

    [TestMethod]
    public void MagnifierTool_HasIcon()
    {
        var tool = new MagnifierTool();
        Assert.IsFalse(string.IsNullOrEmpty(tool.Icon));
    }

    #endregion

    #region AllTools_ImplementITool

    [TestMethod]
    public void AllPhase4Tools_ImplementITool()
    {
        ITool[] tools =
        {
            new GradientTool(),
            new BlurTool(),
            new SharpenTool(),
            new SmudgeTool(),
            new CloneStampTool(),
            new PatternFillTool(),
            new MeasureTool(),
        };

        foreach (var tool in tools)
        {
            Assert.IsNotNull(tool.Name, $"{tool.GetType().Name} has null name");
            Assert.IsNotNull(tool.Icon, $"{tool.GetType().Name} has null icon");
        }
    }

    #endregion

    #region DrawingTool Enum

    [TestMethod]
    public void DrawingTool_Contains_AllExpectedValues()
    {
        var values = Enum.GetValues<DrawingTool>();
        Assert.IsTrue(values.Length >= 19, $"Expected >= 19 DrawingTool values, got {values.Length}");

        // Spot check key values
        Assert.IsTrue(Enum.IsDefined(typeof(DrawingTool), DrawingTool.Pencil));
        Assert.IsTrue(Enum.IsDefined(typeof(DrawingTool), DrawingTool.Gradient));
        Assert.IsTrue(Enum.IsDefined(typeof(DrawingTool), DrawingTool.Blur));
        Assert.IsTrue(Enum.IsDefined(typeof(DrawingTool), DrawingTool.Sharpen));
        Assert.IsTrue(Enum.IsDefined(typeof(DrawingTool), DrawingTool.Smudge));
        Assert.IsTrue(Enum.IsDefined(typeof(DrawingTool), DrawingTool.CloneStamp));
        Assert.IsTrue(Enum.IsDefined(typeof(DrawingTool), DrawingTool.PatternFill));
        Assert.IsTrue(Enum.IsDefined(typeof(DrawingTool), DrawingTool.Measure));
    }

    #endregion

    #region BlendMode Enum

    [TestMethod]
    public void BlendMode_Contains25Modes()
    {
        var values = Enum.GetValues<BlendMode>();
        Assert.IsTrue(values.Length >= 25, $"Expected >= 25 BlendMode values, got {values.Length}");
    }

    [TestMethod]
    public void BlendMode_ContainsAllPhotoshopModes()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Normal));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Dissolve));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Multiply));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Screen));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Overlay));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.SoftLight));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.HardLight));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Difference));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Exclusion));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Hue));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Saturation));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Color));
        Assert.IsTrue(Enum.IsDefined(typeof(BlendMode), BlendMode.Luminosity));
    }

    #endregion

    #region ShapeType Enum

    [TestMethod]
    public void ShapeType_Contains12Shapes()
    {
        var values = Enum.GetValues<ShapeType>();
        Assert.IsTrue(values.Length >= 12, $"Expected >= 12 ShapeType values, got {values.Length}");
    }

    #endregion

    #region BrushStyle Enum

    [TestMethod]
    public void BrushStyle_Contains8Styles()
    {
        var values = Enum.GetValues<BrushStyle>();
        Assert.IsTrue(values.Length >= 8, $"Expected >= 8 BrushStyle values, got {values.Length}");
    }

    #endregion
}
