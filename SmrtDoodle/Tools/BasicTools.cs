using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using SmrtDoodle.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tools;

public class PencilTool : ToolBase
{
    public override string Name => "Pencil";
    public override string Icon => "\uED63";

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        DrawFilledStroke(ds, point, point, color, Math.Max(1f, strokeWidth / 2f));
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        DrawFilledStroke(ds, LastPoint, point, color, Math.Max(1f, strokeWidth / 2f));
        LastPoint = point;
    }

    private static void DrawFilledStroke(CanvasDrawingSession ds, Vector2 from, Vector2 to, Color color, float radius)
    {
        var distance = Vector2.Distance(from, to);
        var steps = Math.Max(1, (int)MathF.Ceiling(distance / Math.Max(1f, radius * 0.5f)));

        for (int i = 0; i <= steps; i++)
        {
            var t = steps == 0 ? 0f : (float)i / steps;
            var point = Vector2.Lerp(from, to, t);
            ds.FillCircle(point, radius, color);
        }
    }
}

public class BrushTool : ToolBase
{
    private readonly Random _rng = new();

    public override string Name => "Brush";
    public override string Icon => "\uE790";

    public BrushStyle CurrentStyle { get; set; } = BrushStyle.Normal;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        DrawBrushDab(ds, point, point, color, strokeWidth);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        DrawBrushDab(ds, LastPoint, point, color, strokeWidth);
        LastPoint = point;
    }

    private void DrawBrushDab(CanvasDrawingSession ds, Vector2 from, Vector2 to, Color color, float strokeWidth)
    {
        static void StampFilledCircles(CanvasDrawingSession canvas, Vector2 start, Vector2 end, Color stampColor, float radius, float spacingFactor = 0.5f)
        {
            var r = Math.Max(1f, radius);
            var distance = Vector2.Distance(start, end);
            var spacing = Math.Max(1f, r * spacingFactor);
            var steps = Math.Max(1, (int)MathF.Ceiling(distance / spacing));

            for (int i = 0; i <= steps; i++)
            {
                var t = steps == 0 ? 0f : (float)i / steps;
                var p = Vector2.Lerp(start, end, t);
                canvas.FillCircle(p, r, stampColor);
            }
        }

        switch (CurrentStyle)
        {
            case BrushStyle.Normal:
            {
                StampFilledCircles(ds, from, to, color, strokeWidth * 0.5f);
                break;
            }
            case BrushStyle.Calligraphy:
            {
                var dist = Vector2.Distance(from, to);
                var steps = Math.Max(1, (int)MathF.Ceiling(dist / Math.Max(1f, strokeWidth * 0.35f)));
                var major = Math.Max(1f, strokeWidth * 0.7f);
                var minor = Math.Max(1f, strokeWidth * 0.35f);

                for (int i = 0; i <= steps; i++)
                {
                    var t = steps == 0 ? 0f : (float)i / steps;
                    var center = Vector2.Lerp(from, to, t);
                    ds.FillEllipse(center.X, center.Y, major, minor, color);
                }
                break;
            }
            case BrushStyle.Airbrush:
            {
                // Scatter spray of dots
                var dist = Vector2.Distance(from, to);
                var count = Math.Max(8, (int)(dist * strokeWidth * 0.3f));
                for (int i = 0; i < count; i++)
                {
                    var t = (float)_rng.NextDouble();
                    var center = Vector2.Lerp(from, to, t);
                    var angle = (float)(_rng.NextDouble() * Math.PI * 2);
                    var radius = (float)(_rng.NextDouble() * strokeWidth);
                    var px = center.X + MathF.Cos(angle) * radius;
                    var py = center.Y + MathF.Sin(angle) * radius;
                    var alpha = (byte)Math.Max(20, 255 - (int)(radius / strokeWidth * 200));
                    var sprayColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                    ds.FillCircle(px, py, 1f, sprayColor);
                }
                break;
            }
            case BrushStyle.Oil:
            {
                StampFilledCircles(ds, from, to, color, strokeWidth * 0.75f, 0.4f);
                var highlight = Color.FromArgb(40, 255, 255, 255);
                StampFilledCircles(ds, from + new Vector2(0, -strokeWidth * 0.2f), to + new Vector2(0, -strokeWidth * 0.2f), highlight, strokeWidth * 0.2f, 0.6f);
                break;
            }
            case BrushStyle.Crayon:
            {
                // Rough edges via scattered short segments
                var dist = Vector2.Distance(from, to);
                var steps = Math.Max(3, (int)(dist / 2));
                for (int i = 0; i < steps; i++)
                {
                    var t = (float)i / steps;
                    var pt = Vector2.Lerp(from, to, t);
                    var jx = (float)(_rng.NextDouble() - 0.5) * strokeWidth * 0.5f;
                    var jy = (float)(_rng.NextDouble() - 0.5) * strokeWidth * 0.5f;
                    var alpha = (byte)(_rng.Next(160, 255));
                    var c = Color.FromArgb(alpha, color.R, color.G, color.B);
                    ds.FillCircle(pt.X + jx, pt.Y + jy, strokeWidth * 0.4f, c);
                }
                break;
            }
            case BrushStyle.Marker:
            {
                var alpha = (byte)Math.Min(128, (int)color.A);
                var markerColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                StampFilledCircles(ds, from, to, markerColor, strokeWidth * 0.9f, 0.35f);
                break;
            }
            case BrushStyle.NaturalPencil:
            {
                // Fine textured line
                var dist = Vector2.Distance(from, to);
                var steps = Math.Max(2, (int)(dist));
                for (int i = 0; i < steps; i++)
                {
                    var t = (float)i / steps;
                    var pt = Vector2.Lerp(from, to, t);
                    var jx = (float)(_rng.NextDouble() - 0.5) * strokeWidth * 0.15f;
                    var jy = (float)(_rng.NextDouble() - 0.5) * strokeWidth * 0.15f;
                    var alpha = (byte)(_rng.Next(100, 220));
                    var c = Color.FromArgb(alpha, color.R, color.G, color.B);
                    ds.FillCircle(pt.X + jx, pt.Y + jy, strokeWidth * 0.2f, c);
                }
                break;
            }
            case BrushStyle.Watercolor:
            {
                var alpha = (byte)Math.Min(40, (int)color.A);
                var wcColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                StampFilledCircles(ds, from, to, wcColor, strokeWidth * 1.25f, 0.45f);
                var wcColor2 = Color.FromArgb((byte)(alpha / 2), color.R, color.G, color.B);
                StampFilledCircles(ds, from, to, wcColor2, strokeWidth * 1.75f, 0.6f);
                break;
            }
        }
    }
}

