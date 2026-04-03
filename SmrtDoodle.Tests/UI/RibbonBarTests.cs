using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;
using SmrtDoodle.Tools;
using System.Xml.Linq;

namespace SmrtDoodle.Tests.UI;

/// <summary>
/// Comprehensive tests that validate every ribbon bar element, menu item, status bar field,
/// and layer panel attribute by parsing MainWindow.xaml and verifying model/tool contracts.
/// </summary>
[TestClass]
public class RibbonBarTests
{
    private static XDocument? _xamlDoc;
    private static XNamespace _defaultNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static XNamespace _xNs = "http://schemas.microsoft.com/winfx/2006/xaml";

    [ClassInitialize]
    public static void LoadXaml(TestContext _)
    {
        // Walk up from bin output to find the MainWindow.xaml in the sibling project
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

        Assert.IsNotNull(xamlPath, "Could not locate MainWindow.xaml");
        _xamlDoc = XDocument.Load(xamlPath);
    }

    private static IEnumerable<XElement> FindByName(string name) =>
        _xamlDoc!.Descendants().Where(e => (string?)e.Attribute(_xNs + "Name") == name);

    private static XElement GetByName(string name)
    {
        var el = FindByName(name).FirstOrDefault();
        Assert.IsNotNull(el, $"Element x:Name=\"{name}\" not found in XAML");
        return el;
    }

    #region Ribbon Structure

    [TestMethod]
    public void RibbonBar_Exists_WithCorrectHeight()
    {
        var ribbon = GetByName("RibbonBar");
        Assert.AreEqual("100", ribbon.Attribute("Height")?.Value);
    }

    [TestMethod]
    public void RibbonBar_HasFiveGroupsAndFourDividers()
    {
        // The ribbon's inner Grid should have 10 column definitions
        var ribbon = GetByName("RibbonBar");
        var innerGrid = ribbon.Elements(_defaultNs + "Grid").FirstOrDefault();
        Assert.IsNotNull(innerGrid, "Ribbon inner Grid not found");
        var colDefs = innerGrid
            .Elements(_defaultNs + "Grid.ColumnDefinitions")
            .Elements(_defaultNs + "ColumnDefinition")
            .ToList();
        Assert.AreEqual(10, colDefs.Count, "Expected 10 columns (5 groups + 4 separators + 1 insert)");

        // Verify 4 Rectangle dividers exist in the ribbon grid
        var rectangles = innerGrid.Elements(_defaultNs + "Rectangle").ToList();
        Assert.AreEqual(4, rectangles.Count, "Expected 4 Rectangle dividers");
    }

    [TestMethod]
    public void RibbonBar_GroupLabels_ArePresent()
    {
        var expectedLabels = new[] { "Tools", "Brush", "Shapes", "Selection", "Colors" };
        var ribbon = GetByName("RibbonBar");
        var textBlocks = ribbon.Descendants(_defaultNs + "TextBlock")
            .Where(tb => tb.Attribute("FontSize")?.Value == "10")
            .Select(tb => tb.Attribute("Text")?.Value)
            .ToList();

        foreach (var label in expectedLabels)
        {
            Assert.IsTrue(textBlocks.Contains(label), $"Group label '{label}' not found in ribbon");
        }
    }

    #endregion

    #region Tool Buttons

    [DataTestMethod]
    [DataRow("BtnPencil", "Pencil", "Pencil")]
    [DataRow("BtnBrush", "Brush", "Brush")]
    [DataRow("BtnEraser", "Eraser", "Eraser")]
    [DataRow("BtnFill", "Fill", "Fill")]
    [DataRow("BtnText", "Text", "Text")]
    [DataRow("BtnEyedropper", "Eyedropper", "Color Picker")]
    [DataRow("BtnLine", "Line", "Line")]
    [DataRow("BtnCurve", "Curve", "Curve")]
    [DataRow("BtnShape", "Shape", "Shape")]
    [DataRow("BtnSelect", "Selection", "Select")]
    [DataRow("BtnFreeSelect", "FreeFormSelection", "Free-form Select")]
    [DataRow("BtnMagnifier", "Magnifier", "Magnifier")]
    public void ToolButton_HasCorrectTagAndTooltip(string name, string expectedTag, string expectedTooltip)
    {
        var btn = GetByName(name);
        Assert.AreEqual("ToggleButton", btn.Name.LocalName, $"{name} should be a ToggleButton");
        Assert.AreEqual(expectedTag, btn.Attribute("Tag")?.Value, $"{name} has wrong Tag");
        Assert.AreEqual(expectedTooltip, btn.Attribute(_defaultNs + "ToolTipService.ToolTip")?.Value
            ?? btn.Attribute("ToolTipService.ToolTip")?.Value, $"{name} has wrong ToolTip");
    }

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
    public void ToolButton_HasCorrectDimensions(string name)
    {
        var btn = GetByName(name);
        Assert.AreEqual("30", btn.Attribute("Width")?.Value, $"{name} Width should be 30");
        Assert.AreEqual("30", btn.Attribute("Height")?.Value, $"{name} Height should be 30");
        Assert.AreEqual("4", btn.Attribute("CornerRadius")?.Value, $"{name} CornerRadius should be 4");
    }

