using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace SmrtDoodle.Tools;

/// <summary>
/// Free-form selection tool: draw a lasso shape to create a custom selection region.
/// </summary>
public class FreeFormSelectionTool : ToolBase
{
    public override string Name => "Free-Form Select";
    public override string Icon => "\uEF20";

    private readonly List<Vector2> _points = new();

    /// <summary>
    /// The bounding rectangle of the completed free-form selection.
    /// </summary>
    public Rect SelectionRect { get; private set; }

    /// <summary>
    /// The collected path points, available for rendering the lasso outline.
    /// </summary>
    public IReadOnlyList<Vector2> Points => _points;

    /// <summary>
    /// Whether a completed free-form selection exists.
    /// </summary>
    public bool HasSelection { get; private set; }

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        _points.Clear();
        _points.Add(point);
        HasSelection = false;
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        _points.Add(point);
    }

    public override void OnPointerReleased(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        _points.Add(point);
        if (_points.Count >= 3)
        {
            SelectionRect = ComputeBoundingRect();
            HasSelection = SelectionRect.Width >= 2 && SelectionRect.Height >= 2;
        }
        base.OnPointerReleased(ds, point, color, strokeWidth);
    }

    /// <summary>
    /// Creates a Win2D geometry from the freeform path for clipping or hit testing.
    /// </summary>
    public CanvasGeometry? CreateGeometry(ICanvasResourceCreator device)
    {
        if (_points.Count < 3) return null;

        using var builder = new CanvasPathBuilder(device);
        builder.BeginFigure(_points[0]);
        for (int i = 1; i < _points.Count; i++)
            builder.AddLine(_points[i]);
        builder.EndFigure(CanvasFigureLoop.Closed);
        return CanvasGeometry.CreatePath(builder);
    }

    /// <summary>
    /// Draws the lasso preview as a dashed line path.
    /// </summary>
    public void DrawLasso(CanvasDrawingSession ds, Color color, float zoomFactor)
    {
        if (_points.Count < 2) return;
        var style = new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash };
        for (int i = 1; i < _points.Count; i++)
            ds.DrawLine(_points[i - 1], _points[i], color, 1f / zoomFactor, style);

        // Close the path visually if we have a selection
        if (HasSelection || IsDrawing)
            ds.DrawLine(_points[^1], _points[0], color, 1f / zoomFactor, style);
    }

    private Rect ComputeBoundingRect()
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in _points)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    public override void Reset()
    {
        base.Reset();
        _points.Clear();
        HasSelection = false;
        SelectionRect = default;
    }
}
