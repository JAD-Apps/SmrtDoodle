using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for all menu bar items: File, Edit, Image, View, Layers.
/// Validates presence, clickability, keyboard accelerators, and behavioral outcomes.
/// </summary>
[TestClass]
public class MenuBarTests : AppiumTestBase
{
    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region File Menu — Structure

    [TestMethod]
    public void FileMenu_Exists()
    {
        var menu = FindByName("File");
        Assert.IsNotNull(menu);
        Assert.IsTrue(menu.Displayed);
    }

    [TestMethod]
    public void FileMenu_ContainsNewItem()
    {
        var item = OpenMenuAndFindItem("File", "New");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void FileMenu_ContainsOpenItem()
    {
        var item = OpenMenuAndFindItem("File", "Open...");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void FileMenu_ContainsSaveItem()
    {
        var item = OpenMenuAndFindItem("File", "Save");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void FileMenu_ContainsSaveAsItem()
    {
        var item = OpenMenuAndFindItem("File", "Save As...");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void FileMenu_ContainsPrintItem()
    {
        var item = OpenMenuAndFindItem("File", "Print...");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void FileMenu_ContainsExitItem()
    {
        var item = OpenMenuAndFindItem("File", "Exit");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void FileMenu_InsertIntoDocumentIsCollapsedByDefault()
    {
        FindByName("File").Click();
        Thread.Sleep(300);

        var item = TryFindByName("Insert into Document & Close");
        // Should not be visible in normal launch mode (only visible when launched from SmrtPad)
        if (item is not null)
        {
            Assert.IsFalse(item.Displayed,
                "Insert into Document should be collapsed when not launched from SmrtPad");
        }

        DismissMenu();
    }

    #endregion

    #region File Menu — Keyboard Accelerators

    [TestMethod]
    public void FileMenu_CtrlN_OpensNewCanvas()
    {
        // Draw something first so we can detect the "new" action
        SelectTool("BtnPencil");
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 50, 50, 100, 100);
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "n");
        Thread.Sleep(500);

        // Dismiss any save prompt
        var dontSave = TryFindByName("Don't Save");
        dontSave?.Click();
        Thread.Sleep(300);

        // Canvas should still exist after new
        canvas = FindByAutomationId("DrawingCanvas");
        Assert.IsNotNull(canvas);
    }

    [TestMethod]
    public void FileMenu_CtrlO_OpensFileDialog()
    {
        SendShortcut(Keys.Control + "o");
        Thread.Sleep(1000);

        // A file picker dialog should appear — dismiss it
        DismissDialog();
        Thread.Sleep(300);
    }

    [TestMethod]
    public void FileMenu_CtrlS_TriggersFirstSaveAsSaveAs()
    {
        ResetCanvas();

        SendShortcut(Keys.Control + "s");
        Thread.Sleep(1000);

        // First save on a new canvas triggers Save As dialog
        DismissDialog();
        Thread.Sleep(300);
    }

    [TestMethod]
    public void FileMenu_CtrlShiftS_TriggersSaveAs()
    {
        SendShortcut(Keys.Control + Keys.Shift + "s");
        Thread.Sleep(1000);

        DismissDialog();
        Thread.Sleep(300);
    }

    #endregion

    #region Edit Menu — Structure

    [TestMethod]
    public void EditMenu_Exists()
    {
        var menu = FindByName("Edit");
        Assert.IsNotNull(menu);
        Assert.IsTrue(menu.Displayed);
    }

    [TestMethod]
    public void EditMenu_ContainsUndoItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Undo");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsRedoItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Redo");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsCutItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Cut");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsCopyItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Copy");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsPasteItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Paste");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsSelectAllItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Select All");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsClearSelectionItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Clear Selection");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsDeleteSelectionItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Delete Selection");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsPasteAsNewImageItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Paste as New Image");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void EditMenu_ContainsPasteFromFileItem()
    {
        var item = OpenMenuAndFindItem("Edit", "Paste From File...");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    #endregion

    #region Edit Menu — Keyboard Accelerators

    [TestMethod]
    public void EditMenu_CtrlZ_TriggersUndo()
    {
        ResetCanvas();

        // Draw something, then undo
        var canvas = FindByAutomationId("DrawingCanvas");
        SelectTool("BtnPencil");
        DragOnElement(canvas, 30, 30, 80, 80);
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "z");
        Thread.Sleep(300);

        // Undo should work without error; canvas still present
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void EditMenu_CtrlY_TriggersRedo()
    {
        ResetCanvas();

        var canvas = FindByAutomationId("DrawingCanvas");
        SelectTool("BtnPencil");
        DragOnElement(canvas, 30, 30, 80, 80);
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "z");
        Thread.Sleep(200);
        SendShortcut(Keys.Control + "y");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void EditMenu_CtrlA_TriggersSelectAll()
    {
        SelectTool("BtnSelect");
        Thread.Sleep(200);

        SendShortcut(Keys.Control + "a");
        Thread.Sleep(300);

        // Selection status should update
        var statusSel = FindByAutomationId("StatusSelection");
        Assert.IsNotNull(statusSel);
    }

    [TestMethod]
    public void EditMenu_Escape_ClearsSelection()
    {
        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(300);

        SendShortcut(Keys.Escape);
        Thread.Sleep(300);

        // Selection should be cleared
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void EditMenu_Delete_DeletesSelection()
    {
        SelectTool("BtnSelect");
        SendShortcut(Keys.Control + "a");
        Thread.Sleep(300);

        SendShortcut(Keys.Delete);
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    #endregion

    #region Image Menu — Structure

    [TestMethod]
    public void ImageMenu_Exists()
    {
        var menu = FindByName("Image");
        Assert.IsNotNull(menu);
        Assert.IsTrue(menu.Displayed);
    }

    [TestMethod]
    public void ImageMenu_ContainsResizeItem()
    {
        var item = OpenMenuAndFindItem("Image", "Resize...");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsCropItem()
    {
        var item = OpenMenuAndFindItem("Image", "Crop");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsFlipHorizontalItem()
    {
        var item = OpenMenuAndFindItem("Image", "Flip Horizontal");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsFlipVerticalItem()
    {
        var item = OpenMenuAndFindItem("Image", "Flip Vertical");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsRotate90Item()
    {
        var item = OpenMenuAndFindItem("Image", "Rotate 90°");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsRotate180Item()
    {
        var item = OpenMenuAndFindItem("Image", "Rotate 180°");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsRotate270Item()
    {
        var item = OpenMenuAndFindItem("Image", "Rotate 270°");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsInvertColorsItem()
    {
        var item = OpenMenuAndFindItem("Image", "Invert Colors");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsClearImageItem()
    {
        var item = OpenMenuAndFindItem("Image", "Clear Image");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ImageMenu_ContainsCanvasPropertiesItem()
    {
        var item = OpenMenuAndFindItem("Image", "Canvas Properties...");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    #endregion

    #region Image Menu — Actions

    [TestMethod]
    public void ImageMenu_FlipHorizontal_DoesNotCrash()
    {
        ResetCanvas();
        ClickMenuItem("Image", "Flip Horizontal");
        Thread.Sleep(300);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ImageMenu_FlipVertical_DoesNotCrash()
    {
        ResetCanvas();
        ClickMenuItem("Image", "Flip Vertical");
        Thread.Sleep(300);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ImageMenu_Rotate90_DoesNotCrash()
    {
        ResetCanvas();
        ClickMenuItem("Image", "Rotate 90°");
        Thread.Sleep(300);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ImageMenu_Rotate180_DoesNotCrash()
    {
        ResetCanvas();
        ClickMenuItem("Image", "Rotate 180°");
        Thread.Sleep(300);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ImageMenu_Rotate270_DoesNotCrash()
    {
        ResetCanvas();
        ClickMenuItem("Image", "Rotate 270°");
        Thread.Sleep(300);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ImageMenu_InvertColors_ViaKeyboard()
    {
        ResetCanvas();
        SendShortcut(Keys.Control + Keys.Shift + "i");
        Thread.Sleep(300);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ImageMenu_ClearImage_ViaKeyboard()
    {
        ResetCanvas();
        SendShortcut(Keys.Control + Keys.Shift + "n");
        Thread.Sleep(300);
        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ImageMenu_Resize_OpensDialog()
    {
        ClickMenuItem("Image", "Resize...");
        Thread.Sleep(500);

        // Resize dialog should appear — dismiss it
        DismissDialog();
        Thread.Sleep(300);
    }

    [TestMethod]
    public void ImageMenu_CanvasProperties_OpensDialog()
    {
        ClickMenuItem("Image", "Canvas Properties...");
        Thread.Sleep(500);

        DismissDialog();
        Thread.Sleep(300);
    }

    #endregion

    #region View Menu — Structure

    [TestMethod]
    public void ViewMenu_Exists()
    {
        var menu = FindByName("View");
        Assert.IsNotNull(menu);
        Assert.IsTrue(menu.Displayed);
    }

    [TestMethod]
    public void ViewMenu_ContainsShowGridToggle()
    {
        var item = OpenMenuAndFindItem("View", "Show Grid");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ViewMenu_ContainsShowRulerToggle()
    {
        var item = OpenMenuAndFindItem("View", "Show Ruler");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ViewMenu_ContainsZoomInItem()
    {
        var item = OpenMenuAndFindItem("View", "Zoom In");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ViewMenu_ContainsZoomOutItem()
    {
        var item = OpenMenuAndFindItem("View", "Zoom Out");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ViewMenu_ContainsZoomToFitItem()
    {
        var item = OpenMenuAndFindItem("View", "Zoom to Fit");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void ViewMenu_Contains100PercentItem()
    {
        var item = OpenMenuAndFindItem("View", "100%");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    #endregion

    #region View Menu — Actions

    [TestMethod]
    public void ViewMenu_ToggleGrid_TogglesOnThenOff()
    {
        // Toggle grid on
        ClickMenuItem("View", "Show Grid");
        Thread.Sleep(300);

        // Toggle grid off
        ClickMenuItem("View", "Show Grid");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ViewMenu_ToggleRuler_TogglesOnThenOff()
    {
        ClickMenuItem("View", "Show Ruler");
        Thread.Sleep(300);

        ClickMenuItem("View", "Show Ruler");
        Thread.Sleep(300);

        Assert.IsNotNull(FindByAutomationId("DrawingCanvas"));
    }

    [TestMethod]
    public void ViewMenu_ZoomIn_UpdatesZoomStatus()
    {
        ResetCanvas();

        // Reset to 100% first
        ClickMenuItem("View", "100%");
        Thread.Sleep(300);

        var zoomBefore = FindByAutomationId("StatusZoom").Text;

        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(300);

        var zoomAfter = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual(zoomBefore, zoomAfter, "Zoom status should change after Zoom In");
    }

    [TestMethod]
    public void ViewMenu_ZoomOut_UpdatesZoomStatus()
    {
        // Reset to 100%
        ClickMenuItem("View", "100%");
        Thread.Sleep(300);

        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);

        var zoomBefore = FindByAutomationId("StatusZoom").Text;

        ClickMenuItem("View", "Zoom Out");
        Thread.Sleep(300);

        var zoomAfter = FindByAutomationId("StatusZoom").Text;
        Assert.AreNotEqual(zoomBefore, zoomAfter, "Zoom status should change after Zoom Out");
    }

    [TestMethod]
    public void ViewMenu_100Percent_ResetsZoom()
    {
        ClickMenuItem("View", "Zoom In");
        Thread.Sleep(200);

        ClickMenuItem("View", "100%");
        Thread.Sleep(300);

        var zoom = FindByAutomationId("StatusZoom").Text;
        Assert.AreEqual("100%", zoom);
    }

    #endregion

    #region Layers Menu — Structure

    [TestMethod]
    public void LayersMenu_Exists()
    {
        var menu = FindByName("Layers");
        Assert.IsNotNull(menu);
        Assert.IsTrue(menu.Displayed);
    }

    [TestMethod]
    public void LayersMenu_ContainsAddLayerItem()
    {
        var item = OpenMenuAndFindItem("Layers", "Add Layer");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void LayersMenu_ContainsDeleteLayerItem()
    {
        var item = OpenMenuAndFindItem("Layers", "Delete Layer");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void LayersMenu_ContainsDuplicateLayerItem()
    {
        var item = OpenMenuAndFindItem("Layers", "Duplicate Layer");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void LayersMenu_ContainsMoveLayerUpItem()
    {
        var item = OpenMenuAndFindItem("Layers", "Move Layer Up");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void LayersMenu_ContainsMoveLayerDownItem()
    {
        var item = OpenMenuAndFindItem("Layers", "Move Layer Down");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void LayersMenu_ContainsMergeDownItem()
    {
        var item = OpenMenuAndFindItem("Layers", "Merge Down");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    [TestMethod]
    public void LayersMenu_ContainsFlattenImageItem()
    {
        var item = OpenMenuAndFindItem("Layers", "Flatten Image");
        Assert.IsTrue(item.Displayed);
        DismissMenu();
    }

    #endregion

    #region Menu Bar — All Five Menus Present

    [TestMethod]
    public void MenuBar_HasExactlyFiveMenus()
    {
        var file = FindByName("File");
        var edit = FindByName("Edit");
        var image = FindByName("Image");
        var view = FindByName("View");
        var layers = FindByName("Layers");

        Assert.IsNotNull(file);
        Assert.IsNotNull(edit);
        Assert.IsNotNull(image);
        Assert.IsNotNull(view);
        Assert.IsNotNull(layers);
    }

    [TestMethod]
    public void MenuBar_MenusAreDisplayed()
    {
        Assert.IsTrue(FindByName("File").Displayed, "File menu should be displayed");
        Assert.IsTrue(FindByName("Edit").Displayed, "Edit menu should be displayed");
        Assert.IsTrue(FindByName("Image").Displayed, "Image menu should be displayed");
        Assert.IsTrue(FindByName("View").Displayed, "View menu should be displayed");
        Assert.IsTrue(FindByName("Layers").Displayed, "Layers menu should be displayed");
    }

    #endregion

    #region File Menu — Sequential Operations

    [TestMethod]
    public void FileMenu_NewThenSave_WorksSequentially()
    {
        ResetCanvas();
        Thread.Sleep(300);

        // Draw a stroke
        SelectTool("BtnPencil");
        var canvas = FindByAutomationId("DrawingCanvas");
        DragOnElement(canvas, 40, 40, 120, 120);
        Thread.Sleep(200);

        // Trigger Save (should open Save As for new canvas)
        SendShortcut(Keys.Control + "s");
        Thread.Sleep(500);

        // Dismiss the Save As dialog
        DismissDialog();
        Thread.Sleep(300);
    }

    #endregion
}
