using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using System;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tools;

public enum GradientType
{
    Linear,
    Radial,
    Angle,
    Reflected,
    Diamond
}

/// <summary>
/// Gradient tool — click-drag to define direction, renders gradient on release.
/// Supports Linear, Radial, Angle, Reflected, and Diamond gradient types.
/// </summary>
public class GradientTool : ToolBase
{
    public override string Name => "Gradient";
    public override string Icon => "\uF0E2";

    public GradientType GradientMode { get; set; } = GradientType.Linear;
    public Color SecondaryColor { get; set; } = Color.FromArgb(255, 255, 255, 255);

    private Vector2 _startPoint;
    private Vector2 _endPoint;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        _startPoint = point;
        _endPoint = point;
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        _endPoint = point;
        // Preview is handled externally via DrawPreview
    }

    /// <summary>
    /// Draws the gradient preview line during drag.
    /// </summary>
    public void DrawPreview(CanvasDrawingSession ds)
    {
        if (!IsDrawing) return;
        ds.DrawLine(_startPoint, _endPoint, Microsoft.UI.Colors.Gray, 1f,
            new Microsoft.Graphics.Canvas.Geometry.CanvasStrokeStyle { DashStyle = Microsoft.Graphics.Canvas.Geometry.CanvasDashStyle.Dash });
    }

    /// <summary>
    /// Commits the gradient to the layer bitmap on pointer release.
    /// </summary>
    public void CommitGradient(CanvasDrawingSession ds, Color primaryColor, Color secondaryColor,
        float canvasWidth, float canvasHeight)
    {
        var distance = Vector2.Distance(_startPoint, _endPoint);
        if (distance < 2f) return;

        switch (GradientMode)
        {
            case GradientType.Linear:
                DrawLinearGradient(ds, primaryColor, secondaryColor, canvasWidth, canvasHeight);
                break;
            case GradientType.Radial:
                DrawRadialGradient(ds, primaryColor, secondaryColor);
                break;
            case GradientType.Angle:
                DrawAngleGradient(ds, primaryColor, secondaryColor, canvasWidth, canvasHeight);
                break;
            case GradientType.Reflected:
                DrawReflectedGradient(ds, primaryColor, secondaryColor, canvasWidth, canvasHeight);
                break;
            case GradientType.Diamond:
                DrawDiamondGradient(ds, primaryColor, secondaryColor, canvasWidth, canvasHeight);
                break;
        }

        IsDrawing = false;
    }

    private void DrawLinearGradient(CanvasDrawingSession ds, Color c1, Color c2,
        float canvasWidth, float canvasHeight)
    {
        using var brush = new CanvasLinearGradientBrush(ds, c1, c2)
        {
            StartPoint = _startPoint,
            EndPoint = _endPoint
        };
        ds.FillRectangle(0, 0, canvasWidth, canvasHeight, brush);
    }

    private void DrawRadialGradient(CanvasDrawingSession ds, Color c1, Color c2)
    {
        var radius = Vector2.Distance(_startPoint, _endPoint);
        using var brush = new CanvasRadialGradientBrush(ds, c1, c2)
        {
            Center = _startPoint,
            RadiusX = radius,
            RadiusY = radius
        };
        // Fill a large rect to cover the canvas
        ds.FillRectangle(_startPoint.X - radius * 2, _startPoint.Y - radius * 2,
            radius * 4, radius * 4, brush);
    }

    private void DrawAngleGradient(CanvasDrawingSession ds, Color c1, Color c2,
        float canvasWidth, float canvasHeight)
    {
        // Pixel-based angle gradient
        int w = (int)canvasWidth, h = (int)canvasHeight;
        var baseAngle = MathF.Atan2(_endPoint.Y - _startPoint.Y, _endPoint.X - _startPoint.X);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var angle = MathF.Atan2(y - _startPoint.Y, x - _startPoint.X) - baseAngle;
                if (angle < 0) angle += MathF.PI * 2f;
                var t = angle / (MathF.PI * 2f);
                var color = LerpColor(c1, c2, t);
                ds.FillRectangle(x, y, 1, 1, color);
            }
        }
    }

    private void DrawReflectedGradient(CanvasDrawingSession ds, Color c1, Color c2,
        float canvasWidth, float canvasHeight)
    {
        // Reflected: linear gradient that mirrors at the center
        var dir = _endPoint - _startPoint;
        var length = dir.Length();
        if (length < 1f) return;
        var norm = dir / length;

        int w = (int)canvasWidth, h = (int)canvasHeight;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var delta = new Vector2(x, y) - _startPoint;
                var projection = Vector2.Dot(delta, norm);
                var t = Math.Abs(projection) / length;
                t = Math.Min(t, 1f);
                var color = LerpColor(c1, c2, t);
                ds.FillRectangle(x, y, 1, 1, color);
            }
        }
    }

    private void DrawDiamondGradient(CanvasDrawingSession ds, Color c1, Color c2,
        float canvasWidth, float canvasHeight)
    {
        var dist = Vector2.Distance(_startPoint, _endPoint);
        if (dist < 1f) return;

        int w = (int)canvasWidth, h = (int)canvasHeight;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var dx = Math.Abs(x - _startPoint.X);
                var dy = Math.Abs(y - _startPoint.Y);
                var t = Math.Min((dx + dy) / dist, 1f);
                var color = LerpColor(c1, c2, t);
                ds.FillRectangle(x, y, 1, 1, color);
            }
        }
    }

    private static Color LerpColor(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (byte)(a.A + (b.A - a.A) * t),
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t));
    }
}
