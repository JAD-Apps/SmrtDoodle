using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Printing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Printing;
using Windows.UI;

namespace SmrtDoodle.Services;

/// <summary>
/// Print service for WinUI 3 — uses PrintDocument to show system print dialog with preview.
/// Falls back to temp-file + OS launcher if PrintManager is unavailable.
/// </summary>
public class PrintService
{
    private readonly List<Models.Layer> _layers;
    private readonly Models.CanvasSettings _settings;
    private PrintDocument? _printDocument;
    private IPrintDocumentSource? _printDocumentSource;
    private Canvas? _printPage;

    public enum PrintScaleMode
    {
        FitToPage,
        ActualSize,
        CustomDpi
    }

    public PrintScaleMode ScaleMode { get; set; } = PrintScaleMode.FitToPage;
    public float CustomDpi { get; set; } = 300f;

    public PrintService(List<Models.Layer> layers, Models.CanvasSettings settings)
    {
        _layers = layers;
        _settings = settings;
    }

    /// <summary>
    /// Show the system print dialog with preview.
    /// </summary>
    public async Task<bool> PrintAsync(Window window, ICanvasResourceCreator device)
    {
        try
        {
            var composite = ComposeForPrint(device);
            try
            {
                // Create a software bitmap from the composed image
                var pixels = composite.GetPixelBytes();
                var width = (int)composite.SizeInPixels.Width;
                var height = (int)composite.SizeInPixels.Height;

                _printDocument = new PrintDocument();
                _printDocumentSource = _printDocument.DocumentSource;

                _printDocument.Paginate += (_, args) =>
                {
                    _printDocument.SetPreviewPageCount(1, PreviewPageCountType.Final);
                };

                _printDocument.GetPreviewPage += (_, args) =>
                {
                    var page = CreatePrintPage(pixels, width, height);
                    _printDocument.SetPreviewPage(args.PageNumber, page);
                };

                _printDocument.AddPages += (_, args) =>
                {
                    var page = CreatePrintPage(pixels, width, height);
                    _printDocument.AddPage(page);
                    _printDocument.AddPagesComplete();
                };

                // Get the PrintManager for this window
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var printManager = PrintManagerInterop.GetForWindow(hWnd);
                printManager.PrintTaskRequested += PrintManager_PrintTaskRequested;

                try
                {
                    await PrintManagerInterop.ShowPrintUIAsync(hWnd);
                    return true;
                }
                finally
                {
                    printManager.PrintTaskRequested -= PrintManager_PrintTaskRequested;
                }
            }
            finally
            {
                composite.Dispose();
            }
        }
        catch (COMException)
        {
            // PrintManager not available — fall back to temp file approach
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void PrintManager_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
    {
        var printTask = args.Request.CreatePrintTask("SmrtDoodle", e =>
        {
            e.SetSource(_printDocumentSource);
        });
    }

    private Canvas CreatePrintPage(byte[] pixels, int width, int height)
    {
        var bitmap = new WriteableBitmap(width, height);
        using (var stream = bitmap.PixelBuffer.AsStream())
        {
            // Win2D pixels are BGRA, WriteableBitmap expects BGRA — direct copy
            stream.Write(pixels, 0, pixels.Length);
        }
        bitmap.Invalidate();

        var image = new Image
        {
            Source = bitmap,
            Width = width,
            Height = height,
            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform
        };

        var page = new Canvas { Width = width, Height = height };
        page.Children.Add(image);
        return page;
    }

    /// <summary>
    /// Composes all visible layers into a single CanvasRenderTarget for printing.
    /// </summary>
    public CanvasRenderTarget ComposeForPrint(ICanvasResourceCreator device)
    {
        var target = new CanvasRenderTarget(device, _settings.Width, _settings.Height, 96f);
        using var ds = target.CreateDrawingSession();
        ds.Clear(_settings.BackgroundColor);

        foreach (var layer in _layers)
        {
            if (layer is not { IsVisible: true, Bitmap: not null }) continue;
            ds.DrawImage(layer.Bitmap, 0, 0,
                new Rect(0, 0, _settings.Width, _settings.Height), layer.Opacity);
        }

        return target;
    }

    /// <summary>
    /// Calculates print dimensions based on scale mode and page size.
    /// </summary>
    public (float width, float height) CalculatePrintSize(float pageWidth, float pageHeight)
    {
        float imgW = _settings.Width;
        float imgH = _settings.Height;

        switch (ScaleMode)
        {
            case PrintScaleMode.FitToPage:
                var scaleX = pageWidth / imgW;
                var scaleY = pageHeight / imgH;
                var scale = Math.Min(scaleX, scaleY);
                return (imgW * scale, imgH * scale);

            case PrintScaleMode.ActualSize:
                return (imgW, imgH);

            case PrintScaleMode.CustomDpi:
                var dpiScale = 96f / CustomDpi;
                return (imgW * dpiScale, imgH * dpiScale);

            default:
                return (imgW, imgH);
        }
    }
}

/// <summary>
/// Interop helpers for PrintManager in Win32 (WinUI 3 desktop).
/// </summary>
internal static class PrintManagerInterop
{
    [DllImport("Windows.Graphics.Printing.dll", PreserveSig = false)]
    private static extern void GetPrintManagerForWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Interface)] out PrintManager printManager);

    public static PrintManager GetForWindow(IntPtr hwnd)
    {
        GetPrintManagerForWindow(hwnd, out var printManager);
        return printManager;
    }

    public static async Task ShowPrintUIAsync(IntPtr hwnd)
    {
        await PrintManager.ShowPrintUIAsync();
    }
}