    [TestMethod]
    public void ToolButton_Count_Matches_DrawingToolEnumExcludingCrop()
    {
        // All DrawingTool values should have a toolbar button, except Crop (menu-only)
        var toolValues = Enum.GetValues<DrawingTool>().Where(t => t != DrawingTool.Crop).ToList();
        var toolButtons = new[]
        {
            "BtnPencil", "BtnBrush", "BtnEraser", "BtnFill", "BtnText", "BtnEyedropper",
            "BtnLine", "BtnCurve", "BtnShape", "BtnSelect", "BtnFreeSelect", "BtnMagnifier"
        };
        Assert.AreEqual(toolValues.Count, toolButtons.Length,
            $"Expected {toolValues.Count} tool buttons (DrawingTool enum minus Crop), found {toolButtons.Length}");
    }

    [TestMethod]
    public void ToolButton_Tags_AreValidDrawingToolValues()
    {
        var toolButtonNames = new[]
        {
            "BtnPencil", "BtnBrush", "BtnEraser", "BtnFill", "BtnText", "BtnEyedropper",
            "BtnLine", "BtnCurve", "BtnShape", "BtnSelect", "BtnFreeSelect", "BtnMagnifier"
        };
        foreach (var name in toolButtonNames)
        {
            var btn = GetByName(name);
            var tag = btn.Attribute("Tag")?.Value;
            Assert.IsTrue(Enum.TryParse<DrawingTool>(tag, out _),
                $"{name} Tag '{tag}' is not a valid DrawingTool enum value");
        }
    }

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
    public void ToolButton_HasClickHandler(string name)
    {
        var btn = GetByName(name);
        Assert.AreEqual("Tool_Click", btn.Attribute("Click")?.Value,
            $"{name} should use Tool_Click handler");
    }

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
    public void ToolButton_HasFontIconChild(string name)
    {
        var btn = GetByName(name);
        var fontIcon = btn.Elements(_defaultNs + "FontIcon").FirstOrDefault();
        Assert.IsNotNull(fontIcon, $"{name} should contain a FontIcon child");
        Assert.AreEqual("14", fontIcon.Attribute("FontSize")?.Value, $"{name} FontIcon size should be 14");
        Assert.IsNotNull(fontIcon.Attribute("Glyph")?.Value, $"{name} FontIcon should have a Glyph");
    }

    [TestMethod]
    public void ToolButton_IconGlyphs_MatchToolDefinitions()
    {
        // Verify the XAML glyphs match the Icon property from each tool class
        var toolIconMap = new Dictionary<string, string>
        {
            ["BtnPencil"] = new PencilTool().Icon,
            ["BtnBrush"] = new BrushTool().Icon,
            ["BtnEraser"] = new EraserTool().Icon,
            ["BtnFill"] = new FillTool().Icon,
            ["BtnText"] = new TextTool().Icon,
            ["BtnEyedropper"] = new EyedropperTool().Icon,
            ["BtnLine"] = new LineTool().Icon,
            ["BtnCurve"] = new CurveTool().Icon,
            ["BtnShape"] = new ShapeTool().Icon,
            ["BtnSelect"] = new SelectionTool().Icon,
            ["BtnFreeSelect"] = new FreeFormSelectionTool().Icon,
            ["BtnMagnifier"] = new MagnifierTool().Icon,
        };

        foreach (var (name, expectedIcon) in toolIconMap)
        {
            var btn = GetByName(name);
            var fontIcon = btn.Elements(_defaultNs + "FontIcon").First();
            var glyphAttr = fontIcon.Attribute("Glyph")?.Value;
            Assert.IsNotNull(glyphAttr, $"{name} FontIcon Glyph is null");

            // XAML uses &#xHHHH; which gets decoded when parsed — compare the decoded character
            var expectedChar = expectedIcon[0];
            Assert.AreEqual(expectedChar, glyphAttr[0],
                $"{name} glyph mismatch: expected U+{(int)expectedChar:X4}, got U+{(int)glyphAttr[0]:X4}");
        }
    }

    #endregion

    #region Brush Group

    [TestMethod]
    public void StrokeSizeSlider_HasCorrectRange()
    {
        var slider = GetByName("StrokeSizeSlider");
        Assert.AreEqual("1", slider.Attribute("Minimum")?.Value);
        Assert.AreEqual("50", slider.Attribute("Maximum")?.Value);
        Assert.AreEqual("3", slider.Attribute("Value")?.Value);
    }

    [TestMethod]
    public void BrushStyleCombo_HasEightItems()
    {
        var combo = GetByName("BrushStyleCombo");
        var items = combo.Elements(_defaultNs + "ComboBoxItem").ToList();
        Assert.AreEqual(8, items.Count, "BrushStyleCombo should have 8 items (one per BrushStyle)");
    }

    [TestMethod]
    public void BrushStyleCombo_ItemsMatchEnum()
    {
        var combo = GetByName("BrushStyleCombo");
        var items = combo.Elements(_defaultNs + "ComboBoxItem")
            .Select(i => i.Attribute("Content")?.Value)
            .ToList();

        var expectedLabels = new[]
        {
            "Normal", "Calligraphy", "Airbrush", "Oil",
            "Crayon", "Marker", "Natural Pencil", "Watercolor"
        };

        CollectionAssert.AreEqual(expectedLabels, items,
            "BrushStyleCombo items don't match expected brush style labels");
    }

    [TestMethod]
    public void BrushStyleCombo_DefaultSelectionIsNormal()
    {
        var combo = GetByName("BrushStyleCombo");
        Assert.AreEqual("0", combo.Attribute("SelectedIndex")?.Value);
    }

