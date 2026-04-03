using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tools;

/// <summary>
/// Bézier curve tool: first click sets start, second click sets end and draws a straight line,
/// subsequent clicks add control points to bend the curve. Press Enter or switch tools to finalize.
/// </summary>
public class CurveTool : ToolBase
{
    public override string Name => "Curve";
    public override string Icon => "\uE746";

    private readonly List<Vector2> _points = new();
    private Vector2 _currentPoint;
    private bool _hasFirstPoint;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        _points.Add(point);
        _currentPoint = point;
        _hasFirstPoint = true;
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        _currentPoint = point;
        // Update the last point as the user drags
        if (_points.Count > 0)
            _points[^1] = point;
    }

    public override void OnPointerReleased(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (_points.Count > 0)
            _points[^1] = point;
        _currentPoint = point;

        // After two points, commit the curve to the canvas
        if (_points.Count >= 2)
        {
            DrawCurve(ds, color, strokeWidth);
            _points.Clear();
            _hasFirstPoint = false;
        }

        base.OnPointerReleased(ds, point, color, strokeWidth);
    }

    /// <summary>
    /// Draws a preview of the curve being built.
    /// </summary>
    public void DrawPreview(CanvasDrawingSession ds, Color color, float strokeWidth)
    {
        if (_points.Count < 1) return;

        var style = new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash };

        if (_points.Count == 1)
        {
            // Preview line from first point to current mouse position
            ds.DrawLine(_points[0], _currentPoint, color, strokeWidth, style);
        }
        else
        {
            DrawCurveInternal(ds, color, strokeWidth, style);
        }
    }

    private void DrawCurve(CanvasDrawingSession ds, Color color, float strokeWidth)
    {
        if (_points.Count < 2) return;
        DrawCurveInternal(ds, color, strokeWidth, null);
    }

    private void DrawCurveInternal(CanvasDrawingSession ds, Color color, float strokeWidth, CanvasStrokeStyle? style)
    {
        using var builder = new CanvasPathBuilder(ds);
        builder.BeginFigure(_points[0]);

        if (_points.Count == 2)
        {
            // Simple quadratic Bézier with midpoint as implicit control
            var mid = (_points[0] + _points[1]) / 2f;
            builder.AddQuadraticBezier(mid, _points[1]);
        }
        else if (_points.Count == 3)
        {
            // Quadratic Bézier: start -> control -> end
            builder.AddQuadraticBezier(_points[1], _points[2]);
        }
        else
        {
            // Cubic Bézier: start -> control1 -> control2 -> end
            builder.AddCubicBezier(_points[1], _points[2], _points[^1]);
        }

        builder.EndFigure(CanvasFigureLoop.Open);
        using var geo = CanvasGeometry.CreatePath(builder);

        if (style != null)
            ds.DrawGeometry(geo, color, strokeWidth, style);
        else
            ds.DrawGeometry(geo, color, strokeWidth);
    }

    public override void Reset()
    {
        base.Reset();
        _points.Clear();
        _hasFirstPoint = false;
    }
}
