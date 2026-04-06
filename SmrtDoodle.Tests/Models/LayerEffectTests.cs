using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;
using Windows.UI;

namespace SmrtDoodle.Tests;

[TestClass]
public class LayerEffectTests
{
    [TestMethod]
    public void LayerEffect_HasDefaultValues()
    {
        var effect = new LayerEffect
        {
            Type = LayerEffectType.DropShadow
        };
        Assert.AreEqual(LayerEffectType.DropShadow, effect.Type);
        Assert.IsTrue(effect.IsEnabled);
        Assert.AreEqual(0.75f, effect.Opacity);
        Assert.AreEqual(0f, effect.OffsetX);
        Assert.AreEqual(5f, effect.OffsetY);
        Assert.AreEqual(10f, effect.BlurRadius);
    }

    [TestMethod]
    public void LayerEffect_Clone_CreatesIndependentCopy()
    {
        var original = new LayerEffect
        {
            Type = LayerEffectType.Stroke,
            BlurRadius = 10f,
            Opacity = 0.75f,
            OffsetX = 5f,
            StrokeWidth = 3f,
            StrokePosition = StrokePosition.Inside
        };

        var clone = original.Clone();
        Assert.AreEqual(original.Type, clone.Type);
        Assert.AreEqual(original.BlurRadius, clone.BlurRadius);
        Assert.AreEqual(original.Opacity, clone.Opacity);
        Assert.AreEqual(original.StrokeWidth, clone.StrokeWidth);
        Assert.AreEqual(original.StrokePosition, clone.StrokePosition);

        // Modify clone — original unaffected
        clone.BlurRadius = 20f;
        Assert.AreEqual(10f, original.BlurRadius);
    }

    [TestMethod]
    public void LayerEffect_AllTypesValid()
    {
        var types = new[] { LayerEffectType.DropShadow, LayerEffectType.InnerShadow, LayerEffectType.OuterGlow, LayerEffectType.Stroke };
        foreach (var t in types)
        {
            var effect = new LayerEffect { Type = t };
            Assert.AreEqual(t, effect.Type);
        }
    }

    [TestMethod]
    public void LayerEffect_ToString_IncludesType()
    {
        var effect = new LayerEffect { Type = LayerEffectType.OuterGlow };
        Assert.IsTrue(effect.ToString().Contains("Outer Glow"));
    }
}
