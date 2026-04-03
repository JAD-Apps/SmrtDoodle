using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Helpers;
using Microsoft.UI.Xaml;

namespace SmrtDoodle.Tests;

[TestClass]
public class ConverterTests
{
    [TestMethod]
    public void BoolToVisibility_True_ReturnsVisible()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(true, typeof(Visibility), null!, "");
        Assert.AreEqual(Visibility.Visible, result);
    }

    [TestMethod]
    public void BoolToVisibility_False_ReturnsCollapsed()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(false, typeof(Visibility), null!, "");
        Assert.AreEqual(Visibility.Collapsed, result);
    }

    [TestMethod]
    public void BoolToVisibility_Inverted_True_ReturnsCollapsed()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(true, typeof(Visibility), "Invert", "");
        Assert.AreEqual(Visibility.Collapsed, result);
    }

    [TestMethod]
    public void BoolToVisibility_Inverted_False_ReturnsVisible()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(false, typeof(Visibility), "Invert", "");
        Assert.AreEqual(Visibility.Visible, result);
    }

    [TestMethod]
    public void BoolToVisibility_ConvertBack_Visible_ReturnsTrue()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, "");
        Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void BoolToVisibility_ConvertBack_Collapsed_ReturnsFalse()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, "");
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void PercentageConverter_Float_FormatsCorrectly()
    {
        var converter = new PercentageConverter();
        var result = converter.Convert(1.0f, typeof(string), null!, "");
        Assert.AreEqual("100%", result);
    }

    [TestMethod]
    public void PercentageConverter_Half_FormatsCorrectly()
    {
        var converter = new PercentageConverter();
        var result = converter.Convert(0.5f, typeof(string), null!, "");
        Assert.AreEqual("50%", result);
    }

    [TestMethod]
    public void PercentageConverter_Double_FormatsCorrectly()
    {
        var converter = new PercentageConverter();
        var result = converter.Convert(0.75d, typeof(string), null!, "");
        Assert.AreEqual("75%", result);
    }

    [TestMethod]
    public void PercentageConverter_ConvertBack_ValidString()
    {
        var converter = new PercentageConverter();
        var result = converter.ConvertBack("50%", typeof(float), null!, "");
        Assert.AreEqual(0.5f, result);
    }

    [TestMethod]
    public void PercentageConverter_ConvertBack_InvalidString()
    {
        var converter = new PercentageConverter();
        var result = converter.ConvertBack("abc", typeof(float), null!, "");
        Assert.AreEqual(1.0f, result);
    }

    [TestMethod]
    public void PercentageConverter_NonNumeric_ReturnsDefault()
    {
        var converter = new PercentageConverter();
        var result = converter.Convert("not a number", typeof(string), null!, "");
        Assert.AreEqual("100%", result);
    }
}
