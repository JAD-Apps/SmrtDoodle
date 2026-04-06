using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tools;

/// <summary>
/// Measurement tool — click-drag to measure distance and angle between two points.
/// Displays measurement info and optionally draws guide lines.
/// </summary>
public class MeasureTool : ToolBase
{
    public override string Name => "Measure";
    public override string Icon => "\uE9B6";

    private Vector2 _startPoint;
    private Vector2 _endPoint;

    /// <summary>The measured distance in pixels.</summary>
    public float Distance { get; private set; }

    /// <summary>The measured angle in degrees.</summary>
    public float Angle { get; private set; }

    /// <summary>Delta X in pixels.</summary>
    public float DeltaX { get; private set; }

    /// <summary>Delta Y in pixels.</summary>
    public float DeltaY { get; private set; }

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        _startPoint = point;
        _endPoint = point;
        UpdateMeasurement();
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        _endPoint = point;
        UpdateMeasurement();
    }

    private void UpdateMeasurement()
    {
        DeltaX = _endPoint.X - _startPoint.X;
        DeltaY = _endPoint.Y - _startPoint.Y;
        Distance = Vector2.Distance(_startPoint, _endPoint);
        Angle = MathF.Atan2(DeltaY, DeltaX) * (180f / MathF.PI);
    }

    /// <summary>
    /// Draws the measurement line and labels as a non-destructive overlay.
    /// </summary>
    public void DrawMeasurement(CanvasDrawingSession ds)
    {
        if (!IsDrawing && Distance < 1f) return;

        // Draw measurement line
        ds.DrawLine(_startPoint, _endPoint, Microsoft.UI.Colors.Cyan, 1.5f,
            new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash });

        // Draw endpoints
        ds.FillCircle(_startPoint, 3f, Microsoft.UI.Colors.Cyan);
        ds.FillCircle(_endPoint, 3f, Microsoft.UI.Colors.Cyan);

        // Draw right-angle indicator
        ds.DrawLine(_startPoint.X, _startPoint.Y, _endPoint.X, _startPoint.Y, Microsoft.UI.Colors.Gray, 0.5f,
            new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dot });
        ds.DrawLine(_endPoint.X, _startPoint.Y, _endPoint.X, _endPoint.Y, Microsoft.UI.Colors.Gray, 0.5f,
            new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dot });
    }

    /// <summary>
    /// Returns a formatted status string for the status bar.
    /// </summary>
    public string GetStatusText()
    {
        if (Distance < 1f) return "Measure: click and drag";
        return $"D: {Distance:F1}px  ΔX: {DeltaX:F1}  ΔY: {DeltaY:F1}  Angle: {Angle:F1}°";
    }
}
