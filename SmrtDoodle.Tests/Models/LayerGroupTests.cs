using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;

namespace SmrtDoodle.Tests;

[TestClass]
public class LayerGroupTests
{
    [TestMethod]
    public void LayerGroup_HasDefaultProperties()
    {
        var group = new LayerGroup("Group 1");
        Assert.AreEqual("Group 1", group.Name);
        Assert.IsTrue(group.IsVisible);
        Assert.IsTrue(group.IsExpanded);
        Assert.AreEqual(1.0f, group.Opacity);
        Assert.AreEqual(BlendMode.Normal, group.BlendMode);
        Assert.AreEqual(0, group.ChildLayerIds.Count);
        Assert.IsFalse(string.IsNullOrEmpty(group.Id));
    }

    [TestMethod]
    public void LayerGroup_CanAddChildIds()
    {
        var group = new LayerGroup("Group");
        var layer1 = new Layer("L1");
        var layer2 = new Layer("L2");
        group.ChildLayerIds.Add(layer1.Id);
        group.ChildLayerIds.Add(layer2.Id);
        Assert.AreEqual(2, group.ChildLayerIds.Count);
        Assert.AreEqual(layer1.Id, group.ChildLayerIds[0]);
    }

    [TestMethod]
    public void LayerGroup_ToggleVisibility()
    {
        var group = new LayerGroup("Group");
        Assert.IsTrue(group.IsVisible);
        group.ToggleVisibility();
        Assert.IsFalse(group.IsVisible);
        group.ToggleVisibility();
        Assert.IsTrue(group.IsVisible);
    }

    [TestMethod]
    public void LayerGroup_UniqueIds()
    {
        var g1 = new LayerGroup("A");
        var g2 = new LayerGroup("B");
        Assert.AreNotEqual(g1.Id, g2.Id);
    }

    [TestMethod]
    public void LayerGroup_ToStringIncludesCount()
    {
        var group = new LayerGroup("Shapes");
        group.ChildLayerIds.Add("id1");
        group.ChildLayerIds.Add("id2");
        var str = group.ToString();
        Assert.IsTrue(str.Contains("Shapes"));
        Assert.IsTrue(str.Contains("2"));
    }
}