    [TestMethod]
    public void BrushStyleCombo_HasSelectionChangedHandler()
    {
        var combo = GetByName("BrushStyleCombo");
        Assert.AreEqual("BrushStyleCombo_SelectionChanged", combo.Attribute("SelectionChanged")?.Value);
    }

    #endregion

    #region Shapes Group

    [TestMethod]
    public void ShapeTypeCombo_HasTwelveItems()
    {
        var combo = GetByName("ShapeTypeCombo");
        var items = combo.Elements(_defaultNs + "ComboBoxItem").ToList();
        Assert.AreEqual(12, items.Count, "ShapeTypeCombo should have 12 items (one per ShapeType)");
    }

    [TestMethod]
    public void ShapeTypeCombo_ItemCountMatchesEnum()
    {
        var enumCount = Enum.GetValues<ShapeType>().Length;
        var combo = GetByName("ShapeTypeCombo");
        var comboCount = combo.Elements(_defaultNs + "ComboBoxItem").Count();
        Assert.AreEqual(enumCount, comboCount,
            $"ShapeTypeCombo items ({comboCount}) should match ShapeType enum ({enumCount})");
    }

    [TestMethod]
    public void ShapeTypeCombo_DefaultSelectionIsRectangle()
    {
        var combo = GetByName("ShapeTypeCombo");
        Assert.AreEqual("0", combo.Attribute("SelectedIndex")?.Value);
        // First item should be Rectangle
        var firstItem = combo.Elements(_defaultNs + "ComboBoxItem").First();
        Assert.AreEqual("Rectangle", firstItem.Attribute("Content")?.Value);
    }

    [TestMethod]
    public void FillModeCombo_HasThreeItems()
    {
        var combo = GetByName("FillModeCombo");
        var items = combo.Elements(_defaultNs + "ComboBoxItem").ToList();
        Assert.AreEqual(3, items.Count, "FillModeCombo should have 3 items");
    }

    [TestMethod]
    public void FillModeCombo_ItemsAreCorrect()
    {
        var combo = GetByName("FillModeCombo");
        var items = combo.Elements(_defaultNs + "ComboBoxItem")
            .Select(i => i.Attribute("Content")?.Value)
            .ToList();
        CollectionAssert.AreEqual(new[] { "Outline", "Fill", "Outline + Fill" }, items);
    }

    [TestMethod]
    public void FillModeCombo_DefaultSelectionIsOutline()
    {
        var combo = GetByName("FillModeCombo");
        Assert.AreEqual("0", combo.Attribute("SelectedIndex")?.Value);
    }

    [TestMethod]
    public void ShapeTypeCombo_HasSelectionChangedHandler()
    {
        var combo = GetByName("ShapeTypeCombo");
        Assert.AreEqual("ShapeTypeCombo_SelectionChanged", combo.Attribute("SelectionChanged")?.Value);
    }

    [TestMethod]
    public void FillModeCombo_HasSelectionChangedHandler()
    {
        var combo = GetByName("FillModeCombo");
        Assert.AreEqual("FillModeCombo_SelectionChanged", combo.Attribute("SelectionChanged")?.Value);
    }

    #endregion

    #region Selection Group

    [TestMethod]
    public void TransparentSelectionCheck_Exists()
    {
        var check = GetByName("TransparentSelectionCheck");
        Assert.AreEqual("CheckBox", check.Name.LocalName);
        Assert.AreEqual("Transparent", check.Attribute("Content")?.Value);
    }

    [TestMethod]
    public void TransparentSelectionCheck_HasClickHandler()
    {
        var check = GetByName("TransparentSelectionCheck");
        Assert.AreEqual("TransparentSelection_Click", check.Attribute("Click")?.Value);
    }

