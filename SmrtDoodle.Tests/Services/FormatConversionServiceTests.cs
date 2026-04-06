using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Services;

namespace SmrtDoodle.Tests;

[TestClass]
public class FormatConversionServiceTests
{
    [TestMethod]
    public void OpenFileTypes_NotNull()
    {
        Assert.IsNotNull(FormatConversionService.OpenFileTypes);
        Assert.IsTrue(FormatConversionService.OpenFileTypes.Count > 0);
    }

    [TestMethod]
    public void SaveFileTypes_NotNull()
    {
        Assert.IsNotNull(FormatConversionService.SaveFileTypes);
        Assert.IsTrue(FormatConversionService.SaveFileTypes.Count > 0);
    }

    [TestMethod]
    public void RequiresMagickForLoad_Png_False()
    {
        Assert.IsFalse(FormatConversionService.RequiresMagickForLoad(".png"));
    }

    [TestMethod]
    public void RequiresMagickForLoad_Jpg_False()
    {
        Assert.IsFalse(FormatConversionService.RequiresMagickForLoad(".jpg"));
        Assert.IsFalse(FormatConversionService.RequiresMagickForLoad(".jpeg"));
    }

    [TestMethod]
    public void RequiresMagickForLoad_Bmp_False()
    {
        Assert.IsFalse(FormatConversionService.RequiresMagickForLoad(".bmp"));
    }

    [TestMethod]
    public void RequiresMagickForLoad_Psd_True()
    {
        Assert.IsTrue(FormatConversionService.RequiresMagickForLoad(".psd"));
    }

    [TestMethod]
    public void RequiresMagickForLoad_Tiff_True()
    {
        Assert.IsTrue(FormatConversionService.RequiresMagickForLoad(".tiff"));
        Assert.IsTrue(FormatConversionService.RequiresMagickForLoad(".tif"));
    }

    [TestMethod]
    public void RequiresMagickForLoad_WebP_True()
    {
        Assert.IsTrue(FormatConversionService.RequiresMagickForLoad(".webp"));
    }

    [TestMethod]
    public void RequiresMagickForSave_Png_False()
    {
        Assert.IsFalse(FormatConversionService.RequiresMagickForSave(".png"));
    }

    [TestMethod]
    public void RequiresMagickForSave_Psd_True()
    {
        Assert.IsTrue(FormatConversionService.RequiresMagickForSave(".psd"));
    }

    [TestMethod]
    public void RequiresMagickForSave_WebP_True()
    {
        Assert.IsTrue(FormatConversionService.RequiresMagickForSave(".webp"));
    }

    [TestMethod]
    public void RequiresMagickForLoad_CaseInsensitive()
    {
        Assert.IsTrue(FormatConversionService.RequiresMagickForLoad(".PSD"));
        Assert.IsTrue(FormatConversionService.RequiresMagickForLoad(".Psd"));
    }

    [TestMethod]
    public void RequiresMagickForSave_CaseInsensitive()
    {
        Assert.IsTrue(FormatConversionService.RequiresMagickForSave(".PSD"));
    }
}
