using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;

namespace SmrtDoodle.Tests.UI;

/// <summary>
/// Tests verifying accessibility attributes in MainWindow.xaml:
/// AccessKey, AutomationProperties, LandmarkType, LiveSetting.
/// </summary>
[TestClass]
public class AccessibilityTests
{
    private static XDocument? _xamlDoc;
    private static XNamespace _defaultNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static XNamespace _xNs = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static XNamespace _autoNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    [ClassInitialize]
    public static void LoadXaml(TestContext _)
    {
        var dir = AppContext.BaseDirectory;
        string? xamlPath = null;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "SmrtDoodle", "MainWindow.xaml");
            if (File.Exists(candidate))
            {
                xamlPath = candidate;
                break;
            }
            dir = Path.GetDirectoryName(dir);
        }
        Assert.IsNotNull(xamlPath, "Could not find MainWindow.xaml");
        _xamlDoc = XDocument.Load(xamlPath);
    }

    private XElement GetByName(string name) =>
        _xamlDoc!.Descendants()
            .First(e => e.Attribute(_xNs + "Name")?.Value == name);

    private string? GetAutoProp(XElement el, string propName) =>
        el.Attribute(_defaultNs + $"AutomationProperties.{propName}")?.Value
        ?? el.Attribute($"AutomationProperties.{propName}")?.Value;

    #region AccessKey on All Tool Buttons

    [DataTestMethod]
    [DataRow("BtnPencil", "P")]
    [DataRow("BtnBrush", "B")]
    [DataRow("BtnEraser", "E")]
    [DataRow("BtnFill", "G")]
    [DataRow("BtnText", "T")]
    [DataRow("BtnEyedropper", "I")]
    [DataRow("BtnLine", "L")]
    [DataRow("BtnCurve", "C")]
    [DataRow("BtnShape", "H")]
    [DataRow("BtnSelect", "S")]
    [DataRow("BtnFreeSelect", "F")]
    [DataRow("BtnMagnifier", "M")]
    [DataRow("BtnGradient", "D")]
    [DataRow("BtnBlur", "U")]
    [DataRow("BtnSharpen", "R")]
    [DataRow("BtnSmudge", "X")]
    [DataRow("BtnCloneStamp", "N")]
    [DataRow("BtnPatternFill", "A")]
    [DataRow("BtnMeasure", "W")]
    public void ToolButton_HasAccessKey(string name, string expectedKey)
    {
        var btn = GetByName(name);
        var accessKey = btn.Attribute("AccessKey")?.Value;
        Assert.AreEqual(expectedKey, accessKey, $"{name} should have AccessKey=\"{expectedKey}\"");
    }

    #endregion

    #region AutomationProperties.Name on Tool Buttons

    [DataTestMethod]
    [DataRow("BtnPencil")]
    [DataRow("BtnBrush")]
    [DataRow("BtnEraser")]
    [DataRow("BtnFill")]
    [DataRow("BtnText")]
    [DataRow("BtnEyedropper")]
    [DataRow("BtnLine")]
    [DataRow("BtnCurve")]
    [DataRow("BtnShape")]
    [DataRow("BtnSelect")]
    [DataRow("BtnFreeSelect")]
    [DataRow("BtnMagnifier")]
    [DataRow("BtnGradient")]
    [DataRow("BtnBlur")]
    [DataRow("BtnSharpen")]
    [DataRow("BtnSmudge")]
    [DataRow("BtnCloneStamp")]
    [DataRow("BtnPatternFill")]
    [DataRow("BtnMeasure")]
    public void ToolButton_HasAutomationName(string name)
    {
        var btn = GetByName(name);
        var autoName = GetAutoProp(btn, "Name");
        Assert.IsNotNull(autoName, $"{name} should have AutomationProperties.Name");
        Assert.IsTrue(autoName!.Length > 0, $"{name} AutomationProperties.Name should not be empty");
    }

    #endregion

    #region Landmarks

    [TestMethod]
    public void MenuBar_HasNavigationLandmark()
    {
        var menuBar = _xamlDoc!.Descendants(_defaultNs + "MenuBar").First();
        var landmark = GetAutoProp(menuBar, "LandmarkType");
        Assert.AreEqual("Navigation", landmark);
    }

    [TestMethod]
    public void RibbonBar_HasCustomLandmark()
    {
        var ribbon = GetByName("RibbonBar");
        var landmark = GetAutoProp(ribbon, "LandmarkType");
        Assert.AreEqual("Custom", landmark);
        var localizedType = GetAutoProp(ribbon, "LocalizedLandmarkType");
        Assert.AreEqual("Toolbar", localizedType);
    }

    [TestMethod]
    public void CanvasScrollViewer_HasMainLandmark()
    {
        var canvas = GetByName("CanvasScrollViewer");
        var landmark = GetAutoProp(canvas, "LandmarkType");
        Assert.AreEqual("Main", landmark);
    }

    #endregion

    #region Status Bar LiveSetting

    [TestMethod]
    public void StatusTool_HasAssertiveLiveSetting()
    {
        var el = GetByName("StatusTool");
        var live = GetAutoProp(el, "LiveSetting");
        Assert.AreEqual("Assertive", live);
    }

    [TestMethod]
    public void StatusPosition_HasPoliteLiveSetting()
    {
        var el = GetByName("StatusPosition");
        var live = GetAutoProp(el, "LiveSetting");
        Assert.AreEqual("Polite", live);
    }

    #endregion

    #region RTL Support

    [TestMethod]
    public void RootGrid_HasNameAttribute()
    {
        var root = GetByName("RootGrid");
        Assert.IsNotNull(root);
    }

    [TestMethod]
    public void CanvasContainer_HasLeftToRightFlowDirection()
    {
        var container = GetByName("CanvasContainer");
        var fd = container.Attribute("FlowDirection")?.Value;
        Assert.AreEqual("LeftToRight", fd, "Canvas should always be LTR regardless of app language");
    }

    #endregion

    #region Insert Button

    [TestMethod]
    public void InsertButton_HasAutomationName()
    {
        var btn = GetByName("InsertButton");
        var autoName = GetAutoProp(btn, "Name");
        Assert.IsNotNull(autoName);
        Assert.IsTrue(autoName!.Contains("SmrtPad", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void InsertButton_HasAccentStyle()
    {
        var btn = GetByName("InsertButton");
        var style = btn.Attribute("Style")?.Value;
        Assert.IsNotNull(style);
        Assert.IsTrue(style!.Contains("AccentButtonStyle"));
    }

    #endregion

    #region AccessKey Uniqueness

    [TestMethod]
    public void AllToolAccessKeys_AreUnique()
    {
        var toolButtons = new[]
        {
            "BtnPencil", "BtnBrush", "BtnEraser", "BtnFill", "BtnText",
            "BtnEyedropper", "BtnLine", "BtnCurve", "BtnShape", "BtnSelect",
            "BtnFreeSelect", "BtnMagnifier", "BtnGradient", "BtnBlur",
            "BtnSharpen", "BtnSmudge", "BtnCloneStamp", "BtnPatternFill", "BtnMeasure"
        };

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in toolButtons)
        {
            var btn = GetByName(name);
            var key = btn.Attribute("AccessKey")?.Value;
            Assert.IsNotNull(key, $"{name} missing AccessKey");
            Assert.IsTrue(keys.Add(key!), $"Duplicate AccessKey '{key}' on {name}");
        }
    }

    #endregion
}
