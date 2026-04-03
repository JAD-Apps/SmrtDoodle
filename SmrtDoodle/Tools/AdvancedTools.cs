using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using SmrtDoodle.Models;

namespace SmrtDoodle.Tools;

public class ShapeTool : ToolBase
{
    public override string Name => "Shape";
    public override string Icon => "\uF158";

    public ShapeType CurrentShapeType { get; set; } = ShapeType.Rectangle;
    public ShapeFillMode FillMode { get; set; } = ShapeFillMode.Outline;
    public Color SecondaryColor { get; set; } = Color.FromArgb(255, 255, 255, 255);
    public Vector2 StartPoint { get; private set; }
    public Vector2 EndPoint { get; set; }

    /// <summary>
    /// Backward-compatible property that maps to <see cref="FillMode"/>.
    /// </summary>
    public bool Filled
    {
        get => FillMode != ShapeFillMode.Outline;
        set => FillMode = value ? ShapeFillMode.Fill : ShapeFillMode.Outline;
    }

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
        DrawShape(ds, StartPoint, EndPoint, color, strokeWidth);
        base.OnPointerReleased(ds, point, color, strokeWidth);
    }

    public void DrawShape(CanvasDrawingSession ds, Vector2 start, Vector2 end, Color color, float strokeWidth)
    {
        var rect = NormalizeRect(start, end);
        switch (CurrentShapeType)
        {
            case ShapeType.Rectangle:
                DrawFilledOutline(ds, rect, color, strokeWidth,
                    (d, r, c) => d.FillRectangle(r, c),
                    (d, r, c, sw) => d.DrawRectangle(r, c, sw));
                break;
            case ShapeType.Ellipse:
                var cx = (float)(rect.X + rect.Width / 2);
                var cy = (float)(rect.Y + rect.Height / 2);
                var rx = (float)(rect.Width / 2);
                var ry = (float)(rect.Height / 2);
                DrawFilledOutline(ds, rect, color, strokeWidth,
                    (d, _, c) => d.FillEllipse(cx, cy, rx, ry, c),
                    (d, _, c, sw) => d.DrawEllipse(cx, cy, rx, ry, c, sw));
                break;
            case ShapeType.RoundedRectangle:
                DrawFilledOutline(ds, rect, color, strokeWidth,
                    (d, r, c) => d.FillRoundedRectangle(r, 10, 10, c),
                    (d, r, c, sw) => d.DrawRoundedRectangle(r, 10, 10, c, sw));
                break;
            case ShapeType.Triangle:
                DrawTriangle(ds, rect, color, strokeWidth);
                break;
            case ShapeType.RightTriangle:
                DrawRightTriangle(ds, rect, color, strokeWidth);
                break;
            case ShapeType.Diamond:
                DrawDiamond(ds, rect, color, strokeWidth);
                break;
            case ShapeType.Pentagon:
                DrawPolygon(ds, rect, 5, color, strokeWidth);
                break;
            case ShapeType.Hexagon:
                DrawPolygon(ds, rect, 6, color, strokeWidth);
                break;
            case ShapeType.Arrow:
                DrawArrow(ds, start, end, color, strokeWidth);
                break;
            case ShapeType.Star:
                DrawStar(ds, rect, color, strokeWidth);
                break;
            case ShapeType.Heart:
                DrawHeart(ds, rect, color, strokeWidth);
                break;
            case ShapeType.Lightning:
                DrawLightning(ds, rect, color, strokeWidth);
                break;
            default:
                DrawFilledOutline(ds, rect, color, strokeWidth,
                    (d, r, c) => d.FillRectangle(r, c),
                    (d, r, c, sw) => d.DrawRectangle(r, c, sw));
                break;
        }
    }

    /// <summary>
    /// Draws a shape with fill, outline, or both based on the current <see cref="FillMode"/>.
    /// </summary>
    private void DrawFilledOutline(CanvasDrawingSession ds, Rect rect, Color color, float sw,
        Action<CanvasDrawingSession, Rect, Color> fill,
        Action<CanvasDrawingSession, Rect, Color, float> outline)
    {
        if (FillMode is ShapeFillMode.Fill or ShapeFillMode.OutlineAndFill)
            fill(ds, rect, FillMode == ShapeFillMode.OutlineAndFill ? SecondaryColor : color);
        if (FillMode is ShapeFillMode.Outline or ShapeFillMode.OutlineAndFill)
            outline(ds, rect, color, sw);
    }

    private void DrawTriangle(CanvasDrawingSession ds, Rect rect, Color color, float sw)
    {
        var pts = new[]
        {
            new Vector2((float)(rect.X + rect.Width / 2), (float)rect.Y),
            new Vector2((float)(rect.X + rect.Width), (float)(rect.Y + rect.Height)),
            new Vector2((float)rect.X, (float)(rect.Y + rect.Height))
        };
        DrawPolygonPoints(ds, pts, color, sw);
    }

    private void DrawRightTriangle(CanvasDrawingSession ds, Rect rect, Color color, float sw)
    {
        var pts = new[]
        {
            new Vector2((float)rect.X, (float)rect.Y),
            new Vector2((float)(rect.X + rect.Width), (float)(rect.Y + rect.Height)),
            new Vector2((float)rect.X, (float)(rect.Y + rect.Height))
        };
        DrawPolygonPoints(ds, pts, color, sw);
    }

    private void DrawDiamond(CanvasDrawingSession ds, Rect rect, Color color, float sw)
    {
        var cx = (float)(rect.X + rect.Width / 2);
        var cy = (float)(rect.Y + rect.Height / 2);
        var pts = new[]
        {
            new Vector2(cx, (float)rect.Y),
            new Vector2((float)(rect.X + rect.Width), cy),
            new Vector2(cx, (float)(rect.Y + rect.Height)),
            new Vector2((float)rect.X, cy)
        };
        DrawPolygonPoints(ds, pts, color, sw);
    }

    private void DrawPolygon(CanvasDrawingSession ds, Rect rect, int sides, Color color, float sw)
    {
        var cx = (float)(rect.X + rect.Width / 2);
        var cy = (float)(rect.Y + rect.Height / 2);
        var rx = (float)(rect.Width / 2);
        var ry = (float)(rect.Height / 2);
        var pts = new Vector2[sides];
        for (int i = 0; i < sides; i++)
        {
            var angle = (float)(2 * Math.PI * i / sides - Math.PI / 2);
            pts[i] = new Vector2(cx + rx * MathF.Cos(angle), cy + ry * MathF.Sin(angle));
        }
        DrawPolygonPoints(ds, pts, color, sw);
    }

    private void DrawArrow(CanvasDrawingSession ds, Vector2 start, Vector2 end, Color color, float sw)
    {
        ds.DrawLine(start, end, color, sw);
        var angle = MathF.Atan2(end.Y - start.Y, end.X - start.X);
        var headLen = 15f;
        var a1 = angle + MathF.PI * 0.8f;
        var a2 = angle - MathF.PI * 0.8f;
        ds.DrawLine(end, new Vector2(end.X + headLen * MathF.Cos(a1), end.Y + headLen * MathF.Sin(a1)), color, sw);
        ds.DrawLine(end, new Vector2(end.X + headLen * MathF.Cos(a2), end.Y + headLen * MathF.Sin(a2)), color, sw);
    }

    private void DrawStar(CanvasDrawingSession ds, Rect rect, Color color, float sw)
    {
        var cx = (float)(rect.X + rect.Width / 2);
        var cy = (float)(rect.Y + rect.Height / 2);
        var rx = (float)(rect.Width / 2);
        var ry = (float)(rect.Height / 2);
        var pts = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            var angle = (float)(Math.PI * i / 5 - Math.PI / 2);
            var r = i % 2 == 0 ? 1f : 0.4f;
            pts[i] = new Vector2(cx + rx * r * MathF.Cos(angle), cy + ry * r * MathF.Sin(angle));
        }
        DrawPolygonPoints(ds, pts, color, sw);
    }

    private void DrawPolygonPoints(CanvasDrawingSession ds, Vector2[] pts, Color color, float sw)
    {
        using var builder = new CanvasPathBuilder(ds);
        builder.BeginFigure(pts[0]);
        for (int i = 1; i < pts.Length; i++)
            builder.AddLine(pts[i]);
        builder.EndFigure(CanvasFigureLoop.Closed);
        using var geo = CanvasGeometry.CreatePath(builder);
        if (FillMode is ShapeFillMode.Fill or ShapeFillMode.OutlineAndFill)
            ds.FillGeometry(geo, FillMode == ShapeFillMode.OutlineAndFill ? SecondaryColor : color);
        if (FillMode is ShapeFillMode.Outline or ShapeFillMode.OutlineAndFill)
            ds.DrawGeometry(geo, color, sw);
    }

    private void DrawHeart(CanvasDrawingSession ds, Rect rect, Color color, float sw)
    {
        var cx = (float)(rect.X + rect.Width / 2);
        var top = (float)rect.Y;
        var bottom = (float)(rect.Y + rect.Height);
        var left = (float)rect.X;
        var right = (float)(rect.X + rect.Width);
        var w4 = (float)(rect.Width / 4);
        var h3 = (float)(rect.Height / 3);

        using var builder = new CanvasPathBuilder(ds);
        builder.BeginFigure(new Vector2(cx, bottom));
        // Left side curve
        builder.AddCubicBezier(
            new Vector2(left - w4, top + h3),
            new Vector2(left, top - h3 * 0.5f),
            new Vector2(cx, top + h3));
        // Right side curve
        builder.AddCubicBezier(
            new Vector2(right, top - h3 * 0.5f),
            new Vector2(right + w4, top + h3),
            new Vector2(cx, bottom));
        builder.EndFigure(CanvasFigureLoop.Closed);
        using var geo = CanvasGeometry.CreatePath(builder);
        if (FillMode is ShapeFillMode.Fill or ShapeFillMode.OutlineAndFill)
            ds.FillGeometry(geo, FillMode == ShapeFillMode.OutlineAndFill ? SecondaryColor : color);
        if (FillMode is ShapeFillMode.Outline or ShapeFillMode.OutlineAndFill)
            ds.DrawGeometry(geo, color, sw);
    }

    private void DrawLightning(CanvasDrawingSession ds, Rect rect, Color color, float sw)
    {
        var x = (float)rect.X;
        var y = (float)rect.Y;
        var w = (float)rect.Width;
        var h = (float)rect.Height;
        var pts = new[]
        {
            new Vector2(x + w * 0.35f, y),
            new Vector2(x + w * 0.75f, y),
            new Vector2(x + w * 0.45f, y + h * 0.4f),
            new Vector2(x + w * 0.7f, y + h * 0.4f),
            new Vector2(x + w * 0.25f, y + h),
            new Vector2(x + w * 0.4f, y + h * 0.55f),
            new Vector2(x + w * 0.15f, y + h * 0.55f)
        };
        DrawPolygonPoints(ds, pts, color, sw);
    }

    public static Rect NormalizeRect(Vector2 a, Vector2 b)
    {
        var x = Math.Min(a.X, b.X);
        var y = Math.Min(a.Y, b.Y);
        var w = Math.Abs(a.X - b.X);
        var h = Math.Abs(a.Y - b.Y);
        return new Rect(x, y, w, h);
    }
}