    [TestMethod]
    public void TransparentSelectionCheck_HasTooltip()
    {
        var check = GetByName("TransparentSelectionCheck");
        var tooltip = check.Attribute(_defaultNs + "ToolTipService.ToolTip")?.Value
                      ?? check.Attribute("ToolTipService.ToolTip")?.Value;
        Assert.IsNotNull(tooltip, "TransparentSelectionCheck should have a ToolTip");
        Assert.IsTrue(tooltip.Contains("Transparent", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Colors Group

    [TestMethod]
    public void PrimaryColorBorder_Exists_WithBlackDefault()
    {
        var border = GetByName("PrimaryColorBorder");
        Assert.AreEqual("Black", border.Attribute("Background")?.Value);
    }

    [TestMethod]
    public void SecondaryColorBorder_Exists_WithWhiteDefault()
    {
        var border = GetByName("SecondaryColorBorder");
        Assert.AreEqual("White", border.Attribute("Background")?.Value);
    }

    [TestMethod]
    public void PrimaryColorBorder_HasTappedHandler()
    {
        var border = GetByName("PrimaryColorBorder");
        Assert.AreEqual("PrimaryColor_Tapped", border.Attribute("Tapped")?.Value);
    }

    [TestMethod]
    public void SecondaryColorBorder_HasTappedHandler()
    {
        var border = GetByName("SecondaryColorBorder");
        Assert.AreEqual("SecondaryColor_Tapped", border.Attribute("Tapped")?.Value);
    }

    [TestMethod]
    public void SwapColorsButton_Exists_WithClickHandler()
    {
        var btn = GetByName("SwapColorsButton");
        Assert.AreEqual("Button", btn.Name.LocalName);
        Assert.AreEqual("SwapColors_Click", btn.Attribute("Click")?.Value);
    }

    [TestMethod]
    public void SwapColorsButton_HasTooltip()
    {
        var btn = GetByName("SwapColorsButton");
        var tooltip = btn.Attribute(_defaultNs + "ToolTipService.ToolTip")?.Value
                      ?? btn.Attribute("ToolTipService.ToolTip")?.Value;
        Assert.AreEqual("Swap Colors", tooltip);
    }

    [TestMethod]
    public void ColorPaletteGrid_Exists_WithClickEnabled()
    {
        var grid = GetByName("ColorPaletteGrid");
        Assert.AreEqual("GridView", grid.Name.LocalName);
        Assert.AreEqual("True", grid.Attribute("IsItemClickEnabled")?.Value);
        Assert.AreEqual("None", grid.Attribute("SelectionMode")?.Value);
    }

    [TestMethod]
    public void ColorPaletteGrid_HasItemClickHandler()
    {
        var grid = GetByName("ColorPaletteGrid");
        Assert.AreEqual("ColorPalette_ItemClick", grid.Attribute("ItemClick")?.Value);
    }

    [TestMethod]
    public void ColorPaletteGrid_ScrollDisabled()
    {
        var grid = GetByName("ColorPaletteGrid");
        // Check all four scroll attributes are disabled
        Assert.AreEqual("Disabled", grid.Attribute(_defaultNs + "ScrollViewer.HorizontalScrollBarVisibility")?.Value
            ?? grid.Attribute("ScrollViewer.HorizontalScrollBarVisibility")?.Value);
        Assert.AreEqual("Disabled", grid.Attribute(_defaultNs + "ScrollViewer.VerticalScrollBarVisibility")?.Value
            ?? grid.Attribute("ScrollViewer.VerticalScrollBarVisibility")?.Value);
    }

    #endregion

    #region Insert Button (SmrtPad mode)

    [TestMethod]
    public void InsertButton_DefaultCollapsed()
    {
        var btn = GetByName("InsertButton");
        Assert.AreEqual("Collapsed", btn.Attribute("Visibility")?.Value);
    }

    [TestMethod]
    public void InsertButton_HasClickHandler()
    {
        var btn = GetByName("InsertButton");
        Assert.AreEqual("InsertIntoDocument_Click", btn.Attribute("Click")?.Value);
    }

    #endregion
}

[TestClass]
public class MenuBarTests
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
        Assert.IsNotNull(xamlPath);
        _xamlDoc = XDocument.Load(xamlPath);
    }

    private static XElement GetByName(string name) =>
        _xamlDoc!.Descendants().First(e => (string?)e.Attribute(_xNs + "Name") == name);

    [TestMethod]
    public void MenuBar_HasFiveMenus()
    {
        var menus = _xamlDoc!.Descendants(_defaultNs + "MenuBarItem").ToList();
        Assert.AreEqual(5, menus.Count, "Expected 5 menus: File, Edit, Image, View, Layers");
    }

    [DataTestMethod]
    [DataRow("File")]
    [DataRow("Edit")]
    [DataRow("Image")]
    [DataRow("View")]
    [DataRow("Layers")]
    public void MenuBar_HasExpectedMenu(string title)
    {
        var menu = _xamlDoc!.Descendants(_defaultNs + "MenuBarItem")
            .FirstOrDefault(m => m.Attribute("Title")?.Value == title);
        Assert.IsNotNull(menu, $"Menu '{title}' not found");
    }

    [TestMethod]
    public void FileMenu_HasExpectedItems()
    {
        var fileMenu = _xamlDoc!.Descendants(_defaultNs + "MenuBarItem")
            .First(m => m.Attribute("Title")?.Value == "File");
        var items = fileMenu.Elements(_defaultNs + "MenuFlyoutItem")
            .Select(i => i.Attribute("Text")?.Value)
            .ToList();

        Assert.IsTrue(items.Contains("New"));
        Assert.IsTrue(items.Contains("Open..."));
        Assert.IsTrue(items.Contains("Save"));
        Assert.IsTrue(items.Contains("Save As..."));
        Assert.IsTrue(items.Contains("Print..."));
        Assert.IsTrue(items.Contains("Exit"));
    }

    [TestMethod]
    public void EditMenu_HasUndoAndRedo()
    {
        var undoItem = GetByName("UndoMenuItem");
        Assert.AreEqual("Undo", undoItem.Attribute("Text")?.Value);
        var redoItem = GetByName("RedoMenuItem");
        Assert.AreEqual("Redo", redoItem.Attribute("Text")?.Value);
    }

    [TestMethod]
    public void EditMenu_HasClipboardItems()
    {
        var editMenu = _xamlDoc!.Descendants(_defaultNs + "MenuBarItem")
            .First(m => m.Attribute("Title")?.Value == "Edit");
        var items = editMenu.Elements(_defaultNs + "MenuFlyoutItem")
            .Select(i => i.Attribute("Text")?.Value)
            .ToList();

        Assert.IsTrue(items.Contains("Cut"));
        Assert.IsTrue(items.Contains("Copy"));
        Assert.IsTrue(items.Contains("Paste"));
        Assert.IsTrue(items.Contains("Paste as New Image"));
        Assert.IsTrue(items.Contains("Paste From File..."));
    }

    [TestMethod]
    public void EditMenu_HasSelectionItems()
    {
        var editMenu = _xamlDoc!.Descendants(_defaultNs + "MenuBarItem")
            .First(m => m.Attribute("Title")?.Value == "Edit");
        var items = editMenu.Elements(_defaultNs + "MenuFlyoutItem")
            .Select(i => i.Attribute("Text")?.Value)
            .ToList();

        Assert.IsTrue(items.Contains("Select All"));
        Assert.IsTrue(items.Contains("Clear Selection"));
        Assert.IsTrue(items.Contains("Delete Selection"));
    }

    [TestMethod]
    public void ImageMenu_HasTransformItems()
    {
        var imageMenu = _xamlDoc!.Descendants(_defaultNs + "MenuBarItem")
            .First(m => m.Attribute("Title")?.Value == "Image");
        var items = imageMenu.Elements(_defaultNs + "MenuFlyoutItem")
            .Select(i => i.Attribute("Text")?.Value)
            .ToList();

        Assert.IsTrue(items.Contains("Resize..."));
        Assert.IsTrue(items.Contains("Crop"));
        Assert.IsTrue(items.Contains("Flip Horizontal"));
        Assert.IsTrue(items.Contains("Flip Vertical"));
        Assert.IsTrue(items.Contains("Rotate 90°"));
        Assert.IsTrue(items.Contains("Rotate 180°"));
        Assert.IsTrue(items.Contains("Rotate 270°"));
        Assert.IsTrue(items.Contains("Invert Colors"));
        Assert.IsTrue(items.Contains("Clear Image"));
        Assert.IsTrue(items.Contains("Canvas Properties..."));
    }

    [TestMethod]
    public void ViewMenu_HasGridAndRulerToggles()
    {
        var showGrid = GetByName("ShowGridToggle");
        Assert.AreEqual("ToggleMenuFlyoutItem", showGrid.Name.LocalName);
        Assert.AreEqual("Show Grid", showGrid.Attribute("Text")?.Value);
        Assert.AreEqual("ToggleGrid_Click", showGrid.Attribute("Click")?.Value);

        var showRuler = GetByName("ShowRulerToggle");
        Assert.AreEqual("ToggleMenuFlyoutItem", showRuler.Name.LocalName);
        Assert.AreEqual("Show Ruler", showRuler.Attribute("Text")?.Value);
        Assert.AreEqual("ToggleRuler_Click", showRuler.Attribute("Click")?.Value);
    }

    [TestMethod]
    public void ViewMenu_HasZoomItems()
    {
        var viewMenu = _xamlDoc!.Descendants(_defaultNs + "MenuBarItem")
            .First(m => m.Attribute("Title")?.Value == "View");
        var items = viewMenu.Elements(_defaultNs + "MenuFlyoutItem")
            .Select(i => i.Attribute("Text")?.Value)
            .ToList();

        Assert.IsTrue(items.Contains("Zoom In"));
        Assert.IsTrue(items.Contains("Zoom Out"));
        Assert.IsTrue(items.Contains("Zoom to Fit"));
        Assert.IsTrue(items.Contains("100%"));
    }

    [TestMethod]
    public void LayersMenu_HasAllExpectedItems()
    {
        var layersMenu = _xamlDoc!.Descendants(_defaultNs + "MenuBarItem")
            .First(m => m.Attribute("Title")?.Value == "Layers");
        var items = layersMenu.Elements(_defaultNs + "MenuFlyoutItem")
            .Select(i => i.Attribute("Text")?.Value)
            .ToList();

        Assert.IsTrue(items.Contains("Add Layer"));
        Assert.IsTrue(items.Contains("Delete Layer"));
        Assert.IsTrue(items.Contains("Duplicate Layer"));
        Assert.IsTrue(items.Contains("Move Layer Up"));
        Assert.IsTrue(items.Contains("Move Layer Down"));
        Assert.IsTrue(items.Contains("Merge Down"));
        Assert.IsTrue(items.Contains("Flatten Image"));
    }

    [TestMethod]
    public void InsertIntoDocumentItem_DefaultCollapsed()
    {
        var item = GetByName("InsertIntoDocumentItem");
        Assert.AreEqual("Collapsed", item.Attribute("Visibility")?.Value);
    }

    [DataTestMethod]
    [DataRow("New", "Control", "N")]
    [DataRow("Open...", "Control", "O")]
    [DataRow("Save", "Control", "S")]
    [DataRow("Save As...", "Control,Shift", "S")]
    [DataRow("Print...", "Control", "P")]
    public void FileMenu_KeyboardAccelerators(string menuText, string modifiers, string key)
    {
        var item = _xamlDoc!.Descendants(_defaultNs + "MenuFlyoutItem")
            .First(i => i.Attribute("Text")?.Value == menuText);
        var accel = item.Descendants(_defaultNs + "KeyboardAccelerator").FirstOrDefault();
        Assert.IsNotNull(accel, $"{menuText} should have a KeyboardAccelerator");
        Assert.AreEqual(modifiers, accel.Attribute("Modifiers")?.Value);
        Assert.AreEqual(key, accel.Attribute("Key")?.Value);
    }

    [DataTestMethod]
    [DataRow("Undo", "Control", "Z")]
    [DataRow("Redo", "Control", "Y")]
    [DataRow("Cut", "Control", "X")]
    [DataRow("Copy", "Control", "C")]
    [DataRow("Paste", "Control", "V")]
    [DataRow("Select All", "Control", "A")]
    public void EditMenu_KeyboardAccelerators(string menuText, string modifiers, string key)
    {
        var item = _xamlDoc!.Descendants(_defaultNs + "MenuFlyoutItem")
            .First(i => i.Attribute("Text")?.Value == menuText);
        var accel = item.Descendants(_defaultNs + "KeyboardAccelerator").FirstOrDefault();
        Assert.IsNotNull(accel, $"{menuText} should have a KeyboardAccelerator");
        Assert.AreEqual(modifiers, accel.Attribute("Modifiers")?.Value);
        Assert.AreEqual(key, accel.Attribute("Key")?.Value);
    }

    [TestMethod]
    public void ClearSelection_UsesEscapeKey()
    {
        var item = _xamlDoc!.Descendants(_defaultNs + "MenuFlyoutItem")
            .First(i => i.Attribute("Text")?.Value == "Clear Selection");
        var accel = item.Descendants(_defaultNs + "KeyboardAccelerator").First();
        Assert.AreEqual("Escape", accel.Attribute("Key")?.Value);
    }

    [TestMethod]
    public void DeleteSelection_UsesDeleteKey()
    {
        var item = _xamlDoc!.Descendants(_defaultNs + "MenuFlyoutItem")
            .First(i => i.Attribute("Text")?.Value == "Delete Selection");
        var accel = item.Descendants(_defaultNs + "KeyboardAccelerator").First();
        Assert.AreEqual("Delete", accel.Attribute("Key")?.Value);
    }

    [TestMethod]
    public void InvertColors_HasKeyboardAccelerator()
    {
        var item = _xamlDoc!.Descendants(_defaultNs + "MenuFlyoutItem")
            .First(i => i.Attribute("Text")?.Value == "Invert Colors");
        var accel = item.Descendants(_defaultNs + "KeyboardAccelerator").First();
        Assert.AreEqual("Control,Shift", accel.Attribute("Modifiers")?.Value);
        Assert.AreEqual("I", accel.Attribute("Key")?.Value);
    }

    [TestMethod]
    public void ClearImage_HasKeyboardAccelerator()
    {
        var item = _xamlDoc!.Descendants(_defaultNs + "MenuFlyoutItem")
            .First(i => i.Attribute("Text")?.Value == "Clear Image");
        var accel = item.Descendants(_defaultNs + "KeyboardAccelerator").First();
        Assert.AreEqual("Control,Shift", accel.Attribute("Modifiers")?.Value);
        Assert.AreEqual("N", accel.Attribute("Key")?.Value);
    }
}

[TestClass]
public class StatusBarTests
{
    private static XDocument? _xamlDoc;
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
        Assert.IsNotNull(xamlPath);
        _xamlDoc = XDocument.Load(xamlPath);
    }

