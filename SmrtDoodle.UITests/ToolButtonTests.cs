using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmrtDoodle.UITests;

/// <summary>
/// Comprehensive tests for all 12 tool ToggleButtons in the ribbon.
/// Validates existence, tooltips, selection/deselection, mutual exclusion,
/// default state, status bar updates, and rapid switching.
/// </summary>
[TestClass]
public class ToolButtonTests : AppiumTestBase
{
    private static readonly string[] AllToolIds =
    [
        "BtnPencil", "BtnBrush", "BtnEraser", "BtnFill", "BtnText", "BtnEyedropper",
        "BtnLine", "BtnCurve", "BtnShape", "BtnSelect", "BtnFreeSelect", "BtnMagnifier"
    ];

    private static readonly Dictionary<string, string> ToolTooltips = new()
    {
        ["BtnPencil"] = "Pencil",
        ["BtnBrush"] = "Brush",
        ["BtnEraser"] = "Eraser",
        ["BtnFill"] = "Fill",
        ["BtnText"] = "Text",
        ["BtnEyedropper"] = "Color Picker",
        ["BtnLine"] = "Line",
        ["BtnCurve"] = "Curve",
        ["BtnShape"] = "Shape",
        ["BtnSelect"] = "Select",
        ["BtnFreeSelect"] = "Free-form Select",
        ["BtnMagnifier"] = "Magnifier"
    };

    private static readonly Dictionary<string, string> ToolStatusNames = new()
    {
        ["BtnPencil"] = "Pencil",
        ["BtnBrush"] = "Brush",
        ["BtnEraser"] = "Eraser",
        ["BtnFill"] = "Fill",
        ["BtnText"] = "Text",
        ["BtnEyedropper"] = "Eyedropper",
        ["BtnLine"] = "Line",
        ["BtnCurve"] = "Curve",
        ["BtnShape"] = "Shape",
        ["BtnSelect"] = "Selection",
        ["BtnFreeSelect"] = "FreeFormSelection",
        ["BtnMagnifier"] = "Magnifier"
    };

    [ClassInitialize]
    public static void Setup(TestContext context) => InitializeSession(context);

    [ClassCleanup]
    public static void Cleanup() => TeardownSession();

    #region Tool Button Existence

    [TestMethod]
    public void BtnPencil_Exists() => AssertElementDisplayed("BtnPencil", "Pencil tool button");

    [TestMethod]
    public void BtnBrush_Exists() => AssertElementDisplayed("BtnBrush", "Brush tool button");

    [TestMethod]
    public void BtnEraser_Exists() => AssertElementDisplayed("BtnEraser", "Eraser tool button");

    [TestMethod]
    public void BtnFill_Exists() => AssertElementDisplayed("BtnFill", "Fill tool button");

    [TestMethod]
    public void BtnText_Exists() => AssertElementDisplayed("BtnText", "Text tool button");

    [TestMethod]
    public void BtnEyedropper_Exists() => AssertElementDisplayed("BtnEyedropper", "Eyedropper tool button");

    [TestMethod]
    public void BtnLine_Exists() => AssertElementDisplayed("BtnLine", "Line tool button");

    [TestMethod]
    public void BtnCurve_Exists() => AssertElementDisplayed("BtnCurve", "Curve tool button");

    [TestMethod]
    public void BtnShape_Exists() => AssertElementDisplayed("BtnShape", "Shape tool button");

    [TestMethod]
    public void BtnSelect_Exists() => AssertElementDisplayed("BtnSelect", "Selection tool button");

    [TestMethod]
    public void BtnFreeSelect_Exists() => AssertElementDisplayed("BtnFreeSelect", "Free-form Select tool button");

    [TestMethod]
    public void BtnMagnifier_Exists() => AssertElementDisplayed("BtnMagnifier", "Magnifier tool button");

    #endregion

    #region Default State

