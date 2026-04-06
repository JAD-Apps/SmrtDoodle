using ImageMagick;
using Microsoft.Graphics.Canvas;
using SmrtDoodle.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

namespace SmrtDoodle.Services;

/// <summary>
/// Handles conversion between internal Win2D bitmaps and external file formats
/// using Magick.NET for format support beyond what Win2D provides natively.
/// Supports PSD, PSDT, TIFF, WebP, ICO, SVG, TGA, DDS, PDF, and 100+ other formats.
/// </summary>
public static class FormatConversionService
{
    /// <summary>
    /// File type filters for the Open File picker.
    /// </summary>
    public static readonly Dictionary<string, IList<string>> OpenFileTypes = new()
    {
        ["Image Files"] = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".psd", ".psdt",
                           ".tiff", ".tif", ".webp", ".ico", ".svg", ".tga", ".dds",
                           ".pdf", ".sdd"],
        ["Photoshop Files"] = [".psd", ".psdt"],
        ["All Files"] = ["*"]
    };

    /// <summary>
    /// File type filters for the Save File picker.
    /// </summary>
    public static readonly Dictionary<string, IList<string>> SaveFileTypes = new()
    {
        ["SmrtDoodle Project"] = [".sdd"],
        ["PNG Image"] = [".png"],
        ["JPEG Image"] = [".jpg", ".jpeg"],
        ["BMP Image"] = [".bmp"],
        ["GIF Image"] = [".gif"],
        ["Photoshop Document"] = [".psd"],
        ["TIFF Image"] = [".tiff", ".tif"],
        ["WebP Image"] = [".webp"],
        ["ICO Icon"] = [".ico"],
        ["TGA Image"] = [".tga"],
        ["DDS Texture"] = [".dds"],
        ["PDF Document"] = [".pdf"],
        ["SVG Vector"] = [".svg"]
    };

    /// <summary>
    /// Determines whether a file extension requires Magick.NET for loading.
    /// </summary>
    public static bool RequiresMagickForLoad(string extension)
    {
        var ext = extension.ToLowerInvariant().TrimStart('.');
        return ext is "psd" or "psdt" or "tiff" or "tif" or "webp" or "ico"
            or "svg" or "tga" or "dds" or "pdf";
    }

    /// <summary>
    /// Determines whether a file extension requires Magick.NET for saving.
    /// </summary>
    public static bool RequiresMagickForSave(string extension)
    {
        var ext = extension.ToLowerInvariant().TrimStart('.');
        return ext is "psd" or "psdt" or "tiff" or "tif" or "webp" or "ico"
            or "svg" or "tga" or "dds" or "pdf";
    }

    /// <summary>
    /// Loads an image file using Magick.NET, returning it as layers.
    /// For PSD files, each PSD layer becomes a SmrtDoodle layer.
    /// For other formats, the image is loaded as a single layer.
    /// </summary>
    public static async Task<(List<Layer> layers, int width, int height)> LoadWithMagickAsync(
        StorageFile file, ICanvasResourceCreator device, float dpi = 96f)
    {
        var layers = new List<Layer>();
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();

        var bytes = await ReadFileBytesAsync(file);

        if (extension is ".psd" or ".psdt")
        {
            return await Task.Run(() => LoadPsdLayers(bytes, device, dpi));
        }

        // Single-image formats
        return await Task.Run(() =>
        {
            using var image = new MagickImage(bytes);
            image.Alpha(AlphaOption.Set);

            var width = (int)image.Width;
            var height = (int)image.Height;

            var layer = new Layer("Background");
            layer.Initialize(device, width, height, dpi);
            CopyMagickToRenderTarget(image, layer.Bitmap!, width, height);

            layers.Add(layer);
            return (layers, width, height);
        });
    }

    /// <summary>
    /// Loads PSD file layers as individual SmrtDoodle layers.
    /// </summary>
    private static (List<Layer> layers, int width, int height) LoadPsdLayers(
        byte[] bytes, ICanvasResourceCreator device, float dpi)
    {
        var layers = new List<Layer>();

        using var psd = new MagickImageCollection(bytes);

        // The first image in the collection is the composite; remaining are layers
        if (psd.Count == 0) return (layers, 0, 0);

        var composite = psd[0];
        var width = (int)composite.Width;
        var height = (int)composite.Height;

        if (psd.Count == 1)
        {
            // No separate layers — just use the composite
            var layer = new Layer("Background");
            layer.Initialize(device, width, height, dpi);
            CopyMagickToRenderTarget(composite, layer.Bitmap!, width, height);
            layers.Add(layer);
        }
        else
        {
            // Skip the composite (index 0) and load each layer
            for (int i = 1; i < psd.Count; i++)
            {
                var psdLayer = psd[i];
                var layerName = string.IsNullOrEmpty(psdLayer.Label) ? $"Layer {i}" : psdLayer.Label;

                // Create a full-size canvas for each PSD layer
                var layer = new Layer(layerName)
                {
                    Opacity = 1.0f
                };
                layer.Initialize(device, width, height, dpi);

                // Composite the PSD layer onto a blank canvas at its correct position
                using var fullLayer = new MagickImage(MagickColors.Transparent, (uint)width, (uint)height);
                fullLayer.Composite(psdLayer, (int)psdLayer.Page.X, (int)psdLayer.Page.Y, CompositeOperator.Over);
                CopyMagickToRenderTarget(fullLayer, layer.Bitmap!, width, height);

                layers.Add(layer);
            }
        }

        // Reverse to match PSD layer order (bottom-to-top)
        layers.Reverse();
        return (layers, width, height);
    }

    /// <summary>
    /// Saves layers to a file using Magick.NET for non-native formats.
    /// </summary>
    public static async Task SaveWithMagickAsync(StorageFile file, IList<Layer> layers,
        int width, int height, float dpi, Color backgroundColor)
    {
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();

        var bytes = await Task.Run(() =>
        {
            if (extension is ".psd" or ".psdt")
            {
                return SaveAsPsd(layers, width, height, dpi, backgroundColor);
            }
            else
            {
                return SaveAsFlatImage(layers, width, height, dpi, backgroundColor, extension);
            }
        });

        // Write bytes to file
        await FileIO.WriteBytesAsync(file, bytes);
    }

    /// <summary>
    /// Saves layers as a PSD file preserving individual layers.
    /// </summary>
    private static byte[] SaveAsPsd(IList<Layer> layers, int width, int height, float dpi, Color backgroundColor)
    {
        using var collection = new MagickImageCollection();

        foreach (var layer in layers)
        {
            if (layer.Bitmap == null) continue;

            var magickLayer = RenderTargetToMagickImage(layer.Bitmap, width, height);
            magickLayer.Label = layer.Name;
            if (!layer.IsVisible)
            {
                // Mark hidden layers — Magick.NET doesn't have a direct visibility flag,
                // but we preserve it in the label for round-tripping .sdd format
                magickLayer.Label = $"[hidden] {layer.Name}";
            }
            collection.Add(magickLayer);
        }

        using var ms = new MemoryStream();
        collection.Write(ms, MagickFormat.Psd);
        return ms.ToArray();
    }

    /// <summary>
    /// Creates a composite flat image from all layers and saves in the requested format.
    /// </summary>
    private static byte[] SaveAsFlatImage(IList<Layer> layers, int width, int height,
        float dpi, Color backgroundColor, string extension)
    {
        using var composite = new MagickImage(
            new MagickColor(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A),
            (uint)width, (uint)height);
        composite.Density = new Density(dpi, dpi);

        foreach (var layer in layers)
        {
            if (layer is not { IsVisible: true, Bitmap: not null }) continue;

            using var magickLayer = RenderTargetToMagickImage(layer.Bitmap, width, height);
            magickLayer.Alpha(AlphaOption.Set);

            // Apply opacity
            if (layer.Opacity < 1.0f)
            {
                magickLayer.Evaluate(Channels.Alpha, EvaluateOperator.Multiply, layer.Opacity);
            }

            composite.Composite(magickLayer, CompositeOperator.Over);
        }

        var format = ExtensionToMagickFormat(extension);
        using var ms = new MemoryStream();
        composite.Write(ms, format);
        return ms.ToArray();
    }

    /// <summary>
    /// Copies pixel data from a MagickImage into a Win2D CanvasRenderTarget.
    /// </summary>
    private static void CopyMagickToRenderTarget(IMagickImage image, CanvasRenderTarget target,
        int width, int height)
    {
        // Convert MagickImage to RGBA byte array, then set pixel colors on the target
        image.Alpha(AlphaOption.Set);

        // Export as raw RGBA bytes (8-bit per channel regardless of Q16)
        var settings = new PixelReadSettings((uint)width, (uint)height, StorageType.Char, PixelMapping.RGBA);
        var rawBytes = image.ToByteArray(MagickFormat.Rgba);

        var colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++)
        {
            var offset = i * 4;
            if (offset + 3 < rawBytes.Length)
            {
                colors[i] = Color.FromArgb(rawBytes[offset + 3], rawBytes[offset], rawBytes[offset + 1], rawBytes[offset + 2]);
            }
        }

        target.SetPixelColors(colors);
    }

    /// <summary>
    /// Converts a Win2D CanvasRenderTarget to a MagickImage.
    /// </summary>
    private static MagickImage RenderTargetToMagickImage(CanvasRenderTarget target, int width, int height)
    {
        var colors = target.GetPixelColors();

        // Build raw RGBA byte array
        var rawBytes = new byte[width * height * 4];
        for (int i = 0; i < colors.Length; i++)
        {
            var offset = i * 4;
            rawBytes[offset] = colors[i].R;
            rawBytes[offset + 1] = colors[i].G;
            rawBytes[offset + 2] = colors[i].B;
            rawBytes[offset + 3] = colors[i].A;
        }

        var readSettings = new PixelReadSettings((uint)width, (uint)height, StorageType.Char, PixelMapping.RGBA);
        var image = new MagickImage();
        image.ReadPixels(rawBytes, readSettings);
        return image;
    }

    private static MagickFormat ExtensionToMagickFormat(string extension) => extension.TrimStart('.') switch
    {
        "psd" or "psdt" => MagickFormat.Psd,
        "tiff" or "tif" => MagickFormat.Tiff,
        "webp" => MagickFormat.WebP,
        "ico" => MagickFormat.Ico,
        "svg" => MagickFormat.Svg,
        "tga" => MagickFormat.Tga,
        "dds" => MagickFormat.Dds,
        "pdf" => MagickFormat.Pdf,
        "png" => MagickFormat.Png,
        "jpg" or "jpeg" => MagickFormat.Jpeg,
        "bmp" => MagickFormat.Bmp,
        "gif" => MagickFormat.Gif,
        _ => MagickFormat.Png
    };

    private static async Task<byte[]> ReadFileBytesAsync(StorageFile file)
    {
        using var stream = await file.OpenReadAsync();
        var bytes = new byte[stream.Size];
        using var reader = new DataReader(stream);
        await reader.LoadAsync((uint)stream.Size);
        reader.ReadBytes(bytes);
        return bytes;
    }
}