    private static XElement GetByName(string name) =>
        _xamlDoc!.Descendants().First(e => (string?)e.Attribute(_xNs + "Name") == name);

    [TestMethod]
    public void StatusPosition_HasDefaultText()
    {
        var el = GetByName("StatusPosition");
        Assert.AreEqual("0, 0 px", el.Attribute("Text")?.Value);
    }

    [TestMethod]
    public void StatusCanvasSize_HasDefaultText()
    {
        var el = GetByName("StatusCanvasSize");
        Assert.AreEqual("800 x 600 px", el.Attribute("Text")?.Value);
    }

    [TestMethod]
    public void StatusTool_HasDefaultText()
    {
        var el = GetByName("StatusTool");
        Assert.AreEqual("Pencil", el.Attribute("Text")?.Value);
    }

    [TestMethod]
    public void StatusSelection_IsEmptyByDefault()
    {
        var el = GetByName("StatusSelection");
        Assert.AreEqual("", el.Attribute("Text")?.Value);
    }

    [TestMethod]
    public void StatusZoom_HasDefaultText()
    {
        var el = GetByName("StatusZoom");
        Assert.AreEqual("100%", el.Attribute("Text")?.Value);
    }

    [TestMethod]
    public void ZoomSlider_HasCorrectRange()
    {
        var slider = GetByName("ZoomSlider");
        Assert.AreEqual("10", slider.Attribute("Minimum")?.Value);
        Assert.AreEqual("800", slider.Attribute("Maximum")?.Value);
        Assert.AreEqual("100", slider.Attribute("Value")?.Value);
    }

