using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tools;

/// <summary>
/// Clone Stamp tool — Alt+Click to set source point, then paint to copy source region to target.
/// </summary>
public class CloneStampTool : ToolBase
{
    public override string Name => "Clone Stamp";
    public override string Icon => "\uE8C8";

    private Vector2? _sourcePoint;
    private Vector2 _sourceOffset;
    private bool _sourceSet;

    /// <summary>Whether the source point has been set (Alt+Click).</summary>
    public bool IsSourceSet => _sourceSet;

    /// <summary>The current source point for visual feedback.</summary>
    public Vector2 SourcePoint => _sourcePoint ?? Vector2.Zero;

    /// <summary>
    /// Sets the source sample point. Called when Alt+Click is detected.
    /// </summary>
    public void SetSource(Vector2 point)
    {
        _sourcePoint = point;
        _sourceSet = true;
    }

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!_sourceSet) return;
        base.OnPointerPressed(ds, point, color, strokeWidth);
        _sourceOffset = _sourcePoint!.Value - point;
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing || !_sourceSet) return;
        LastPoint = point;
    }

    /// <summary>
    /// Copies pixels from source to destination within the brush radius.
    /// </summary>
    public void ApplyClone(CanvasRenderTarget target, Vector2 from, Vector2 to, float radius)
    {
        if (!_sourceSet) return;

        var w = (int)target.SizeInPixels.Width;
        var h = (int)target.SizeInPixels.Height;
        var pixels = target.GetPixelColors();

        var dist = Vector2.Distance(from, to);
        var steps = Math.Max(1, (int)MathF.Ceiling(dist / Math.Max(1f, radius * 0.25f)));

        for (int i = 0; i <= steps; i++)
        {
            var t = (float)i / steps;
            var destPt = Vector2.Lerp(from, to, t);
            var srcPt = destPt + _sourceOffset;

            var r = (int)Math.Ceiling(radius);
            int minX = Math.Max(0, (int)destPt.X - r);
            int maxX = Math.Min(w - 1, (int)destPt.X + r);
            int minY = Math.Max(0, (int)destPt.Y - r);
            int maxY = Math.Min(h - 1, (int)destPt.Y + r);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var dx = x - destPt.X;
                    var dy = y - destPt.Y;
                    if (dx * dx + dy * dy > radius * radius) continue;

                    var sx = (int)(x + _sourceOffset.X);
                    var sy = (int)(y + _sourceOffset.Y);
                    if (sx < 0 || sx >= w || sy < 0 || sy >= h) continue;

                    pixels[y * w + x] = pixels[sy * w + sx];
                }
            }
        }

        target.SetPixelColors(pixels);
    }

    /// <summary>
    /// Draws the source crosshair indicator.
    /// </summary>
    public void DrawSourceIndicator(CanvasDrawingSession ds, Vector2 currentPoint, float radius)
    {
        if (!_sourceSet) return;
        var srcPt = currentPoint + _sourceOffset;
        ds.DrawCircle(srcPt, radius, Microsoft.UI.Colors.Cyan, 1f);
        ds.DrawLine(srcPt.X - radius, srcPt.Y, srcPt.X + radius, srcPt.Y, Microsoft.UI.Colors.Cyan, 0.5f);
        ds.DrawLine(srcPt.X, srcPt.Y - radius, srcPt.X, srcPt.Y + radius, Microsoft.UI.Colors.Cyan, 0.5f);
    }
}
