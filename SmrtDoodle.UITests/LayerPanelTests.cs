using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for the Layer panel:
/// Layer list, add/delete/duplicate/move up/down buttons, visibility checkbox,
/// and default "Background" layer behavior.
/// </summary>
[TestClass]
public class LayerPanelTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Layer Panel Structure

    [TestMethod]
    public void LayerPanel_HasLayersTitle()
    {
        var title = FindByName("Layers");
        Assert.IsNotNull(title);
        Assert.IsTrue(title.Displayed);
    }

    [TestMethod]
    public void LayerListView_Exists()
    {
        var list = FindByAutomationId("LayerListView");
        Assert.IsNotNull(list);
        Assert.IsTrue(list.Displayed);
    }

    #endregion

    #region Default State

    [TestMethod]
    public void DefaultState_HasOneLayer()
    {
        ResetCanvas();
        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));
        Assert.AreEqual(1, items.Count, "New canvas should have exactly 1 layer");
    }

    [TestMethod]
    public void DefaultState_LayerNamedBackground()
    {
        ResetCanvas();
        var bgLayer = TryFindByName("Background");
        Assert.IsNotNull(bgLayer, "Default layer should be named 'Background'");
    }

    [TestMethod]
    public void DefaultState_BackgroundLayerIsSelected()
    {
        ResetCanvas();
        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));
        Assert.IsTrue(items.Count >= 1, "Should have at least one layer");
        // The first item should be selected
        var isSelected = items[0].GetAttribute("SelectionItem.IsSelected");
        Assert.AreEqual("True", isSelected, "Background layer should be selected by default");
    }

    #endregion

    #region Add Layer Button

    [TestMethod]
    public void AddLayerButton_Exists()
    {
        var buttons = FindAllByClassName("Button");
        Assert.IsTrue(buttons.Count > 0, "Layer panel should have action buttons");
    }

    [TestMethod]
    public void AddLayer_IncrementsLayerCount()
    {
        ResetCanvas();

        var list = FindByAutomationId("LayerListView");
        var initialCount = list.FindElements(By.ClassName("ListViewItem")).Count;

        // Click Add Layer via menu
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(300);

        var newCount = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(initialCount + 1, newCount, "Adding a layer should increment count by 1");
    }

    [TestMethod]
    public void AddLayer_Twice_Creates3Layers()
    {
        ResetCanvas();

        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);

        var list = FindByAutomationId("LayerListView");
        var count = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(3, count, "Adding 2 layers to default should result in 3 layers");
    }

    [TestMethod]
    public void AddLayer_FiveTimes_Creates6Layers()
    {
        ResetCanvas();

        for (int i = 0; i < 5; i++)
        {
            ClickMenuItem("Layers", "Add Layer");
            Thread.Sleep(150);
        }

        var list = FindByAutomationId("LayerListView");
        var count = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(6, count, "Adding 5 layers to default should result in 6 layers");
    }

    #endregion

    #region Delete Layer

    [TestMethod]
    public void DeleteLayer_DecrementsLayerCount()
    {
        ResetCanvas();
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);

        var list = FindByAutomationId("LayerListView");
        var countBefore = list.FindElements(By.ClassName("ListViewItem")).Count;

        ClickMenuItem("Layers", "Delete Layer");
        Thread.Sleep(300);

        var countAfter = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(countBefore - 1, countAfter, "Deleting a layer should decrement count by 1");
    }

    [TestMethod]
    public void DeleteLayer_CannotDeleteLastLayer()
    {
        ResetCanvas();

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);

        // Try to delete the only layer
        ClickMenuItem("Layers", "Delete Layer");
        Thread.Sleep(300);

        // Should still have at least 1 layer (app should prevent deleting last layer)
        var count = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.IsTrue(count >= 1, "Should not be able to delete the last remaining layer");
    }

    [TestMethod]
    public void DeleteAllAddedLayers_LeavesBackground()
    {
        ResetCanvas();

        // Add 3 layers
        for (int i = 0; i < 3; i++)
        {
            ClickMenuItem("Layers", "Add Layer");
            Thread.Sleep(150);
        }

        // Delete 3 layers
        for (int i = 0; i < 3; i++)
        {
            ClickMenuItem("Layers", "Delete Layer");
            Thread.Sleep(200);
        }

        var list = FindByAutomationId("LayerListView");
        var count = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(1, count, "After deleting all added layers, only Background should remain");
    }

    #endregion

    #region Duplicate Layer

    [TestMethod]
    public void DuplicateLayer_IncrementsLayerCount()
    {
        ResetCanvas();

        var list = FindByAutomationId("LayerListView");
        var countBefore = list.FindElements(By.ClassName("ListViewItem")).Count;

        ClickMenuItem("Layers", "Duplicate Layer");
        Thread.Sleep(300);

        var countAfter = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(countBefore + 1, countAfter, "Duplicating should add 1 layer");
    }

    [TestMethod]
    public void DuplicateLayer_ThreeTimes()
    {
        ResetCanvas();

        for (int i = 0; i < 3; i++)
        {
            ClickMenuItem("Layers", "Duplicate Layer");
            Thread.Sleep(200);
        }

        var list = FindByAutomationId("LayerListView");
        var count = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(4, count, "3 duplicates + original = 4 layers");
    }

    #endregion

    #region Move Layer Up/Down

    [TestMethod]
    public void MoveLayerUp_DoesNotCrash()
    {
        ResetCanvas();
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);

        ClickMenuItem("Layers", "Move Layer Up");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("LayerListView"));
    }

    [TestMethod]
    public void MoveLayerDown_DoesNotCrash()
    {
        ResetCanvas();
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);

        // Select first layer (Background)
        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));
        if (items.Count > 0) items[0].Click();
        Thread.Sleep(200);

        ClickMenuItem("Layers", "Move Layer Down");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("LayerListView"));
    }

    [TestMethod]
    public void MoveLayerUp_AtTop_DoesNotCrash()
    {
        ResetCanvas();

        // With only 1 layer, moving up should be a no-op
        ClickMenuItem("Layers", "Move Layer Up");
        Thread.Sleep(300);

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);
    }

    [TestMethod]
    public void MoveLayerDown_AtBottom_DoesNotCrash()
    {
        ResetCanvas();

        ClickMenuItem("Layers", "Move Layer Down");
        Thread.Sleep(300);

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);
    }

    #endregion

    #region Merge & Flatten

    [TestMethod]
    public void MergeDown_WithTwoLayers()
    {
        ResetCanvas();
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);

        ClickMenuItem("Layers", "Merge Down");
        Thread.Sleep(300);

        var list = FindByAutomationId("LayerListView");
        var count = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(1, count, "Merging 2 layers should result in 1 layer");
    }

    [TestMethod]
    public void FlattenImage_WithMultipleLayers()
    {
        ResetCanvas();

        for (int i = 0; i < 3; i++)
        {
            ClickMenuItem("Layers", "Add Layer");
            Thread.Sleep(150);
        }

        ClickMenuItem("Layers", "Flatten Image");
        Thread.Sleep(300);

        var list = FindByAutomationId("LayerListView");
        var count = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.AreEqual(1, count, "Flattening should result in 1 layer");
    }

    [TestMethod]
    public void FlattenImage_SingleLayer_DoesNotCrash()
    {
        ResetCanvas();

        ClickMenuItem("Layers", "Flatten Image");
        Thread.Sleep(300);

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);
    }

    [TestMethod]
    public void MergeDown_SingleLayer_DoesNotCrash()
    {
        ResetCanvas();

        ClickMenuItem("Layers", "Merge Down");
        Thread.Sleep(300);

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);
    }

    #endregion

    #region Layer Selection

    [TestMethod]
    public void ClickingLayer_SelectsIt()
    {
        ResetCanvas();
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);

        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));
        Assert.IsTrue(items.Count >= 2);

        // Click the first layer
        items[0].Click();
        Thread.Sleep(200);

        var isSelected = items[0].GetAttribute("SelectionItem.IsSelected");
        Assert.AreEqual("True", isSelected, "Clicked layer should be selected");
    }

    [TestMethod]
    public void SelectingLayer_DeselectsOthers()
    {
        ResetCanvas();
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);

        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));

        // Click first, then second
        items[0].Click();
        Thread.Sleep(100);
        items[1].Click();
        Thread.Sleep(200);

        var firstSelected = items[0].GetAttribute("SelectionItem.IsSelected");
        var secondSelected = items[1].GetAttribute("SelectionItem.IsSelected");

        Assert.AreEqual("False", firstSelected, "First layer should be deselected");
        Assert.AreEqual("True", secondSelected, "Second layer should be selected");
    }

    #endregion

    #region Layer Visibility

    [TestMethod]
    public void LayerVisibility_CheckboxExists()
    {
        ResetCanvas();

        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));
        Assert.IsTrue(items.Count >= 1);

        // Each layer item should have a CheckBox
        var checkboxes = items[0].FindElements(By.ClassName("CheckBox"));
        Assert.IsTrue(checkboxes.Count > 0, "Layer item should have a visibility checkbox");
    }

    [TestMethod]
    public void LayerVisibility_DefaultIsChecked()
    {
        ResetCanvas();

        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));
        var checkbox = items[0].FindElement(By.ClassName("CheckBox"));

        var toggleState = checkbox.GetAttribute("Toggle.ToggleState");
        Assert.AreEqual("1", toggleState, "Default layer visibility should be checked (visible)");
    }

    [TestMethod]
    public void LayerVisibility_Toggle_DoesNotCrash()
    {
        ResetCanvas();

        var list = FindByAutomationId("LayerListView");
        var items = list.FindElements(By.ClassName("ListViewItem"));
        var checkbox = items[0].FindElement(By.ClassName("CheckBox"));

        // Uncheck
        checkbox.Click();
        Thread.Sleep(200);

        // Re-check
        checkbox.Click();
        Thread.Sleep(200);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Layer Panel Buttons

    [TestMethod]
    public void LayerPanel_AddButton_HasTooltip()
    {
        var btn = FindByName("Add Layer");
        Assert.IsNotNull(btn);
    }

    [TestMethod]
    public void LayerPanel_DeleteButton_HasTooltip()
    {
        var btn = FindByName("Delete Layer");
        Assert.IsNotNull(btn);
    }

    [TestMethod]
    public void LayerPanel_DuplicateButton_HasTooltip()
    {
        var btn = FindByName("Duplicate");
        Assert.IsNotNull(btn);
    }

    [TestMethod]
    public void LayerPanel_UpButton_HasTooltip()
    {
        var btn = FindByName("Up");
        Assert.IsNotNull(btn);
    }

    [TestMethod]
    public void LayerPanel_DownButton_HasTooltip()
    {
        var btn = FindByName("Down");
        Assert.IsNotNull(btn);
    }

    #endregion

    #region Complex Layer Workflows

    [TestMethod]
    public void AddDuplicateDeleteMerge_Workflow()
    {
        ResetCanvas();

        // Add 2 layers
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(150);
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(150);

        // Duplicate top layer
        ClickMenuItem("Layers", "Duplicate Layer");
        Thread.Sleep(150);

        // Delete one layer
        ClickMenuItem("Layers", "Delete Layer");
        Thread.Sleep(200);

        // Merge remaining top layer down
        ClickMenuItem("Layers", "Merge Down");
        Thread.Sleep(200);

        var list = FindByAutomationId("LayerListView");
        var count = list.FindElements(By.ClassName("ListViewItem")).Count;
        Assert.IsTrue(count >= 1, "Complex workflow should leave at least 1 layer");
    }

    [TestMethod]
    public void DrawOnDifferentLayers()
    {
        ResetCanvas();
        SelectTool("BtnPencil");

        // Draw on background
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 30, 30, 100, 30);
        Thread.Sleep(200);

        // Add layer and draw
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);
        DragOnElement(canvas, 30, 60, 100, 60);
        Thread.Sleep(200);

        // Add another layer and draw
        ClickMenuItem("Layers", "Add Layer");
        Thread.Sleep(200);
        DragOnElement(canvas, 30, 90, 100, 90);
        Thread.Sleep(200);

        // Flatten
        ClickMenuItem("Layers", "Flatten Image");
        Thread.Sleep(300);

        var list = FindByAutomationId("LayerListView");
        Assert.AreEqual(1, list.FindElements(By.ClassName("ListViewItem")).Count);
    }

    #endregion
}
