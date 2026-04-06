using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;

namespace SmrtDoodle.Tests;

[TestClass]
public class LayerTests
{
    [TestMethod]
    public void Layer_HasDefaultProperties()
    {
        var layer = new Layer("Test Layer");
        Assert.AreEqual("Test Layer", layer.Name);
        Assert.IsTrue(layer.IsVisible);
        Assert.AreEqual(1.0f, layer.Opacity);
        Assert.IsFalse(layer.IsLocked);
        Assert.AreEqual(BlendMode.Normal, layer.BlendMode);
        Assert.IsNull(layer.Bitmap);
        Assert.IsFalse(string.IsNullOrEmpty(layer.Id));
        Assert.IsNull(layer.ParentGroupId);
        Assert.IsNull(layer.MaskBitmap);
        Assert.IsFalse(layer.IsMaskEnabled);
        Assert.IsFalse(layer.IsEditingMask);
        Assert.IsNotNull(layer.Effects);
        Assert.AreEqual(0, layer.Effects.Count);
    }

    [TestMethod]
    public void Layer_UniqueIds()
    {
        var layer1 = new Layer("Layer 1");
        var layer2 = new Layer("Layer 2");
        Assert.AreNotEqual(layer1.Id, layer2.Id);
    }

    [TestMethod]
    public void Layer_CanModifyProperties()
    {
        var layer = new Layer("Test")
        {
            IsVisible = false,
            Opacity = 0.5f,
            IsLocked = true,
            BlendMode = BlendMode.Multiply,
            Name = "Modified"
        };
        Assert.AreEqual("Modified", layer.Name);
        Assert.IsFalse(layer.IsVisible);
        Assert.AreEqual(0.5f, layer.Opacity);
        Assert.IsTrue(layer.IsLocked);
        Assert.AreEqual(BlendMode.Multiply, layer.BlendMode);
    }

    [TestMethod]
    public void Layer_DisposeIsIdempotent()
    {
        var layer = new Layer("Test");
        layer.Dispose();
        layer.Dispose(); // Should not throw
    }

    [TestMethod]
    public void Layer_CanSetParentGroupId()
    {
        var layer = new Layer("Child");
        var group = new LayerGroup("Parent");
        layer.ParentGroupId = group.Id;
        Assert.AreEqual(group.Id, layer.ParentGroupId);
    }

    [TestMethod]
    public void Layer_CanAddEffects()
    {
        var layer = new Layer("FX Layer");
        layer.Effects.Add(new LayerEffect { Type = LayerEffectType.DropShadow, BlurRadius = 10 });
        layer.Effects.Add(new LayerEffect { Type = LayerEffectType.Stroke, StrokeWidth = 2 });
        Assert.AreEqual(2, layer.Effects.Count);
        Assert.AreEqual(LayerEffectType.DropShadow, layer.Effects[0].Type);
    }
}
