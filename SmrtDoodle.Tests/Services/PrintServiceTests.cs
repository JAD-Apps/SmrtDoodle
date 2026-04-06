using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;
using SmrtDoodle.Services;
using PrintScaleMode = SmrtDoodle.Services.PrintService.PrintScaleMode;

namespace SmrtDoodle.Tests;

[TestClass]
public class PrintServiceTests
{
    [TestMethod]
    public void Default_ScaleMode_FitToPage()
    {
        var svc = new PrintService(new List<Layer>(), new CanvasSettings());
        Assert.AreEqual(PrintScaleMode.FitToPage, svc.ScaleMode);
    }

    [TestMethod]
    public void Default_CustomDpi()
    {
        var svc = new PrintService(new List<Layer>(), new CanvasSettings());
        Assert.AreEqual(300f, svc.CustomDpi);
    }

    [TestMethod]
    public void CalculatePrintSize_FitToPage_Landscape()
    {
        var settings = new CanvasSettings { Width = 1000, Height = 500 };
        var svc = new PrintService(new List<Layer>(), settings);
        svc.ScaleMode = PrintScaleMode.FitToPage;

        var (w, h) = svc.CalculatePrintSize(800, 600);
        Assert.IsTrue(w > 0);
        Assert.IsTrue(h > 0);
        Assert.IsTrue(w <= 800);
        Assert.IsTrue(h <= 600);
    }

    [TestMethod]
    public void CalculatePrintSize_FitToPage_Portrait()
    {
        var settings = new CanvasSettings { Width = 500, Height = 1000 };
        var svc = new PrintService(new List<Layer>(), settings);
        svc.ScaleMode = PrintScaleMode.FitToPage;

        var (w, h) = svc.CalculatePrintSize(800, 600);
        Assert.IsTrue(w > 0);
        Assert.IsTrue(h > 0);
        Assert.IsTrue(w <= 800);
        Assert.IsTrue(h <= 600);
    }

    [TestMethod]
    public void CalculatePrintSize_ActualSize()
    {
        var settings = new CanvasSettings { Width = 400, Height = 300 };
        var svc = new PrintService(new List<Layer>(), settings);
        svc.ScaleMode = PrintScaleMode.ActualSize;

        var (w, h) = svc.CalculatePrintSize(800, 600);
        Assert.AreEqual(400f, w);
        Assert.AreEqual(300f, h);
    }

    [TestMethod]
    public void ScaleMode_CanBeSet()
    {
        var svc = new PrintService(new List<Layer>(), new CanvasSettings());
        svc.ScaleMode = PrintScaleMode.CustomDpi;
        Assert.AreEqual(PrintScaleMode.CustomDpi, svc.ScaleMode);
    }

    [TestMethod]
    public void CustomDpi_CanBeSet()
    {
        var svc = new PrintService(new List<Layer>(), new CanvasSettings());
        svc.CustomDpi = 150f;
        Assert.AreEqual(150f, svc.CustomDpi);
    }

    [TestMethod]
    public void AllPrintScaleModes_Defined()
    {
        var values = Enum.GetValues(typeof(PrintScaleMode));
        Assert.AreEqual(3, values.Length);
    }
}
