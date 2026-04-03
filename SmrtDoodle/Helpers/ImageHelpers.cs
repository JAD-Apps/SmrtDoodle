using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;

namespace SmrtDoodle.Helpers;

public static class FloodFill
{
    public static void Execute(CanvasRenderTarget target, int x, int y, Color fillColor, int tolerance = 32)
    {
        var w = (int)target.SizeInPixels.Width;
        var h = (int)target.SizeInPixels.Height;
        if (x < 0 || x >= w || y < 0 || y >= h) return;

        var pixels = target.GetPixelColors();
        var targetColor = pixels[y * w + x];

        if (ColorsMatch(targetColor, fillColor, 0)) return;

        var visited = new bool[w * h];
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((x, y));

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            if (cx < 0 || cx >= w || cy < 0 || cy >= h) continue;
            var idx = cy * w + cx;
            if (visited[idx]) continue;
            if (!ColorsMatch(pixels[idx], targetColor, tolerance)) continue;

            visited[idx] = true;
            pixels[idx] = fillColor;

            queue.Enqueue((cx + 1, cy));
            queue.Enqueue((cx - 1, cy));
            queue.Enqueue((cx, cy + 1));
            queue.Enqueue((cx, cy - 1));
        }

        target.SetPixelColors(pixels);
    }

    private static bool ColorsMatch(Color a, Color b, int tolerance) =>
        Math.Abs(a.R - b.R) <= tolerance &&
        Math.Abs(a.G - b.G) <= tolerance &&
        Math.Abs(a.B - b.B) <= tolerance &&
        Math.Abs(a.A - b.A) <= tolerance;
}

public static class ImageTransforms
{
    public static CanvasRenderTarget FlipHorizontal(ICanvasResourceCreator device, CanvasRenderTarget source)
    {
        var w = (int)source.SizeInPixels.Width;
        var h = (int)source.SizeInPixels.Height;
        var result = new CanvasRenderTarget(device, w, h, source.Dpi);
        using var ds = result.CreateDrawingSession();
        ds.Transform = Matrix3x2.CreateScale(-1, 1, new Vector2(w / 2f, h / 2f));
        ds.DrawImage(source);
        return result;
    }

    public static CanvasRenderTarget FlipVertical(ICanvasResourceCreator device, CanvasRenderTarget source)
    {
        var w = (int)source.SizeInPixels.Width;
        var h = (int)source.SizeInPixels.Height;
        var result = new CanvasRenderTarget(device, w, h, source.Dpi);
        using var ds = result.CreateDrawingSession();
        ds.Transform = Matrix3x2.CreateScale(1, -1, new Vector2(w / 2f, h / 2f));
        ds.DrawImage(source);
        return result;
    }

    public static CanvasRenderTarget Rotate90(ICanvasResourceCreator device, CanvasRenderTarget source)
    {
        var w = (int)source.SizeInPixels.Width;
        var h = (int)source.SizeInPixels.Height;
        var result = new CanvasRenderTarget(device, h, w, source.Dpi);
        using var ds = result.CreateDrawingSession();
        ds.Transform = Matrix3x2.CreateRotation(MathF.PI / 2, new Vector2(0, 0)) *
                        Matrix3x2.CreateTranslation(h, 0);
        ds.DrawImage(source);
        return result;
    }

    public static CanvasRenderTarget Rotate180(ICanvasResourceCreator device, CanvasRenderTarget source)
    {
        var w = (int)source.SizeInPixels.Width;
        var h = (int)source.SizeInPixels.Height;
        var result = new CanvasRenderTarget(device, w, h, source.Dpi);
        using var ds = result.CreateDrawingSession();
        ds.Transform = Matrix3x2.CreateRotation(MathF.PI, new Vector2(w / 2f, h / 2f));
        ds.DrawImage(source);
        return result;
    }

    public static CanvasRenderTarget Rotate270(ICanvasResourceCreator device, CanvasRenderTarget source)
    {
        var w = (int)source.SizeInPixels.Width;
        var h = (int)source.SizeInPixels.Height;
        var result = new CanvasRenderTarget(device, h, w, source.Dpi);
        using var ds = result.CreateDrawingSession();
        ds.Transform = Matrix3x2.CreateRotation(-MathF.PI / 2, new Vector2(0, 0)) *
                        Matrix3x2.CreateTranslation(0, w);
        ds.DrawImage(source);
        return result;
    }

    public static CanvasRenderTarget Resize(ICanvasResourceCreator device, CanvasRenderTarget source,
        int newWidth, int newHeight, float dpi)
    {
        var result = new CanvasRenderTarget(device, newWidth, newHeight, dpi);
        using var ds = result.CreateDrawingSession();
        ds.DrawImage(source, new Windows.Foundation.Rect(0, 0, newWidth, newHeight),
            new Windows.Foundation.Rect(0, 0, source.SizeInPixels.Width, source.SizeInPixels.Height));
        return result;
    }

    public static CanvasRenderTarget Crop(ICanvasResourceCreator device, CanvasRenderTarget source,
        Windows.Foundation.Rect cropRect, float dpi)
    {
        var result = new CanvasRenderTarget(device, (float)cropRect.Width, (float)cropRect.Height, dpi);
        using var ds = result.CreateDrawingSession();
        ds.DrawImage(source, new Windows.Foundation.Rect(0, 0, cropRect.Width, cropRect.Height), cropRect);
        return result;
    }

    /// <summary>
    /// Inverts all pixel colors (RGB channels) of the given bitmap in-place.
    /// </summary>
    public static void InvertColors(CanvasRenderTarget target)
    {
        var pixels = target.GetPixelColors();
        for (int i = 0; i < pixels.Length; i++)
        {
            var c = pixels[i];
            pixels[i] = Color.FromArgb(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));
        }
        target.SetPixelColors(pixels);
    }

    /// <summary>
    /// Applies horizontal and vertical skew to a bitmap.
    /// </summary>
    public static CanvasRenderTarget Skew(ICanvasResourceCreator device, CanvasRenderTarget source,
        float skewX, float skewY, float dpi)
    {
        var w = (int)source.SizeInPixels.Width;
        var h = (int)source.SizeInPixels.Height;

        // Calculate new dimensions after skew
        var extraW = (int)(Math.Abs(MathF.Tan(skewX)) * h);
        var extraH = (int)(Math.Abs(MathF.Tan(skewY)) * w);
        var newW = w + extraW;
        var newH = h + extraH;

        var result = new CanvasRenderTarget(device, newW, newH, dpi);
        using var ds = result.CreateDrawingSession();
        ds.Clear(Color.FromArgb(0, 0, 0, 0));

        var skewMatrix = new Matrix3x2(
            1, MathF.Tan(skewY),
            MathF.Tan(skewX), 1,
            skewX < 0 ? 0 : extraW / 2f,
            skewY < 0 ? 0 : extraH / 2f);

        ds.Transform = skewMatrix;
        ds.DrawImage(source);
        return result;
    }
}