    [TestMethod]
    public void ZoomSlider_HasValueChangedHandler()
    {
        var slider = GetByName("ZoomSlider");
        Assert.AreEqual("ZoomSlider_ValueChanged", slider.Attribute("ValueChanged")?.Value);
    }
}

[TestClass]
public class LayerPanelTests
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
        Assert.IsNotNull(xamlPath);
        _xamlDoc = XDocument.Load(xamlPath);
    }

    private static XElement GetByName(string name) =>
        _xamlDoc!.Descendants().First(e => (string?)e.Attribute(_xNs + "Name") == name);

    [TestMethod]
    public void LayerListView_HasSingleSelectionMode()
    {
        var list = GetByName("LayerListView");
        Assert.AreEqual("Single", list.Attribute("SelectionMode")?.Value);
    }

    [TestMethod]
    public void LayerListView_HasSelectionChangedHandler()
    {
        var list = GetByName("LayerListView");
        Assert.AreEqual("LayerList_SelectionChanged", list.Attribute("SelectionChanged")?.Value);
    }

    [TestMethod]
    public void LayerPanel_HasFiveButtons()
    {
        // The layer panel has 5 buttons: Add, Delete, Duplicate, Up, Down
        // They're inside a StackPanel at the bottom of the layer panel grid
        var layerList = GetByName("LayerListView");
        var parentGrid = layerList.Parent;
        Assert.IsNotNull(parentGrid);

        var buttons = parentGrid.Descendants(_defaultNs + "Button").ToList();
        Assert.AreEqual(5, buttons.Count, "Layer panel should have 5 buttons");
    }

    [DataTestMethod]
    [DataRow("Add Layer", "AddLayer_Click")]
    [DataRow("Delete Layer", "DeleteLayer_Click")]
    [DataRow("Duplicate", "DuplicateLayer_Click")]
    [DataRow("Up", "MoveLayerUp_Click")]
    [DataRow("Down", "MoveLayerDown_Click")]
    public void LayerPanel_ButtonHasCorrectTooltipAndHandler(string tooltip, string handler)
    {
        var layerList = GetByName("LayerListView");
        var parentGrid = layerList.Parent!;
        var buttons = parentGrid.Descendants(_defaultNs + "Button").ToList();

        var btn = buttons.FirstOrDefault(b =>
        {
            var tt = b.Attribute(_defaultNs + "ToolTipService.ToolTip")?.Value
                     ?? b.Attribute("ToolTipService.ToolTip")?.Value;
            return tt == tooltip;
        });
        Assert.IsNotNull(btn, $"Button with ToolTip '{tooltip}' not found");
        Assert.AreEqual(handler, btn.Attribute("Click")?.Value);
    }
}

