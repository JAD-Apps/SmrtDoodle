using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
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
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");
        return await picker.PickSingleFileAsync();
    }

    public async Task<StorageFile?> ShowSaveDialogAsync()
    {
        var picker = new FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.SuggestedFileName = "Untitled";
        picker.FileTypeChoices.Add("PNG Image", new List<string> { ".png" });
        picker.FileTypeChoices.Add("JPEG Image", new List<string> { ".jpg" });
        picker.FileTypeChoices.Add("BMP Image", new List<string> { ".bmp" });
        return await picker.PickSaveFileAsync();
    }

    public async Task<CanvasBitmap?> LoadImageAsync(ICanvasResourceCreator device, StorageFile file)
    {
        using var stream = await file.OpenReadAsync();
        return await CanvasBitmap.LoadAsync(device, stream);
    }

    public async Task SaveImageAsync(CanvasRenderTarget renderTarget, StorageFile file)
    {
        var format = GetBitmapFormat(file.FileType);
        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
        await renderTarget.SaveAsync(stream, format);
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
        using var ds = composite.CreateDrawingSession();
        ds.Clear(backgroundColor);
        foreach (var layer in layers)
        {
            if (layer is { IsVisible: true, Bitmap: not null })
            {
                ds.DrawImage(layer.Bitmap, 0, 0, new Windows.Foundation.Rect(0, 0, width, height),
                    layer.Opacity);
            }
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
