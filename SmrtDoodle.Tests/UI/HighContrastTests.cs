using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;

namespace SmrtDoodle.Tests.UI;

/// <summary>
/// Tests verifying the HighContrastResources.xaml theme dictionary.
/// </summary>
[TestClass]
public class HighContrastTests
{
    private static XDocument? _doc;
    private static XNamespace _ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static XNamespace _xNs = "http://schemas.microsoft.com/winfx/2006/xaml";

    [ClassInitialize]
    public static void LoadResources(TestContext _)
    {
        var dir = AppContext.BaseDirectory;
        string? path = null;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "SmrtDoodle", "Themes", "HighContrastResources.xaml");
            if (File.Exists(candidate))
            {
                path = candidate;
                break;
            }
            dir = Path.GetDirectoryName(dir);
        }
        Assert.IsNotNull(path, "Could not find HighContrastResources.xaml");
        _doc = XDocument.Load(path);
    }

    [TestMethod]
    public void HighContrast_ThemeDictionary_Exists()
    {
        var dicts = _doc!.Descendants(_ns + "ResourceDictionary")
            .Where(d => d.Attribute(_xNs + "Key")?.Value == "HighContrast");
        Assert.AreEqual(1, dicts.Count(), "HighContrast theme dictionary should exist");
    }

    [DataTestMethod]
    [DataRow("HighContrast")]
    [DataRow("Default")]
    [DataRow("Light")]
    [DataRow("Dark")]
    public void ThemeDictionary_Exists(string theme)
    {
        var dicts = _doc!.Descendants(_ns + "ResourceDictionary")
            .Where(d => d.Attribute(_xNs + "Key")?.Value == theme);
        Assert.AreEqual(1, dicts.Count(), $"{theme} theme dictionary should exist");
    }

    [DataTestMethod]
    [DataRow("CanvasBorderBrush")]
    [DataRow("CanvasBackgroundBrush")]
    [DataRow("SelectionBorderBrush")]
    [DataRow("ActiveToolHighlightBrush")]
    [DataRow("ActiveToolForegroundBrush")]
    [DataRow("LayerItemSelectedBrush")]
    [DataRow("LayerItemTextBrush")]
    [DataRow("StatusBarBackgroundBrush")]
    [DataRow("StatusBarForegroundBrush")]
    [DataRow("RulerBackgroundBrush")]
    [DataRow("RulerForegroundBrush")]
    [DataRow("RulerTickBrush")]
    [DataRow("GridLineBrush")]
    [DataRow("PaletteSwatchBorderBrush")]
    public void HighContrast_ContainsRequiredBrush(string resourceKey)
    {
        var hcDict = _doc!.Descendants(_ns + "ResourceDictionary")
            .First(d => d.Attribute(_xNs + "Key")?.Value == "HighContrast");

        var brush = hcDict.Descendants(_ns + "SolidColorBrush")
            .FirstOrDefault(b => b.Attribute(_xNs + "Key")?.Value == resourceKey);

        Assert.IsNotNull(brush, $"HighContrast dictionary should contain {resourceKey}");
    }

    [TestMethod]
    public void HighContrast_SelectionBorderThickness_GreaterThanDefault()
    {
        var hcDict = _doc!.Descendants(_ns + "ResourceDictionary")
            .First(d => d.Attribute(_xNs + "Key")?.Value == "HighContrast");
        var defaultDict = _doc!.Descendants(_ns + "ResourceDictionary")
            .First(d => d.Attribute(_xNs + "Key")?.Value == "Default");

        var hcThickness = hcDict.Descendants()
            .FirstOrDefault(e => e.Attribute(_xNs + "Key")?.Value == "SelectionBorderThickness");
        var defaultThickness = defaultDict.Descendants()
            .FirstOrDefault(e => e.Attribute(_xNs + "Key")?.Value == "SelectionBorderThickness");

        Assert.IsNotNull(hcThickness);
        Assert.IsNotNull(defaultThickness);

        double hcVal = double.Parse(hcThickness!.Value);
        double defVal = double.Parse(defaultThickness!.Value);
        Assert.IsTrue(hcVal >= defVal, "High contrast selection border should be at least as thick as default");
    }

    [TestMethod]
    public void AllThemes_HaveSameBrushKeys()
    {
        var themes = new[] { "HighContrast", "Default", "Light", "Dark" };
        var brushesPerTheme = new Dictionary<string, HashSet<string>>();

        foreach (var theme in themes)
        {
            var dict = _doc!.Descendants(_ns + "ResourceDictionary")
                .First(d => d.Attribute(_xNs + "Key")?.Value == theme);
            var keys = dict.Descendants(_ns + "SolidColorBrush")
                .Select(b => b.Attribute(_xNs + "Key")?.Value ?? "")
                .Where(k => k.Length > 0)
                .ToHashSet();
            brushesPerTheme[theme] = keys;
        }

        var reference = brushesPerTheme["Default"];
        foreach (var theme in themes)
        {
            foreach (var key in reference)
            {
                Assert.IsTrue(brushesPerTheme[theme].Contains(key),
                    $"Theme '{theme}' is missing brush '{key}'");
            }
        }
    }
}
