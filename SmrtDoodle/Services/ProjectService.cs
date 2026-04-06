using Microsoft.Graphics.Canvas;
using SmrtDoodle.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

namespace SmrtDoodle.Services;

/// <summary>
/// Handles the native .sdd (SmrtDoodle Document) project format.
/// The format is a ZIP archive containing:
///   project.json  — canvas metadata and layer info
///   layers/layer_0.png, layers/layer_1.png, ...  — individual layer bitmaps
///   thumbnail.png — 256px composite preview
/// </summary>
public static class ProjectService
{
    private const string ProjectJsonEntry = "project.json";
    private const string ThumbnailEntry = "thumbnail.png";
    private const string LayerFolder = "layers/";

    /// <summary>
    /// Saves the current project to an .sdd file.
    /// </summary>
    public static async Task SaveProjectAsync(StorageFile file, ProjectData data,
        IList<Layer> layers, ICanvasResourceCreator device)
    {
        using var memStream = new MemoryStream();
        using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            // Write project.json
            var projectJson = new ProjectJson
            {
                Version = "0.5.0",
                Width = data.Width,
                Height = data.Height,
                Dpi = data.Dpi,
                BackgroundColor = ColorToHex(data.BackgroundColor),
                Layers = []
            };

            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                var layerInfo = new LayerJson
                {
                    Index = i,
                    Name = layer.Name,
                    IsVisible = layer.IsVisible,
                    Opacity = layer.Opacity,
                    IsLocked = layer.IsLocked,
                    BlendMode = layer.BlendMode.ToString()
                };
                projectJson.Layers.Add(layerInfo);

                // Save layer bitmap as PNG
                if (layer.Bitmap != null)
                {
                    var entryName = $"{LayerFolder}layer_{i}.png";
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var bitmapStream = new InMemoryRandomAccessStream();
                    await layer.Bitmap.SaveAsync(bitmapStream, CanvasBitmapFileFormat.Png);
                    bitmapStream.Seek(0);
                    await bitmapStream.AsStreamForRead().CopyToAsync(entryStream);
                }
            }

