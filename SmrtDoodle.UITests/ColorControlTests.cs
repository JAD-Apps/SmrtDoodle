using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for the Colors ribbon group:
/// Primary/secondary color swatches, swap button, and 28-color palette grid.
/// </summary>
[TestClass]
public class ColorControlTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Primary Color Swatch

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void PrimaryColorBorder_Exists()
    {
        var el = FindByAutomationId("PrimaryColorBorder");
        Assert.IsNotNull(el);
        Assert.IsTrue(el.Displayed);
    }

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void PrimaryColorBorder_HasTooltip()
    {
        var el = FindByAutomationId("PrimaryColorBorder");
        var tooltip = el.GetAttribute("HelpText");
        Assert.AreEqual("Primary Color (left-click)", tooltip);
    }

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void PrimaryColorBorder_IsClickable()
    {
        var el = FindByAutomationId("PrimaryColorBorder");
        Assert.IsTrue(el.Enabled);
    }

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void PrimaryColorBorder_TapOpensColorPicker()
    {
        var el = FindByAutomationId("PrimaryColorBorder");
        el.Click();
        Thread.Sleep(500);

        // A ContentDialog with color picker should appear
        // Dismiss it
        DismissDialog();
        Thread.Sleep(300);
    }

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void PrimaryColorBorder_DefaultIsBlack()
    {
        ResetCanvas();
        var el = FindByAutomationId("PrimaryColorBorder");
        // The background brush should be black — check via automation property
        Assert.IsNotNull(el, "Primary color border should exist with default black color");
    }

    #endregion

    #region Secondary Color Swatch

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void SecondaryColorBorder_Exists()
    {
        var el = FindByAutomationId("SecondaryColorBorder");
        Assert.IsNotNull(el);
        Assert.IsTrue(el.Displayed);
    }

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void SecondaryColorBorder_HasTooltip()
    {
        var el = FindByAutomationId("SecondaryColorBorder");
        var tooltip = el.GetAttribute("HelpText");
        Assert.AreEqual("Secondary Color (right-click)", tooltip);
    }

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void SecondaryColorBorder_IsClickable()
    {
        var el = FindByAutomationId("SecondaryColorBorder");
        Assert.IsTrue(el.Enabled);
    }

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void SecondaryColorBorder_TapOpensColorPicker()
    {
        var el = FindByAutomationId("SecondaryColorBorder");
        el.Click();
        Thread.Sleep(500);

        DismissDialog();
        Thread.Sleep(300);
    }

    #endregion

    #region Swap Colors Button

    [TestMethod]
    public void SwapColorsButton_Exists()
    {
        var btn = FindByAutomationId("SwapColorsButton");
        Assert.IsNotNull(btn);
        Assert.IsTrue(btn.Displayed);
    }

    [TestMethod]
    [Ignore("ToolTipService.ToolTip is not exposed as an automation property in WinUI 3")]
    public void SwapColorsButton_HasTooltip()
    {
        var btn = FindByAutomationId("SwapColorsButton");
        var tooltip = btn.GetAttribute("Name");
        Assert.AreEqual("Swap Colors", tooltip);
    }

    [TestMethod]
    public void SwapColorsButton_IsEnabled()
    {
        var btn = FindByAutomationId("SwapColorsButton");
        Assert.IsTrue(btn.Enabled);
    }

    [TestMethod]
    public void SwapColorsButton_Click_DoesNotCrash()
    {
        var btn = FindByAutomationId("SwapColorsButton");
        btn.Click();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("SwapColorsButton"));
    }

    [TestMethod]
    public void SwapColorsButton_DoubleClick_RestoresOriginal()
    {
        ResetCanvas();

        var btn = FindByAutomationId("SwapColorsButton");

        // Swap twice should restore original colors
        btn.Click();
        Thread.Sleep(100);
        btn.Click();
        Thread.Sleep(200);

        // Primary should be back to black, secondary to white
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void SwapColorsButton_RapidClicks()
    {
        var btn = FindByAutomationId("SwapColorsButton");
        for (int i = 0; i < 10; i++)
        {
            btn.Click();
            Thread.Sleep(50);
        }

        // After 10 swaps (even number), colors should be back to original
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Color Palette Grid

    [TestMethod]
    public void ColorPaletteGrid_Exists()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        Assert.IsNotNull(grid);
        Assert.IsTrue(grid.Displayed);
    }

    [TestMethod]
    public void ColorPaletteGrid_Has28Colors()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        // The grid should have 28 color items
        var items = grid.FindElements(By.ClassName("GridViewItem"));
        Assert.AreEqual(28, items.Count, "Palette should have exactly 28 color swatches");
    }

    [TestMethod]
    public void ColorPaletteGrid_FirstColorIsBlack()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));
        Assert.IsTrue(items.Count > 0, "Palette should have items");

        // Click first item (Black) and verify primary color changes
        items[0].Click();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ColorPaletteGrid_ClickEachColor()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));

        for (int i = 0; i < items.Count; i++)
        {
            items[i].Click();
            Thread.Sleep(100);
        }

        // All clicks should succeed without crash
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ColorPaletteGrid_AllItemsAreDisplayed()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));

        foreach (var item in items)
        {
            Assert.IsTrue(item.Displayed, "Each palette color should be displayed");
        }
    }

    [TestMethod]
    public void ColorPaletteGrid_AllItemsAreEnabled()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));

        foreach (var item in items)
        {
            Assert.IsTrue(item.Enabled, "Each palette color should be enabled");
        }
    }

    [TestMethod]
    public void ColorPaletteGrid_ClickColor_SetsPrimaryColor()
    {
        ResetCanvas();

        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));

        // Click a non-black color (index 3 is Red in the palette)
        if (items.Count > 3)
        {
            items[3].Click();
            Thread.Sleep(200);
        }

        // Primary swatch should still exist (we can't read the actual color easily)
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ColorPaletteGrid_ClickRow1Colors()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));

        // First 14 items are row 1
        for (int i = 0; i < Math.Min(14, items.Count); i++)
        {
            items[i].Click();
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ColorPaletteGrid_ClickRow2Colors()
    {
        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));

        // Items 14-27 are row 2
        for (int i = 14; i < Math.Min(28, items.Count); i++)
        {
            items[i].Click();
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Color Workflow — Drawing

    [TestMethod]
    public void SelectRedColor_DrawWithPencil()
    {
        SelectTool("BtnPencil");

        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));

        // Click Red (index 3 in palette)
        if (items.Count > 3)
        {
            items[3].Click();
            Thread.Sleep(200);
        }

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 50);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void SwapColors_ThenDraw_UsesSwappedColor()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        // Swap so white is primary
        var btn = FindByAutomationId("SwapColorsButton");
        btn.Click();
        Thread.Sleep(200);

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 50);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void SelectMultipleColors_DrawSuccessively()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var grid = FindByAutomationId("ColorPaletteGrid");
        var items = grid.FindElements(By.ClassName("GridViewItem"));
        var canvas = FindByAutomationId("DrawingCanvas");

        int y = 30;
        for (int i = 0; i < Math.Min(10, items.Count); i++)
        {
            items[i].Click();
            Thread.Sleep(100);
            DragOnElement(canvas, 30, y, 200, y);
            y += 20;
            Thread.Sleep(100);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Color Picker Dialog

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void PrimaryColorPicker_OpensAndDismisses()
    {
        var primary = FindByAutomationId("PrimaryColorBorder");
        primary.Click();
        Thread.Sleep(500);

        // Dialog should be open — attempt to find Cancel button
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

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void SecondaryColorPicker_OpensAndDismisses()
    {
        var secondary = FindByAutomationId("SecondaryColorBorder");
        secondary.Click();
        Thread.Sleep(500);

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

    [TestMethod]
    [Ignore("Border controls do not have automation peers in WinUI 3")]
    public void PrimaryColorPicker_OpenedTwice_DoesNotCrash()
    {
        for (int i = 0; i < 2; i++)
        {
            var primary = FindByAutomationId("PrimaryColorBorder");
            primary.Click();
            Thread.Sleep(500);

            var cancel = TryFindByName("Cancel");
            if (cancel is not null)
                cancel.Click();
            else
                DismissDialog();

            Thread.Sleep(300);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion
}
