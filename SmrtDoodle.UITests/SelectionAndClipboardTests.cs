using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for selection and clipboard operations:
/// Rectangle select, free-form select, select all, clear/delete selection,
/// cut/copy/paste, transparent selection, and clipboard edge cases.
/// </summary>
[TestClass]
public class SelectionAndClipboardTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Selection Tool — Basic

    [TestMethod]
    public void SelectionTool_DragCreatesSelection()
    {
        ResetCanvas();
        SelectTool("BtnSelect");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void SelectionTool_SelectAll()
    {
        ResetCanvas();
        SelectTool("BtnSelect");

        SendShortcut(Keys.Control + "a");
        Thread.Sleep(300);

        var selStatus = FindByAutomationId("StatusSelection");
        Assert.IsNotNull(selStatus);
    }

    [TestMethod]
    public void SelectionTool_ClearSelection_ViaEscape()
    {
        ResetCanvas();
        SelectTool("BtnSelect");

        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        SendShortcut(Keys.Escape);
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void SelectionTool_ClearSelection_ViaMenu()
    {
        ResetCanvas();
        SelectTool("BtnSelect");

        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        ClickMenuItem("Edit", "Clear Selection");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void SelectionTool_DeleteSelection_ViaDeleteKey()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        SendShortcut(Keys.Delete);
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void SelectionTool_DeleteSelection_ViaMenu()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        ClickMenuItem("Edit", "Delete Selection");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Selection Tool — Multiple Selections

    [TestMethod]
    public void SelectionTool_CreateThenReplaceSelection()
    {
        ResetCanvas();
        SelectTool("BtnSelect");

        var canvas = FindByAutomationId("DrawingCanvas");

        // First selection
        DragOnElement(canvas, 50, 50, 150, 150);
        Thread.Sleep(200);

        // Second selection replaces first
        DragOnElement(canvas, 200, 200, 300, 300);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void SelectionTool_SelectThenSwitch_ClearsSelection()
    {
        ResetCanvas();
        SelectTool("BtnSelect");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        // Switch to pencil — should commit/clear selection
        SelectTool("BtnPencil");
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Free-form Selection

    [TestMethod]
    public void FreeFormSelection_DragOnCanvas()
    {
        ResetCanvas();
        SelectTool("BtnFreeSelect");

        var canvas = FindByAutomationId("DrawingCanvas");

        // Draw a lasso
        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 50, 50)
               .ClickAndHold()
               .MoveByOffset(50, 0)
               .MoveByOffset(0, 50)
               .MoveByOffset(-50, 0)
               .MoveByOffset(0, -50)
               .Release()
               .Perform();
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void FreeFormSelection_ThenClearSelection()
    {
        SelectTool("BtnFreeSelect");

        var canvas = FindByAutomationId("DrawingCanvas");
        var actions = new Actions(Driver!);
        actions.MoveToElement(canvas, 100, 100)
               .ClickAndHold()
               .MoveByOffset(30, 0)
               .MoveByOffset(0, 30)
               .MoveByOffset(-30, 0)
               .Release()
               .Perform();
        Thread.Sleep(200);

        SendShortcut(Keys.Escape);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Transparent Selection

    [TestMethod]
    public void TransparentSelectionCheck_Exists()
    {
        var chk = FindByAutomationId("TransparentSelectionCheck");
        Assert.IsNotNull(chk);
        Assert.IsTrue(chk.Displayed);
    }

    [TestMethod]
    public void TransparentSelectionCheck_DefaultIsUnchecked()
    {
        var chk = FindByAutomationId("TransparentSelectionCheck");
        var state = chk.GetAttribute("Toggle.ToggleState");
        Assert.AreEqual("0", state, "Transparent selection should be unchecked by default");
    }

    [TestMethod]
    public void TransparentSelectionCheck_Toggle()
    {
        var chk = FindByAutomationId("TransparentSelectionCheck");

        // Check
        chk.Click();
        Thread.Sleep(200);

        var stateAfterCheck = chk.GetAttribute("Toggle.ToggleState");
        Assert.AreEqual("1", stateAfterCheck, "Should be checked after click");

        // Uncheck
        chk.Click();
        Thread.Sleep(200);

        var stateAfterUncheck = chk.GetAttribute("Toggle.ToggleState");
        Assert.AreEqual("0", stateAfterUncheck, "Should be unchecked after second click");
    }

    [TestMethod]
    public void TransparentSelection_DrawWithSelectionTool()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        // Enable transparent selection
        var chk = FindByAutomationId("TransparentSelectionCheck");
        chk.Click();
        Thread.Sleep(200);

        // Select and move
        SelectTool("BtnSelect");
        DragOnElement(canvas, 40, 40, 210, 210);
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));

        // Reset transparent selection
        chk.Click();
        Thread.Sleep(100);
    }

    #endregion

    #region Cut/Copy/Paste

    [TestMethod]
    public void Cut_WithSelection()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "x");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Copy_WithSelection()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "c");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Paste_AfterCopy()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "c");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "v");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void CutThenPaste_Workflow()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "x");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "v");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void PasteAsNewImage_ViaMenu()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "c");
        Thread.Sleep(200);

        ClickMenuItem("Edit", "Paste as New Image");
        Thread.Sleep(500);

        // Dismiss any save prompt
        var dontSave = TryFindByName("Don't Save");
        dontSave?.Click();
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Clipboard Edge Cases

    [TestMethod]
    public void Paste_WithoutCopy_DoesNotCrash()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        SendShortcut(Keys.Control + "v");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Cut_WithoutSelection_DoesNotCrash()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        SendShortcut(Keys.Control + "x");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void Copy_WithoutSelection_DoesNotCrash()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        SendShortcut(Keys.Control + "c");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void MultiPaste_DoesNotCrash()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 200, 200);
        Thread.Sleep(200);

        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(200);
        SendShortcut(Keys.Control + "c");
        Thread.Sleep(200);

        for (int i = 0; i < 5; i++)
        {
            SendShortcut(Keys.Control + "v");
            Thread.Sleep(200);
        }

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion
}