            // Write project.json entry
            var jsonEntry = archive.CreateEntry(ProjectJsonEntry, CompressionLevel.Optimal);
            using (var jsonStream = jsonEntry.Open())
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                await JsonSerializer.SerializeAsync(jsonStream, projectJson, options);
            }

            // Write thumbnail (256px composite preview)
            try
            {
                var composite = FileService.ComposeLayers(device, layers,
                    data.Width, data.Height, data.Dpi, data.BackgroundColor);

                var scale = Math.Min(256f / data.Width, 256f / data.Height);
                var thumbW = (int)(data.Width * scale);
                var thumbH = (int)(data.Height * scale);

                using var thumbnail = new CanvasRenderTarget(device, thumbW, thumbH, 96f);
                using (var ds = thumbnail.CreateDrawingSession())
                {
                    ds.DrawImage(composite, new Windows.Foundation.Rect(0, 0, thumbW, thumbH),
                        new Windows.Foundation.Rect(0, 0, data.Width, data.Height));
                }
                composite.Dispose();

                var thumbEntry = archive.CreateEntry(ThumbnailEntry, CompressionLevel.Optimal);
                using var thumbStream = thumbEntry.Open();
                using var thumbBitmapStream = new InMemoryRandomAccessStream();
                await thumbnail.SaveAsync(thumbBitmapStream, CanvasBitmapFileFormat.Png);
                thumbBitmapStream.Seek(0);
                await thumbBitmapStream.AsStreamForRead().CopyToAsync(thumbStream);
            }
            catch
            {
                // Thumbnail is non-critical — skip on error
            }
        }

        // Write the ZIP bytes to the file
        memStream.Seek(0, SeekOrigin.Begin);
        var bytes = memStream.ToArray();
        await FileIO.WriteBytesAsync(file, bytes);
    }

    /// <summary>
    /// Loads a project from an .sdd file.
    /// </summary>
    public static async Task<(ProjectData data, List<Layer> layers)> LoadProjectAsync(
        StorageFile file, ICanvasResourceCreator device)
    {
        var bytes = await ReadFileBytesAsync(file);

        using var memStream = new MemoryStream(bytes);
        using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);

        // Read project.json
        var jsonEntry = archive.GetEntry(ProjectJsonEntry)
            ?? throw new InvalidDataException("Invalid .sdd file: missing project.json");

        ProjectJson projectJson;
        using (var jsonStream = jsonEntry.Open())
        {
            projectJson = await JsonSerializer.DeserializeAsync<ProjectJson>(jsonStream)
                ?? throw new InvalidDataException("Invalid .sdd file: corrupt project.json");
        }

        var data = new ProjectData
        {
            Width = projectJson.Width,
            Height = projectJson.Height,
            Dpi = projectJson.Dpi,
            BackgroundColor = HexToColor(projectJson.BackgroundColor)
        };

        var layers = new List<Layer>();
        foreach (var layerInfo in projectJson.Layers)
        {
            var layer = new Layer(layerInfo.Name)
            {
                IsVisible = layerInfo.IsVisible,
                Opacity = layerInfo.Opacity,
                IsLocked = layerInfo.IsLocked,
                BlendMode = Enum.TryParse<BlendMode>(layerInfo.BlendMode, out var bm) ? bm : BlendMode.Normal
            };
            layer.Initialize(device, data.Width, data.Height, data.Dpi);

            // Load layer bitmap
            var bitmapEntryName = $"{LayerFolder}layer_{layerInfo.Index}.png";
            var bitmapEntry = archive.GetEntry(bitmapEntryName);
            if (bitmapEntry != null && layer.Bitmap != null)
            {
                using var bitmapStream = bitmapEntry.Open();
                using var randomAccess = new InMemoryRandomAccessStream();
                await bitmapStream.CopyToAsync(randomAccess.AsStreamForWrite());
                randomAccess.Seek(0);

                var loadedBitmap = await CanvasBitmap.LoadAsync(device, randomAccess);
                using var ds = layer.Bitmap.CreateDrawingSession();
                ds.Clear(Microsoft.UI.Colors.Transparent);
                ds.DrawImage(loadedBitmap);
                loadedBitmap.Dispose();
            }

            layers.Add(layer);
        }

        return (data, layers);
    }

    private static string ColorToHex(Color c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

    private static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length < 7) return Color.FromArgb(255, 255, 255, 255);
        hex = hex.TrimStart('#');
        return hex.Length switch
        {
            6 => Color.FromArgb(255,
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16)),
            8 => Color.FromArgb(
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16),
                Convert.ToByte(hex[6..8], 16)),
            _ => Color.FromArgb(255, 255, 255, 255)
        };
    }

    private static async Task<byte[]> ReadFileBytesAsync(StorageFile file)
    {
        using var stream = await file.OpenReadAsync();
        var fileBytes = new byte[stream.Size];
        using var reader = new DataReader(stream);
        await reader.LoadAsync((uint)stream.Size);
        reader.ReadBytes(fileBytes);
        return fileBytes;
    }
}

/// <summary>
/// Canvas settings passed to save/load operations.
/// </summary>
public class ProjectData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public float Dpi { get; set; } = 96f;
    public Color BackgroundColor { get; set; } = Color.FromArgb(255, 255, 255, 255);
}

/// <summary>
/// JSON schema for project.json inside .sdd archive.
/// </summary>
internal class ProjectJson
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.5.0";

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("dpi")]
    public float Dpi { get; set; }

    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "#FFFFFFFF";

    [JsonPropertyName("layers")]
    public List<LayerJson> Layers { get; set; } = [];
}

internal class LayerJson
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("isVisible")]
    public bool IsVisible { get; set; } = true;

    [JsonPropertyName("opacity")]
    public float Opacity { get; set; } = 1f;

    [JsonPropertyName("isLocked")]
    public bool IsLocked { get; set; }

    [JsonPropertyName("blendMode")]
    public string BlendMode { get; set; } = "Normal";
}
