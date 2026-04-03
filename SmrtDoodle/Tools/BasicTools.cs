using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
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
    public override string Name => "Brush";
    public override string Icon => "\uE790";

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        ds.FillCircle(point, strokeWidth / 2f, color);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round };
        ds.DrawLine(LastPoint, point, color, strokeWidth, style);
        LastPoint = point;
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