public class EraserTool : ToolBase
{
    public override string Name => "Eraser";
    public override string Icon => "\uE75C";

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        ds.Blend = CanvasBlend.Copy;
        ds.FillCircle(point, strokeWidth, Microsoft.UI.Colors.Transparent);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        ds.Blend = CanvasBlend.Copy;
        var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round };
        ds.DrawLine(LastPoint, point, Microsoft.UI.Colors.Transparent, strokeWidth * 2, style);
        ds.FillCircle(point, strokeWidth, Microsoft.UI.Colors.Transparent);
        LastPoint = point;
    }

    public override void OnPointerReleased(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        ds.Blend = CanvasBlend.SourceOver;
        base.OnPointerReleased(ds, point, color, strokeWidth);
    }
}

public class LineTool : ToolBase
{
    public Vector2 StartPoint { get; private set; }
    public Vector2 EndPoint { get; set; }

    public override string Name => "Line";
    public override string Icon => "\uE738";

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        StartPoint = point;
        EndPoint = point;
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        EndPoint = point;
    }

    public override void OnPointerReleased(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        EndPoint = point;
        ds.DrawLine(StartPoint, EndPoint, color, strokeWidth,
            new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round });
        base.OnPointerReleased(ds, point, color, strokeWidth);
    }
}

public class EyedropperTool : ToolBase
{
    public Color? PickedColor { get; set; }

    public override string Name => "Color Picker";
    public override string Icon => "\uEF3C";

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
    }
}

/// <summary>
/// Magnifier tool — zoom is handled by the main window; this is a no-op placeholder.
/// </summary>
public class MagnifierTool : ToolBase
{
    public override string Name => "Magnifier";
    public override string Icon => "\uE71E";
}
