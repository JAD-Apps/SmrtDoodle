using Microsoft.Graphics.Canvas;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tools;

public interface ITool
{
    string Name { get; }
    string Icon { get; }
    void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth);
    void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth);
    void OnPointerReleased(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth);
    void Reset();
}

public abstract class ToolBase : ITool
{
    public abstract string Name { get; }
    public abstract string Icon { get; }
    protected Vector2 LastPoint { get; set; }
    protected bool IsDrawing { get; set; }

    public virtual void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        LastPoint = point;
        IsDrawing = true;
    }

    public virtual void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth) { }

    public virtual void OnPointerReleased(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        IsDrawing = false;
    }

    public virtual void Reset()
    {
        IsDrawing = false;
    }
}
