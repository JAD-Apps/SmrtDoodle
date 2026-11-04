using Microsoft.Graphics.Canvas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;
using SmrtDoodle.Services;
using SmrtDoodle.Helpers;
using SmrtDoodle.Tools;
using Windows.Storage;
using Windows.UI;

namespace SmrtDoodle.Tests;

[TestClass]
[TestCategory("NativeRendering")]
public class NativeFileRoundTripTests
{
    [DataTestMethod]
    [DataRow(72f)]
    [DataRow(96f)]
    [DataRow(300f)]
    [Timeout(30000)]
    public void CanvasDpi_PreservesDocumentPixelDimensions(float dpi)
    {
        using var device = new CanvasDevice(true);
        var settings = new CanvasSettings { Width = 32, Height = 24, Dpi = dpi };
        using var layer = new Layer("Drawing");
        layer.Initialize(device, settings.Width, settings.Height, settings.Dpi);
        Assert.AreEqual(settings.Width, (int)layer.Bitmap!.SizeInPixels.Width);
        Assert.AreEqual(settings.Height, (int)layer.Bitmap.SizeInPixels.Height);
    }

    [DataTestMethod]
    [DataRow(72f)]
    [DataRow(96f)]
    [DataRow(300f)]
    [Timeout(30000)]
    public async Task NativeProject_RoundTripsLayerPixelsAndMetadata(float dpi)
    {
        using var device = new CanvasDevice(true);
        using var lower = MakeLayer(device, "Lower", Color.FromArgb(255, 29, 113, 207));
        using var upper = MakeLayer(device, "Upper", Color.FromArgb(255, 193, 47, 91));
        upper.IsVisible = false;
        upper.IsLocked = true;
        upper.Opacity = 0.4f;
        upper.BlendMode = BlendMode.Multiply;
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sdd");
        List<Layer>? restored = null;
        try
        {
            var file = await CreateFileAsync(path);
            await ProjectService.SaveProjectAsync(file,
                new ProjectData { Width = 8, Height = 8, Dpi = dpi, BackgroundColor = Color.FromArgb(0, 0, 0, 0) },
                new[] { lower, upper }, device);
            var loaded = await ProjectService.LoadProjectAsync(file, device);
            restored = loaded.layers;
            Assert.AreEqual(2, restored.Count);
            Assert.AreEqual(8, loaded.data.Width);
            Assert.AreEqual(dpi, loaded.data.Dpi);
            CollectionAssert.AreEqual(lower.Bitmap!.GetPixelColors(), restored[0].Bitmap!.GetPixelColors());
            CollectionAssert.AreEqual(upper.Bitmap!.GetPixelColors(), restored[1].Bitmap!.GetPixelColors());
            Assert.AreEqual("Upper", restored[1].Name);
            Assert.IsFalse(restored[1].IsVisible);
            Assert.IsTrue(restored[1].IsLocked);
            Assert.AreEqual(upper.Opacity, restored[1].Opacity);
            Assert.AreEqual(upper.BlendMode, restored[1].BlendMode);
        }
        finally
        {
            if (restored is not null) foreach (var layer in restored) layer.Dispose();
            File.Delete(path);
        }
    }

    [DataTestMethod]
    [DataRow(".tiff", 255)]
    [DataRow(".psd", 255)]
    [DataRow(".tiff", 128)]
    [DataRow(".psd", 128)]
    [Timeout(30000)]
    public async Task LosslessExport_RoundTripsLayerPixels(string extension, int alpha)
    {
        using var device = new CanvasDevice(true);
        using var layer = MakeLayer(device, "Drawing", Color.FromArgb((byte)alpha, 29, 53, 101));
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        List<Layer>? restored = null;
        try
        {
            var file = await CreateFileAsync(path);
            await FormatConversionService.SaveWithMagickAsync(file, new[] { layer }, 8, 8, 96,
                Color.FromArgb(0, 0, 0, 0));
            var loaded = await FormatConversionService.LoadWithMagickAsync(file, device);
            restored = loaded.layers;
            Assert.AreEqual(8, loaded.width);
            Assert.AreEqual(8, loaded.height);
            Assert.AreEqual(1, restored.Count);
            CollectionAssert.AreEqual(layer.Bitmap!.GetPixelColors(), restored[0].Bitmap!.GetPixelColors());
        }
        finally
        {
            if (restored is not null) foreach (var item in restored) item.Dispose();
            File.Delete(path);
        }
    }

