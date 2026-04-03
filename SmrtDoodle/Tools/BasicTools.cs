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
        ds.DrawCircle(point, strokeWidth / 4f, color);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        ds.DrawLine(LastPoint, point, color, strokeWidth / 2f);
        LastPoint = point;
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
        switch (CurrentStyle)
        {
            case BrushStyle.Normal:
            {
                var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round };
                ds.DrawLine(from, to, color, strokeWidth, style);
                break;
            }
            case BrushStyle.Calligraphy:
            {
                // Angled flat brush
                var offset = new Vector2(strokeWidth * 0.35f, -strokeWidth * 0.35f);
                ds.DrawLine(from + offset, to + offset, color, strokeWidth * 0.3f);
                ds.DrawLine(from - offset, to - offset, color, strokeWidth * 0.3f);
                ds.DrawLine(from, to, color, strokeWidth * 0.5f);
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
                // Thick opaque stroke with slight texture
                var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Flat, EndCap = CanvasCapStyle.Flat };
                ds.DrawLine(from, to, color, strokeWidth * 1.5f, style);
                var highlight = Color.FromArgb(40, 255, 255, 255);
                ds.DrawLine(from + new Vector2(0, -strokeWidth * 0.3f), to + new Vector2(0, -strokeWidth * 0.3f), highlight, strokeWidth * 0.4f);
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
                // Semi-transparent wide stroke
                var alpha = (byte)Math.Min(128, (int)color.A);
                var markerColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Square, EndCap = CanvasCapStyle.Square };
                ds.DrawLine(from, to, markerColor, strokeWidth * 1.8f, style);
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
                // Very transparent wide soft dabs
                var alpha = (byte)Math.Min(40, (int)color.A);
                var wcColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round };
                ds.DrawLine(from, to, wcColor, strokeWidth * 2.5f, style);
                // Second lighter pass for diffusion
                var wcColor2 = Color.FromArgb((byte)(alpha / 2), color.R, color.G, color.B);
                ds.DrawLine(from, to, wcColor2, strokeWidth * 3.5f, style);
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