    [TestMethod]
    public void DefaultTool_IsPencil()
    {
        ResetCanvas();

        var pencil = FindByAutomationId("BtnPencil");
        Assert.IsTrue(IsToggled(pencil), "Pencil should be the default selected tool");
    }

    [TestMethod]
    public void DefaultTool_StatusBarShowsPencil()
    {
        ResetCanvas();

        var status = FindByAutomationId("StatusTool");
        Assert.AreEqual("Pencil", status.Text, "Default status bar tool should show 'Pencil'");
    }

    [TestMethod]
    public void DefaultTool_OnlyPencilIsSelected()
    {
        ResetCanvas();

        foreach (var toolId in AllToolIds)
        {
            var btn = FindByAutomationId(toolId);
            if (toolId == "BtnPencil")
            {
                Assert.IsTrue(IsToggled(btn), $"{toolId} should be selected by default");
            }
            else
            {
                Assert.IsFalse(IsToggled(btn), $"{toolId} should NOT be selected by default");
            }
        }
    }

    #endregion

    #region Tool Selection — Individual

    [TestMethod]
    public void SelectPencil_TogglesOn()
    {
        SelectTool("BtnBrush"); // Switch away first
        SelectTool("BtnPencil");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnPencil")));
    }

    [TestMethod]
    public void SelectBrush_TogglesOn()
    {
        SelectTool("BtnBrush");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnBrush")));
    }

    [TestMethod]
    public void SelectEraser_TogglesOn()
    {
        SelectTool("BtnEraser");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnEraser")));
    }

    [TestMethod]
    public void SelectFill_TogglesOn()
    {
        SelectTool("BtnFill");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnFill")));
    }

    [TestMethod]
    public void SelectText_TogglesOn()
    {
        SelectTool("BtnText");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnText")));
    }

    [TestMethod]
    public void SelectEyedropper_TogglesOn()
    {
        SelectTool("BtnEyedropper");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnEyedropper")));
    }

    [TestMethod]
    public void SelectLine_TogglesOn()
    {
        SelectTool("BtnLine");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnLine")));
    }

    [TestMethod]
    public void SelectCurve_TogglesOn()
    {
        SelectTool("BtnCurve");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnCurve")));
    }

    [TestMethod]
    public void SelectShape_TogglesOn()
    {
        SelectTool("BtnShape");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnShape")));
    }

    [TestMethod]
    public void SelectSelection_TogglesOn()
    {
        SelectTool("BtnSelect");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnSelect")));
    }

    [TestMethod]
    public void SelectFreeSelect_TogglesOn()
    {
        SelectTool("BtnFreeSelect");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnFreeSelect")));
    }

    [TestMethod]
    public void SelectMagnifier_TogglesOn()
    {
        SelectTool("BtnMagnifier");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnMagnifier")));
    }

    #endregion

    #region Mutual Exclusion

    [TestMethod]
    public void SelectingBrush_DeselectsPencil()
    {
        SelectTool("BtnPencil");
        SelectTool("BtnBrush");

        Assert.IsFalse(IsToggled(FindByAutomationId("BtnPencil")), "Pencil should be deselected when Brush is selected");
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnBrush")), "Brush should be selected");
    }

    [TestMethod]
    public void SelectingEraser_DeselectsBrush()
    {
        SelectTool("BtnBrush");
        SelectTool("BtnEraser");

        Assert.IsFalse(IsToggled(FindByAutomationId("BtnBrush")));
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnEraser")));
    }

    [TestMethod]
    public void MutualExclusion_EachToolDeselectsAllOthers()
    {
        foreach (var toolId in AllToolIds)
        {
            SelectTool(toolId);
            Thread.Sleep(100);

            foreach (var otherId in AllToolIds)
            {
                var btn = FindByAutomationId(otherId);
                if (otherId == toolId)
                {
                    Assert.IsTrue(IsToggled(btn), $"{otherId} should be selected when clicked");
                }
                else
                {
                    Assert.IsFalse(IsToggled(btn), $"{otherId} should be deselected when {toolId} is selected");
                }
            }
        }
    }

