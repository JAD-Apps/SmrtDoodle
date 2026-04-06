using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;
using Windows.UI;

namespace SmrtDoodle.Tests;

[TestClass]
public class AdjustmentLayerTests
{
    [TestMethod]
    public void AdjustmentLayer_HasDefaults()
    {
        var adj = new AdjustmentLayer("Brightness", AdjustmentType.BrightnessContrast);
        Assert.AreEqual("Brightness", adj.Name);
        Assert.AreEqual(AdjustmentType.BrightnessContrast, adj.AdjustmentType);
        Assert.AreEqual(0f, adj.Brightness);
        Assert.AreEqual(0f, adj.Contrast);
    }

    [TestMethod]
    public void AdjustmentLayer_BrightnessContrast_NeutralPassthrough()
    {
        var adj = new AdjustmentLayer("BC", AdjustmentType.BrightnessContrast)
        {
            Brightness = 0,
            Contrast = 0
        };
        var input = Color.FromArgb(255, 128, 64, 200);
        var output = adj.ApplyToPixel(input);
        Assert.AreEqual(input.R, output.R);
        Assert.AreEqual(input.G, output.G);
        Assert.AreEqual(input.B, output.B);
    }

    [TestMethod]
    public void AdjustmentLayer_Brightness_ClampsTo255()
    {
        var adj = new AdjustmentLayer("BC", AdjustmentType.BrightnessContrast)
        {
            Brightness = 100
        };
        var input = Color.FromArgb(255, 250, 250, 250);
        var output = adj.ApplyToPixel(input);
        Assert.AreEqual(255, output.R);
        Assert.AreEqual(255, output.G);
        Assert.AreEqual(255, output.B);
    }

    [TestMethod]
    public void AdjustmentLayer_HSL_NeutralPassthrough()
    {
        var adj = new AdjustmentLayer("HSL", AdjustmentType.HueSaturationLightness);
        var input = Color.FromArgb(255, 100, 150, 200);
        var output = adj.ApplyToPixel(input);
        // With neutral settings, output should be very close to input
        Assert.IsTrue(Math.Abs(input.R - output.R) <= 1);
        Assert.IsTrue(Math.Abs(input.G - output.G) <= 1);
        Assert.IsTrue(Math.Abs(input.B - output.B) <= 1);
    }

    [TestMethod]
    public void AdjustmentLayer_Levels_DefaultPassthrough()
    {
        var adj = new AdjustmentLayer("Levels", AdjustmentType.Levels);
        var input = Color.FromArgb(255, 128, 64, 200);
        var output = adj.ApplyToPixel(input);
        Assert.IsTrue(Math.Abs(input.R - output.R) <= 1);
        Assert.IsTrue(Math.Abs(input.G - output.G) <= 1);
        Assert.IsTrue(Math.Abs(input.B - output.B) <= 1);
    }

    [TestMethod]
    public void AdjustmentLayer_ColorBalance_ShiftsColor()
    {
        var adj = new AdjustmentLayer("CB", AdjustmentType.ColorBalance)
        {
            CyanRed = 50  // Shift toward red
        };
        var input = Color.FromArgb(255, 100, 100, 100);
        var output = adj.ApplyToPixel(input);
        Assert.IsTrue(output.R > input.R);
        Assert.AreEqual(input.G, output.G);
        Assert.AreEqual(input.B, output.B);
    }

    [TestMethod]
    public void AdjustmentLayer_IsSubclassOfLayer()
    {
        var adj = new AdjustmentLayer("Test", AdjustmentType.Curves);
        Assert.IsInstanceOfType(adj, typeof(Layer));
    }

    [TestMethod]
    public void AdjustmentLayer_InheritsLayerProperties()
    {
        var adj = new AdjustmentLayer("Test", AdjustmentType.BrightnessContrast)
        {
            IsVisible = false,
            Opacity = 0.5f,
            BlendMode = BlendMode.Overlay
        };
        Assert.IsFalse(adj.IsVisible);
        Assert.AreEqual(0.5f, adj.Opacity);
        Assert.AreEqual(BlendMode.Overlay, adj.BlendMode);
    }
}
