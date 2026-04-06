using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;

namespace SmrtDoodle.Tests.UI;

/// <summary>
/// Tests verifying AI Tools menu items and their properties in MainWindow.xaml.
/// </summary>
[TestClass]
public class AiMenuTests
{
    private static XDocument? _xamlDoc;
    private static XNamespace _defaultNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static XNamespace _xNs = "http://schemas.microsoft.com/winfx/2006/xaml";

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

    [TestMethod]
    public void AiToolsSubMenu_ExistsInImageMenu()
    {
        var subItems = _xamlDoc!.Descendants(_defaultNs + "MenuFlyoutSubItem")
            .Where(e => e.Attribute("Text")?.Value?.Contains("AI Tools") == true);
        Assert.IsTrue(subItems.Any(), "AI Tools submenu should exist in Image menu");
    }

    [DataTestMethod]
    [DataRow("AiRemoveBackground", "Remove Background")]
    [DataRow("AiUpscale", "Upscale Image...")]
    [DataRow("AiContentAwareFill", "Content-Aware Fill")]
    [DataRow("AiAutoColorize", "Auto-Colorize")]
    [DataRow("AiStyleTransfer", "Style Transfer...")]
    [DataRow("AiDenoise", "Noise Reduction...")]
    public void AiMenuItem_Exists_WithCorrectText(string name, string expectedText)
    {
        var menuItem = GetByName(name);
        Assert.IsNotNull(menuItem, $"{name} should exist in XAML");
        Assert.AreEqual(expectedText, menuItem.Attribute("Text")?.Value);
    }

    [DataTestMethod]
    [DataRow("AiRemoveBackground", "AiRemoveBackground_Click")]
    [DataRow("AiUpscale", "AiUpscale_Click")]
    [DataRow("AiContentAwareFill", "AiContentAwareFill_Click")]
    [DataRow("AiAutoColorize", "AiAutoColorize_Click")]
    [DataRow("AiStyleTransfer", "AiStyleTransfer_Click")]
    [DataRow("AiDenoise", "AiDenoise_Click")]
    public void AiMenuItem_HasClickHandler(string name, string expectedHandler)
    {
        var menuItem = GetByName(name);
        var clickHandler = menuItem.Attribute("Click")?.Value;
        Assert.AreEqual(expectedHandler, clickHandler);
    }

    [DataTestMethod]
    [DataRow("AiRemoveBackground")]
    [DataRow("AiUpscale")]
    [DataRow("AiContentAwareFill")]
    [DataRow("AiAutoColorize")]
    [DataRow("AiStyleTransfer")]
    [DataRow("AiDenoise")]
    public void AiMenuItem_HasAutomationName(string name)
    {
        var menuItem = GetByName(name);
        var autoName = menuItem.Attribute("AutomationProperties.Name")?.Value;
        Assert.IsNotNull(autoName, $"{name} should have AutomationProperties.Name");
        Assert.IsTrue(autoName!.StartsWith("AI"), $"{name} AutomationProperties.Name should start with 'AI'");
    }

    [TestMethod]
    public void AiToolsSubMenu_HasSixItems()
    {
        var subMenu = _xamlDoc!.Descendants(_defaultNs + "MenuFlyoutSubItem")
            .First(e => e.Attribute("Text")?.Value?.Contains("AI Tools") == true);
        var items = subMenu.Elements(_defaultNs + "MenuFlyoutItem").ToList();
        Assert.AreEqual(6, items.Count, "AI Tools submenu should have 6 items");
    }
}
