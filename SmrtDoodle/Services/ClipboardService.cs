using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using SmrtDoodle.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SmrtDoodle.Services;

public class ClipboardService
{
    public async Task CopyToClipboardAsync(CanvasRenderTarget bitmap)
    {
        var stream = await ConvertToStreamAsync(bitmap);
        var package = new DataPackage();
        package.RequestedOperation = DataPackageOperation.Copy;
        var reference = RandomAccessStreamReference.CreateFromStream(stream);
        package.SetBitmap(reference);
        Clipboard.SetContent(package);
    }

    public async Task<CanvasBitmap?> PasteFromClipboard(ICanvasResourceCreator device)
    {
        var content = Clipboard.GetContent();
        if (content.Contains(StandardDataFormats.Bitmap))
        {
            var reference = await content.GetBitmapAsync();
            using var stream = await reference.OpenReadAsync();
            return await CanvasBitmap.LoadAsync(device, stream);
        }
        return null;
    }

    private static async Task<IRandomAccessStream> ConvertToStreamAsync(CanvasRenderTarget bitmap)
    {
        var stream = new InMemoryRandomAccessStream();
        await bitmap.SaveAsync(stream, CanvasBitmapFileFormat.Png);
        stream.Seek(0);
        return stream;
    }
}