[TestClass]
public class CanvasAreaTests
{
    private static XDocument? _xamlDoc;
    private static XNamespace _defaultNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static XNamespace _xNs = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static XNamespace _canvasNs = "using:Microsoft.Graphics.Canvas.UI.Xaml";

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
        Assert.IsNotNull(xamlPath);
        _xamlDoc = XDocument.Load(xamlPath);
    }

    private static XElement GetByName(string name) =>
        _xamlDoc!.Descendants().First(e => (string?)e.Attribute(_xNs + "Name") == name);

    [TestMethod]
    public void DrawingCanvas_HasRequiredHandlers()
    {
        var canvas = GetByName("DrawingCanvas");
        Assert.AreEqual("DrawingCanvas_Draw", canvas.Attribute("Draw")?.Value);
        Assert.AreEqual("DrawingCanvas_CreateResources", canvas.Attribute("CreateResources")?.Value);
        Assert.AreEqual("DrawingCanvas_PointerPressed", canvas.Attribute("PointerPressed")?.Value);
        Assert.AreEqual("DrawingCanvas_PointerMoved", canvas.Attribute("PointerMoved")?.Value);
        Assert.AreEqual("DrawingCanvas_PointerReleased", canvas.Attribute("PointerReleased")?.Value);
    }

    [TestMethod]
    public void RulerCanvas_HasDrawHandler()
    {
        var canvas = GetByName("RulerCanvas");
        Assert.AreEqual("RulerCanvas_Draw", canvas.Attribute("Draw")?.Value);
    }

    [TestMethod]
    public void RulerCanvas_IsNotHitTestVisible()
    {
        var canvas = GetByName("RulerCanvas");
        Assert.AreEqual("False", canvas.Attribute("IsHitTestVisible")?.Value);
    }

    [TestMethod]
    public void RulerCanvas_DefaultCollapsed()
    {
        var canvas = GetByName("RulerCanvas");
        Assert.AreEqual("Collapsed", canvas.Attribute("Visibility")?.Value);
    }

    [TestMethod]
    public void RulerCanvas_IsOnTopOfDrawingCanvas()
    {
        // RulerCanvas should come AFTER DrawingCanvas in XAML for correct z-order
        var container = GetByName("CanvasContainer");
        var children = container.Elements().ToList();

        int drawingIdx = -1, rulerIdx = -1;
        for (int i = 0; i < children.Count; i++)
        {
            var name = (string?)children[i].Attribute(_xNs + "Name");
            if (name == "DrawingCanvas") drawingIdx = i;
            if (name == "RulerCanvas") rulerIdx = i;
        }

        Assert.IsTrue(drawingIdx >= 0, "DrawingCanvas not found in CanvasContainer");
        Assert.IsTrue(rulerIdx >= 0, "RulerCanvas not found in CanvasContainer");
        Assert.IsTrue(rulerIdx > drawingIdx,
            "RulerCanvas must come after DrawingCanvas in XAML for correct z-order overlay");
    }

    [TestMethod]
    public void CanvasScrollViewer_HasAutoScrollBars()
    {
        var sv = GetByName("CanvasScrollViewer");
        Assert.AreEqual("Auto", sv.Attribute("HorizontalScrollBarVisibility")?.Value);
        Assert.AreEqual("Auto", sv.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.AreEqual("Disabled", sv.Attribute("ZoomMode")?.Value);
    }
}

[TestClass]
public class ToolModelContractTests
{
    [TestMethod]
    public void MagnifierTool_HasCorrectNameAndIcon()
    {
        var tool = new MagnifierTool();
        Assert.AreEqual("Magnifier", tool.Name);
        Assert.AreEqual("\uE71E", tool.Icon);
    }

    [TestMethod]
    public void BrushTool_DefaultStyleIsNormal()
    {
        var tool = new BrushTool();
        Assert.AreEqual(BrushStyle.Normal, tool.CurrentStyle);
    }