public class SelectionTool : ToolBase
{
    public override string Name => "Select";
    public override string Icon => "\uE8B3";

    public Rect SelectionRect { get; set; }
    public Vector2 StartPoint { get; private set; }
    public SelectionMode Mode { get; set; } = SelectionMode.None;
    public CanvasRenderTarget? SelectionBitmap { get; set; }

    /// <summary>
    /// When true, the secondary color in the selection is treated as transparent during move/paste.
    /// </summary>
    public bool TransparentSelection { get; set; }

    /// <summary>
    /// The background color to treat as transparent when <see cref="TransparentSelection"/> is enabled.
    /// </summary>
    public Color TransparentColor { get; set; } = Color.FromArgb(255, 255, 255, 255);

    private Vector2 _moveOffset;
    private bool _hasLiftedPixels;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);

        if (Mode == SelectionMode.None || !SelectionRect.Contains(new Point(point.X, point.Y)))
        {
            // Commit any previously floating selection before starting a new one
            CommitFloatingSelection(ds);
            StartPoint = point;
            Mode = SelectionMode.Selecting;
        }
        else
        {
            // Pixel lifting is done by the MainWindow when it detects the first move
            Mode = SelectionMode.Moving;
            _moveOffset = new Vector2(point.X - (float)SelectionRect.X, point.Y - (float)SelectionRect.Y);
        }
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        if (Mode == SelectionMode.Selecting)
        {
            SelectionRect = ShapeTool.NormalizeRect(StartPoint, point);
        }
        else if (Mode == SelectionMode.Moving)
        {
            SelectionRect = new Rect(point.X - _moveOffset.X, point.Y - _moveOffset.Y,
                SelectionRect.Width, SelectionRect.Height);
        }
    }

    public override void OnPointerReleased(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (Mode == SelectionMode.Selecting)
        {
            SelectionRect = ShapeTool.NormalizeRect(StartPoint, point);
            if (SelectionRect.Width < 2 || SelectionRect.Height < 2)
                Mode = SelectionMode.None;
        }
        base.OnPointerReleased(ds, point, color, strokeWidth);
    }

    /// <summary>
    /// Lifts the pixels under the selection rectangle from the given bitmap, storing them
    /// in <see cref="SelectionBitmap"/> and clearing the source area.
    /// </summary>
    public void LiftPixels(CanvasRenderTarget sourceBitmap, float dpi)
    {
        if (SelectionBitmap != null || SelectionRect.Width < 1 || SelectionRect.Height < 1) return;

        var device = sourceBitmap.Device;
        SelectionBitmap = new CanvasRenderTarget(device, (float)SelectionRect.Width, (float)SelectionRect.Height, dpi);
        using (var sds = SelectionBitmap.CreateDrawingSession())
        {
            sds.DrawImage(sourceBitmap,
                new Rect(0, 0, SelectionRect.Width, SelectionRect.Height),
                SelectionRect);
        }

        // Apply transparent selection: replace the background color with transparent
        if (TransparentSelection)
        {
            ApplyTransparency(SelectionBitmap, TransparentColor);
        }

        // Clear source area
        using (var ds = sourceBitmap.CreateDrawingSession())
        {
            ds.Blend = CanvasBlend.Copy;
            ds.FillRectangle(SelectionRect, Color.FromArgb(0, 0, 0, 0));
            ds.Blend = CanvasBlend.SourceOver;
        }

        _hasLiftedPixels = true;
    }

    private static void ApplyTransparency(CanvasRenderTarget bitmap, Color bgColor, int tolerance = 30)
    {
        var pixels = bitmap.GetPixelColors();
        for (int i = 0; i < pixels.Length; i++)
        {
            var c = pixels[i];
            if (Math.Abs(c.R - bgColor.R) <= tolerance &&
                Math.Abs(c.G - bgColor.G) <= tolerance &&
                Math.Abs(c.B - bgColor.B) <= tolerance &&
                c.A > 0)
            {
                pixels[i] = Color.FromArgb(0, 0, 0, 0);
            }
        }
        bitmap.SetPixelColors(pixels);
    }

    /// <summary>
    /// Stamps the floating selection bitmap onto the canvas at the current position.
    /// </summary>
    public void CommitFloatingSelection(CanvasDrawingSession ds)
    {
        if (SelectionBitmap != null && _hasLiftedPixels)
        {
            ds.DrawImage(SelectionBitmap, (float)SelectionRect.X, (float)SelectionRect.Y);
            SelectionBitmap.Dispose();
            SelectionBitmap = null;
            _hasLiftedPixels = false;
        }
    }

    /// <summary>
    /// Checks whether this tool currently has floating pixel data.
    /// </summary>
    public bool HasFloatingSelection => _hasLiftedPixels && SelectionBitmap != null;

    public override void Reset()
    {
        base.Reset();
        SelectionBitmap?.Dispose();
        SelectionBitmap = null;
        _hasLiftedPixels = false;
        Mode = SelectionMode.None;
        SelectionRect = default;
    }
}

public class FillTool : ToolBase
{
    public override string Name => "Fill";
    public override string Icon => "\uE771";

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
    }
}

public class TextTool : ToolBase
{
    public override string Name => "Text";
    public override string Icon => "\uE8D2";

    public Vector2 InsertionPoint { get; private set; }

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        InsertionPoint = point;
    }
}