    [TestMethod]
    public void SelectingFill_DeselectsLine()
    {
        SelectTool("BtnLine");
        SelectTool("BtnFill");

        Assert.IsFalse(IsToggled(FindByAutomationId("BtnLine")));
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnFill")));
    }

    [TestMethod]
    public void SelectingMagnifier_DeselectsShape()
    {
        SelectTool("BtnShape");
        SelectTool("BtnMagnifier");

        Assert.IsFalse(IsToggled(FindByAutomationId("BtnShape")));
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnMagnifier")));
    }

    [TestMethod]
    public void SelectingText_DeselectsEyedropper()
    {
        SelectTool("BtnEyedropper");
        SelectTool("BtnText");

        Assert.IsFalse(IsToggled(FindByAutomationId("BtnEyedropper")));
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnText")));
    }

    [TestMethod]
    public void SelectingSelect_DeselectsFreeSelect()
    {
        SelectTool("BtnFreeSelect");
        SelectTool("BtnSelect");

        Assert.IsFalse(IsToggled(FindByAutomationId("BtnFreeSelect")));
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnSelect")));
    }

    [TestMethod]
    public void SelectingCurve_DeselectsSelect()
    {
        SelectTool("BtnSelect");
        SelectTool("BtnCurve");

        Assert.IsFalse(IsToggled(FindByAutomationId("BtnSelect")));
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnCurve")));
    }

    #endregion

    #region Status Bar Tool Updates

    [TestMethod]
    public void SelectPencil_StatusBarShowsPencil()
    {
        SelectTool("BtnPencil");
        AssertStatusText("StatusTool", "Pencil", "Pencil tool status");
    }

    [TestMethod]
    public void SelectBrush_StatusBarShowsBrush()
    {
        SelectTool("BtnBrush");
        AssertStatusText("StatusTool", "Brush", "Brush tool status");
    }

    [TestMethod]
    public void SelectEraser_StatusBarShowsEraser()
    {
        SelectTool("BtnEraser");
        AssertStatusText("StatusTool", "Eraser", "Eraser tool status");
    }

    [TestMethod]
    public void SelectFill_StatusBarShowsFill()
    {
        SelectTool("BtnFill");
        AssertStatusText("StatusTool", "Fill", "Fill tool status");
    }

    [TestMethod]
    public void SelectText_StatusBarShowsText()
    {
        SelectTool("BtnText");
        AssertStatusText("StatusTool", "Text", "Text tool status");
    }

    [TestMethod]
    public void SelectEyedropper_StatusBarShowsEyedropper()
    {
        SelectTool("BtnEyedropper");
        AssertStatusText("StatusTool", "Color Picker", "Eyedropper tool status");
    }

    [TestMethod]
    public void SelectLine_StatusBarShowsLine()
    {
        SelectTool("BtnLine");
        AssertStatusText("StatusTool", "Line", "Line tool status");
    }

    [TestMethod]
    public void SelectCurve_StatusBarShowsCurve()
    {
        SelectTool("BtnCurve");
        AssertStatusText("StatusTool", "Curve", "Curve tool status");
    }

    [TestMethod]
    public void SelectShape_StatusBarShowsShape()
    {
        SelectTool("BtnShape");
        AssertStatusText("StatusTool", "Shape", "Shape tool status");
    }

    [TestMethod]
    public void SelectSelection_StatusBarShowsSelection()
    {
        SelectTool("BtnSelect");
        AssertStatusText("StatusTool", "Select", "Selection tool status");
    }

    [TestMethod]
    public void SelectFreeFormSelection_StatusBarShowsFreeFormSelection()
    {
        SelectTool("BtnFreeSelect");
        AssertStatusText("StatusTool", "Free-Form Select", "Free-form selection tool status");
    }