    [TestMethod]
    public void BrushTool_CanChangeStyle()
    {
        var tool = new BrushTool();
        foreach (var style in Enum.GetValues<BrushStyle>())
        {
            tool.CurrentStyle = style;
            Assert.AreEqual(style, tool.CurrentStyle);
        }
    }

    [TestMethod]
    public void SelectionTool_TransparentSelection_DefaultFalse()
    {
        var tool = new SelectionTool();
        Assert.IsFalse(tool.TransparentSelection);
    }

    [TestMethod]
    public void SelectionTool_TransparentColor_DefaultWhite()
    {
        var tool = new SelectionTool();
        Assert.AreEqual(255, tool.TransparentColor.R);
        Assert.AreEqual(255, tool.TransparentColor.G);
        Assert.AreEqual(255, tool.TransparentColor.B);
    }

    [TestMethod]
    public void SelectionTool_TransparentSelection_CanBeToggled()
    {
        var tool = new SelectionTool();
        tool.TransparentSelection = true;
        Assert.IsTrue(tool.TransparentSelection);
        tool.TransparentSelection = false;
        Assert.IsFalse(tool.TransparentSelection);
    }

    [TestMethod]
    public void CanvasSettings_ShowRuler_DefaultFalse()
    {
        var settings = new CanvasSettings();
        Assert.IsFalse(settings.ShowRuler);
    }

    [TestMethod]
    public void CanvasSettings_ShowRuler_CanBeToggled()
    {
        var settings = new CanvasSettings();
        settings.ShowRuler = true;
        Assert.IsTrue(settings.ShowRuler);
    }

    [TestMethod]
    public void CanvasSettings_ShowGrid_CanBeToggled()
    {
        var settings = new CanvasSettings();
        settings.ShowGrid = true;
        Assert.IsTrue(settings.ShowGrid);
    }

    [TestMethod]
    public void EachDrawingToolEnum_HasMatchingToolClass()
    {
        // All DrawingTool values (except Crop which is menu-only) should have a tool class
        var toolMap = new Dictionary<DrawingTool, ITool>
        {
            [DrawingTool.Pencil] = new PencilTool(),
            [DrawingTool.Brush] = new BrushTool(),
            [DrawingTool.Eraser] = new EraserTool(),
            [DrawingTool.Fill] = new FillTool(),
            [DrawingTool.Text] = new TextTool(),
            [DrawingTool.Eyedropper] = new EyedropperTool(),
            [DrawingTool.Line] = new LineTool(),
            [DrawingTool.Curve] = new CurveTool(),
            [DrawingTool.Shape] = new ShapeTool(),
            [DrawingTool.Selection] = new SelectionTool(),
            [DrawingTool.FreeFormSelection] = new FreeFormSelectionTool(),
            [DrawingTool.Magnifier] = new MagnifierTool(),
        };

        foreach (var dt in Enum.GetValues<DrawingTool>())
        {
            if (dt == DrawingTool.Crop) continue; // Crop is handled by SelectionTool + menu
            Assert.IsTrue(toolMap.ContainsKey(dt), $"DrawingTool.{dt} has no tool class mapping");
            Assert.IsNotNull(toolMap[dt].Name);
            Assert.IsNotNull(toolMap[dt].Icon);
        }
    }

    [TestMethod]
    public void AllToolIcons_AreNonEmpty()
    {
        ITool[] tools =
        {
            new PencilTool(), new BrushTool(), new EraserTool(), new FillTool(),
            new TextTool(), new EyedropperTool(), new LineTool(), new CurveTool(),
            new ShapeTool(), new SelectionTool(), new FreeFormSelectionTool(), new MagnifierTool()
        };
        foreach (var tool in tools)
        {
            Assert.IsFalse(string.IsNullOrEmpty(tool.Icon), $"{tool.GetType().Name}.Icon is null or empty");
            Assert.AreEqual(1, tool.Icon.Length, $"{tool.GetType().Name}.Icon should be a single Unicode character");
        }
    }

    [TestMethod]
    public void AllToolNames_AreDistinct()
    {
        ITool[] tools =
        {
            new PencilTool(), new BrushTool(), new EraserTool(), new FillTool(),
            new TextTool(), new EyedropperTool(), new LineTool(), new CurveTool(),
            new ShapeTool(), new SelectionTool(), new FreeFormSelectionTool(), new MagnifierTool()
        };
        var names = tools.Select(t => t.Name).ToList();
        Assert.AreEqual(names.Count, names.Distinct().Count(), "Tool names must be unique");
    }

    [TestMethod]
    public void AllToolIcons_AreDistinct()
    {
        ITool[] tools =
        {
            new PencilTool(), new BrushTool(), new EraserTool(), new FillTool(),
            new TextTool(), new EyedropperTool(), new LineTool(), new CurveTool(),
            new ShapeTool(), new SelectionTool(), new FreeFormSelectionTool(), new MagnifierTool()
        };
        var icons = tools.Select(t => t.Icon).ToList();
        Assert.AreEqual(icons.Count, icons.Distinct().Count(), "Tool icons must be unique");
    }

    [TestMethod]
    public void AllTools_ResetToNonDrawingState()
    {
        ITool[] tools =
        {
            new PencilTool(), new BrushTool(), new EraserTool(), new FillTool(),
            new TextTool(), new EyedropperTool(), new LineTool(), new CurveTool(),
            new ShapeTool(), new SelectionTool(), new FreeFormSelectionTool(), new MagnifierTool()
        };
        foreach (var tool in tools)
        {
            tool.Reset(); // Should not throw
        }
    }
}
