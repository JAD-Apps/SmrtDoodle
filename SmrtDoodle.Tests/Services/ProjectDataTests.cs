using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Services;

namespace SmrtDoodle.Tests;

[TestClass]
public class ProjectDataTests
{
    [TestMethod]
    public void Default_Width()
    {
        var data = new ProjectData();
        Assert.AreEqual(0, data.Width);
    }

    [TestMethod]
    public void Default_Height()
    {
        var data = new ProjectData();
        Assert.AreEqual(0, data.Height);
    }

    [TestMethod]
    public void Default_Dpi()
    {
        var data = new ProjectData();
        Assert.AreEqual(96f, data.Dpi);
    }

    [TestMethod]
    public void Default_BackgroundColor_White()
    {
        var data = new ProjectData();
        Assert.AreEqual(255, data.BackgroundColor.A);
        Assert.AreEqual(255, data.BackgroundColor.R);
        Assert.AreEqual(255, data.BackgroundColor.G);
        Assert.AreEqual(255, data.BackgroundColor.B);
    }

    [TestMethod]
    public void Properties_Settable()
    {
        var data = new ProjectData
        {
            Width = 1920,
            Height = 1080,
            Dpi = 300f
        };
        Assert.AreEqual(1920, data.Width);
        Assert.AreEqual(1080, data.Height);
        Assert.AreEqual(300f, data.Dpi);
    }
}
