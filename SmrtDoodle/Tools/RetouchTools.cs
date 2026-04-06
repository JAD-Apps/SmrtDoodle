using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tools;

/// <summary>
/// Blur tool — paints a Gaussian blur effect over pixels under the brush.
/// </summary>
public class BlurTool : ToolBase
{
    public override string Name => "Blur";
    public override string Icon => "\uE7B3";

    public int Strength { get; set; } = 3;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        LastPoint = point;
    }

    /// <summary>
    /// Apply blur kernel to pixels within the brush radius on the actual render target.
    /// Must be called with the layer's CanvasRenderTarget, not the drawing session.
    /// </summary>
    public void ApplyBlur(CanvasRenderTarget target, Vector2 center, float radius)
    {
        var w = (int)target.SizeInPixels.Width;
        var h = (int)target.SizeInPixels.Height;
        var pixels = target.GetPixelColors();

        var cx = (int)center.X;
        var cy = (int)center.Y;
        var r = (int)Math.Ceiling(radius);
        var kernelSize = Math.Max(1, Strength);

        int minX = Math.Max(0, cx - r);
        int maxX = Math.Min(w - 1, cx + r);
        int minY = Math.Max(0, cy - r);
        int maxY = Math.Min(h - 1, cy + r);

        var output = (Color[])pixels.Clone();

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                // Check within circular brush
                var dx = x - center.X;
                var dy = y - center.Y;
                if (dx * dx + dy * dy > radius * radius) continue;

                // Box blur kernel averaging
                int sumR = 0, sumG = 0, sumB = 0, sumA = 0, count = 0;
                for (int ky = -kernelSize; ky <= kernelSize; ky++)
                {
                    for (int kx = -kernelSize; kx <= kernelSize; kx++)
                    {
                        var sx = Math.Clamp(x + kx, 0, w - 1);
                        var sy = Math.Clamp(y + ky, 0, h - 1);
                        var c = pixels[sy * w + sx];
                        sumR += c.R; sumG += c.G; sumB += c.B; sumA += c.A;
                        count++;
                    }
                }
                output[y * w + x] = Color.FromArgb(
                    (byte)(sumA / count), (byte)(sumR / count),
                    (byte)(sumG / count), (byte)(sumB / count));
            }
        }

        target.SetPixelColors(output);
    }
}

/// <summary>
/// Sharpen tool — applies an unsharp mask kernel under the brush.
/// </summary>
public class SharpenTool : ToolBase
{
    public override string Name => "Sharpen";
    public override string Icon => "\uE7B3";

    public float Strength { get; set; } = 0.5f;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        LastPoint = point;
    }

    public void ApplySharpen(CanvasRenderTarget target, Vector2 center, float radius)
    {
        var w = (int)target.SizeInPixels.Width;
        var h = (int)target.SizeInPixels.Height;
        var pixels = target.GetPixelColors();

        var cx = (int)center.X;
        var cy = (int)center.Y;
        var r = (int)Math.Ceiling(radius);

        int minX = Math.Max(0, cx - r);
        int maxX = Math.Min(w - 1, cx + r);
        int minY = Math.Max(0, cy - r);
        int maxY = Math.Min(h - 1, cy + r);

        var output = (Color[])pixels.Clone();
        var amount = Strength;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var dx = x - center.X;
                var dy = y - center.Y;
                if (dx * dx + dy * dy > radius * radius) continue;

                // 3x3 blur for unsharp mask
                int sumR = 0, sumG = 0, sumB = 0, count = 0;
                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        var sx = Math.Clamp(x + kx, 0, w - 1);
                        var sy = Math.Clamp(y + ky, 0, h - 1);
                        var c = pixels[sy * w + sx];
                        sumR += c.R; sumG += c.G; sumB += c.B;
                        count++;
                    }
                }
                var orig = pixels[y * w + x];
                var blurR = sumR / count;
                var blurG = sumG / count;
                var blurB = sumB / count;

                // Unsharp mask: original + amount * (original - blur)
                output[y * w + x] = Color.FromArgb(orig.A,
                    (byte)Math.Clamp((int)(orig.R + amount * (orig.R - blurR)), 0, 255),
                    (byte)Math.Clamp((int)(orig.G + amount * (orig.G - blurG)), 0, 255),
                    (byte)Math.Clamp((int)(orig.B + amount * (orig.B - blurB)), 0, 255));
            }
        }

        target.SetPixelColors(output);
    }
}

/// <summary>
/// Smudge tool — samples color at brush start and blends/pushes along drag path.
/// </summary>
public class SmudgeTool : ToolBase
{
    public override string Name => "Smudge";
    public override string Icon => "\uE7E8";

    public float Strength { get; set; } = 0.5f;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        LastPoint = point;
    }

    public void ApplySmudge(CanvasRenderTarget target, Vector2 from, Vector2 to, float radius)
    {
        var w = (int)target.SizeInPixels.Width;
        var h = (int)target.SizeInPixels.Height;
        var pixels = target.GetPixelColors();

        var dist = Vector2.Distance(from, to);
        var steps = Math.Max(1, (int)MathF.Ceiling(dist / Math.Max(1f, radius * 0.3f)));

        for (int i = 0; i <= steps; i++)
        {
            var t = (float)i / steps;
            var pt = Vector2.Lerp(from, to, t);
            var cx = (int)pt.X;
            var cy = (int)pt.Y;
            var r = (int)Math.Ceiling(radius);

            int minX = Math.Max(0, cx - r);
            int maxX = Math.Min(w - 1, cx + r);
            int minY = Math.Max(0, cy - r);
            int maxY = Math.Min(h - 1, cy + r);

            // Sample center color
            if (cx >= 0 && cx < w && cy >= 0 && cy < h)
            {
                var sampleColor = pixels[cy * w + cx];

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        var dx = x - pt.X;
                        var dy = y - pt.Y;
                        if (dx * dx + dy * dy > radius * radius) continue;

                        var distFromCenter = MathF.Sqrt(dx * dx + dy * dy) / radius;
                        var blendFactor = Strength * (1f - distFromCenter);
                        if (blendFactor <= 0) continue;

                        var idx = y * w + x;
                        var orig = pixels[idx];
                        pixels[idx] = Color.FromArgb(orig.A,
                            (byte)(orig.R + (sampleColor.R - orig.R) * blendFactor),
                            (byte)(orig.G + (sampleColor.G - orig.G) * blendFactor),
                            (byte)(orig.B + (sampleColor.B - orig.B) * blendFactor));
                    }
                }
            }
        }

        target.SetPixelColors(pixels);
    }
}