    [TestMethod]
    public void SelectMagnifier_StatusBarShowsMagnifier()
    {
        SelectTool("BtnMagnifier");
        AssertStatusText("StatusTool", "Magnifier", "Magnifier tool status");
    }

    #endregion

    #region Tooltip Verification

    [TestMethod]
    public void BtnPencil_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnPencil");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Pencil tool", name);
    }

    [TestMethod]
    public void BtnBrush_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnBrush");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Brush tool", name);
    }

    [TestMethod]
    public void BtnEraser_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnEraser");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Eraser tool", name);
    }

    [TestMethod]
    public void BtnFill_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnFill");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Fill tool", name);
    }

    [TestMethod]
    public void BtnText_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnText");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Text tool", name);
    }

    [TestMethod]
    public void BtnEyedropper_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnEyedropper");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Eyedropper Color Picker tool", name);
    }

    [TestMethod]
    public void BtnLine_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnLine");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Line tool", name);
    }

    [TestMethod]
    public void BtnCurve_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnCurve");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Curve tool", name);
    }

    [TestMethod]
    public void BtnShape_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnShape");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Shape tool", name);
    }

    [TestMethod]
    public void BtnSelect_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnSelect");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Rectangle Selection tool", name);
    }

    [TestMethod]
    public void BtnFreeSelect_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnFreeSelect");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Free-form Selection tool", name);
    }

    [TestMethod]
    public void BtnMagnifier_HasCorrectTooltip()
    {
        var btn = FindByAutomationId("BtnMagnifier");
        var name = btn.GetAttribute("Name");
        Assert.AreEqual("Magnifier Zoom tool", name);
    }

    #endregion

    #region Rapid Switching

    [TestMethod]
    public void RapidSwitch_AllToolsSequentially()
    {
        foreach (var toolId in AllToolIds)
        {
            SelectTool(toolId);
            Thread.Sleep(50);
        }

        // Final tool (Magnifier) should be selected
        Assert.IsTrue(IsToggled(FindByAutomationId("BtnMagnifier")));
    }

    [TestMethod]
    public void RapidSwitch_PencilBrushEraserThreeTimes()
    {
        for (int i = 0; i < 3; i++)
        {
            SelectTool("BtnPencil");
            Thread.Sleep(50);
            SelectTool("BtnBrush");
            Thread.Sleep(50);
            SelectTool("BtnEraser");
            Thread.Sleep(50);
        }

        Assert.IsTrue(IsToggled(FindByAutomationId("BtnEraser")));
        Assert.IsFalse(IsToggled(FindByAutomationId("BtnPencil")));
        Assert.IsFalse(IsToggled(FindByAutomationId("BtnBrush")));
    }

    [TestMethod]
    public void RapidSwitch_SelectThenBackToPencil()
    {
        SelectTool("BtnSelect");
        Thread.Sleep(50);
        SelectTool("BtnFreeSelect");
        Thread.Sleep(50);
        SelectTool("BtnMagnifier");
        Thread.Sleep(50);
        SelectTool("BtnPencil");
        Thread.Sleep(100);

        Assert.IsTrue(IsToggled(FindByAutomationId("BtnPencil")));
        AssertStatusText("StatusTool", "Pencil", "After rapid switch back to Pencil");
    }

    #endregion

    #region Tool Button Size and Layout

    [TestMethod]
    public void AllToolButtons_AreEnabled()
    {
        foreach (var toolId in AllToolIds)
        {
            var btn = FindByAutomationId(toolId);
            Assert.IsTrue(btn.Enabled, $"{toolId} should be enabled");
        }
    }

    [TestMethod]
    public void AllToolButtons_AreVisible()
    {
        foreach (var toolId in AllToolIds)
        {
            var btn = FindByAutomationId(toolId);
            Assert.IsTrue(btn.Displayed, $"{toolId} should be visible");
        }
    }

    #endregion
}