    [TestMethod]
    [Timeout(30000)]
    public async Task PhotoshopExport_PreservesBothLayers()
    {
        using var device = new CanvasDevice(true);
        using var lower = MakeLayer(device, "Lower", Color.FromArgb(255, 29, 113, 207));
        using var upper = MakeLayer(device, "Upper", Color.FromArgb(255, 193, 47, 91));
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".psd");
        List<Layer>? restored = null;
        try
        {
            var file = await CreateFileAsync(path);
            await FormatConversionService.SaveWithMagickAsync(file, new[] { lower, upper }, 8, 8, 96,
                Color.FromArgb(0, 0, 0, 0));
            var loaded = await FormatConversionService.LoadWithMagickAsync(file, device);
            restored = loaded.layers;
            Assert.AreEqual(2, restored.Count, "The PSD composite must not replace a drawing layer.");
            CollectionAssert.AreEqual(lower.Bitmap!.GetPixelColors(), restored[0].Bitmap!.GetPixelColors());
            CollectionAssert.AreEqual(upper.Bitmap!.GetPixelColors(), restored[1].Bitmap!.GetPixelColors());
        }
        finally
        {
            if (restored is not null) foreach (var layer in restored) layer.Dispose();
            File.Delete(path);
        }
    }

    [DataTestMethod]
    [DataRow(72f)]
    [DataRow(300f)]
    [Timeout(30000)]
    public void EditingAtPrintDpi_PreservesPixelsAndUndo(float dpi)
    {
        using var device = new CanvasDevice(true);
        using var layer = new Layer("Drawing");
        layer.Initialize(device, 8, 8, dpi);
        layer.InitializeMask(device, 8, 8, dpi);
        var colour = Color.FromArgb(255, 29, 113, 207);
        layer.Bitmap!.SetPixelColors(Enumerable.Repeat(colour, 64).ToArray());
        var before = layer.Bitmap.GetPixelColors();
        Assert.AreEqual(8, (int)layer.MaskBitmap!.SizeInPixels.Width);
        using var clone = layer.Clone(device);
        CollectionAssert.AreEqual(before, clone.Bitmap!.GetPixelColors());
        using var resized = ImageTransforms.Resize(device, layer.Bitmap, 16, 12, dpi);
        Assert.AreEqual(16, (int)resized.SizeInPixels.Width);
        Assert.AreEqual(12, (int)resized.SizeInPixels.Height);
        using var cropped = ImageTransforms.Crop(device, layer.Bitmap, new Windows.Foundation.Rect(0, 0, 4, 3), dpi);
        Assert.AreEqual(4, (int)cropped.SizeInPixels.Width);
        Assert.AreEqual(3, (int)cropped.SizeInPixels.Height);
        using var skewed = ImageTransforms.Skew(device, layer.Bitmap, 0, 0, dpi);
        CollectionAssert.AreEqual(before, skewed.GetPixelColors());
        using var undo = new BitmapUndoAction(layer, device, "Move selection");
        undo.CaptureBeforeState();
        var selection = new SelectionTool { SelectionRect = new Windows.Foundation.Rect(0, 0, 4, 3) };
        try
        {
            selection.LiftPixels(layer.Bitmap, dpi);
            Assert.AreEqual(4, (int)selection.SelectionBitmap!.SizeInPixels.Width);
            Assert.AreEqual(3, (int)selection.SelectionBitmap.SizeInPixels.Height);
            Assert.AreEqual((byte)0, layer.Bitmap.GetPixelColors()[0].A);
            Assert.AreEqual(colour, selection.SelectionBitmap.GetPixelColors()[0]);
            undo.CaptureAfterState();
            undo.Undo();
            CollectionAssert.AreEqual(before, layer.Bitmap.GetPixelColors());
            undo.Redo();
            Assert.AreEqual((byte)0, layer.Bitmap.GetPixelColors()[0].A);
        }
        finally { selection.Reset(); }
    }

    private static Layer MakeLayer(CanvasDevice device, string name, Color colour)
    {
        var layer = new Layer(name);
        layer.Initialize(device, 8, 8, 96);
        layer.Bitmap!.SetPixelColors(Enumerable.Repeat(colour, 64).ToArray());
        return layer;
    }

    private static async Task<StorageFile> CreateFileAsync(string path)
    {
        await File.WriteAllBytesAsync(path, Array.Empty<byte>());
        return await StorageFile.GetFileFromPathAsync(path);
    }
}
