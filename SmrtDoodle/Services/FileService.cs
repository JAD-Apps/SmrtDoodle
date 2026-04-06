using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using SmrtDoodle.Helpers;
using SmrtDoodle.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace SmrtDoodle.Services;

public class FileService
{
    private readonly Window _window;
    public string? CurrentFilePath { get; set; }
    public bool HasUnsavedChanges { get; set; }

    public FileService(Window window)
    {
        _window = window;
    }

    public async Task<StorageFile?> ShowOpenDialogAsync()
    {
        var picker = new FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        foreach (var kvp in FormatConversionService.OpenFileTypes)
        {
            foreach (var ext in kvp.Value)
                picker.FileTypeFilter.Add(ext);
        }

        return await picker.PickSingleFileAsync();
    }

    public async Task<StorageFile?> ShowSaveDialogAsync()
    {
        var picker = new FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.SuggestedFileName = "Untitled";

        foreach (var kvp in FormatConversionService.SaveFileTypes)
        {
            picker.FileTypeChoices.Add(kvp.Key, kvp.Value);
        }

        return await picker.PickSaveFileAsync();
    }

    /// <summary>
    /// Loads an image file. Uses Magick.NET for advanced formats, Win2D for standard formats.
    /// </summary>
    public async Task<CanvasBitmap?> LoadImageAsync(ICanvasResourceCreator device, StorageFile file)
    {
        // Standard formats handled natively by Win2D
        using var stream = await file.OpenReadAsync();
        return await CanvasBitmap.LoadAsync(device, stream);
    }

    /// <summary>
    /// Loads a file as layers, supporting PSD and other multi-layer formats via Magick.NET.
    /// Returns null if the format doesn't support layers.
    /// </summary>
    public async Task<(List<Layer> layers, int width, int height)?> LoadAsLayersAsync(
        ICanvasResourceCreator device, StorageFile file, float dpi = 96f)
    {
        var ext = Path.GetExtension(file.Name);
        if (FormatConversionService.RequiresMagickForLoad(ext))
        {
            return await FormatConversionService.LoadWithMagickAsync(file, device, dpi);
        }
        return null; // Caller should use LoadImageAsync and wrap in single layer
    }

    /// <summary>
    /// Saves layers to a file. Routes to Magick.NET for advanced formats.
    /// </summary>
    public async Task SaveImageAsync(CanvasRenderTarget renderTarget, StorageFile file,
        IList<Layer>? layers = null, int width = 0, int height = 0, float dpi = 96f,
        Windows.UI.Color backgroundColor = default)
    {
        var ext = Path.GetExtension(file.Name);

        if (FormatConversionService.RequiresMagickForSave(ext) && layers != null)
        {
            await FormatConversionService.SaveWithMagickAsync(file, layers, width, height, dpi, backgroundColor);
        }
        else
        {
            var format = GetBitmapFormat(file.FileType);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            await renderTarget.SaveAsync(stream, format);
        }

        CurrentFilePath = file.Path;
        HasUnsavedChanges = false;
    }

    public async Task SaveToTempFileAsync(CanvasRenderTarget renderTarget, string tempPath)
    {
        var folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(tempPath)!);
        var file = await folder.CreateFileAsync(Path.GetFileName(tempPath), CreationCollisionOption.ReplaceExisting);
        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
        await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
    }

    public static CanvasRenderTarget ComposeLayers(ICanvasResourceCreator device, IList<Layer> layers,
        int width, int height, float dpi, Windows.UI.Color backgroundColor)
    {
        var composite = new CanvasRenderTarget(device, width, height, dpi);
        using (var ds = composite.CreateDrawingSession())
        {
            ds.Clear(backgroundColor);
        }

        foreach (var layer in layers)
        {
            if (layer is not { IsVisible: true, Bitmap: not null }) continue;

            BlendModeHelper.ComposeLayer(composite, layer.Bitmap, layer.BlendMode, layer.Opacity, width, height);
        }
        return composite;
    }

    private static CanvasBitmapFileFormat GetBitmapFormat(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => CanvasBitmapFileFormat.Jpeg,
            ".bmp" => CanvasBitmapFileFormat.Bmp,
            ".gif" => CanvasBitmapFileFormat.Gif,
            _ => CanvasBitmapFileFormat.Png
        };
}
