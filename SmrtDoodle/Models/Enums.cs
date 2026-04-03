namespace SmrtDoodle.Models;

public enum DrawingTool
{
    Pencil,
    Brush,
    Eraser,
    Fill,
    Text,
    Eyedropper,
    Line,
    Curve,
    Shape,
    Selection,
    FreeFormSelection,
    Crop
}

public enum ShapeType
{
    Rectangle,
    Ellipse,
    RoundedRectangle,
    Triangle,
    RightTriangle,
    Diamond,
    Pentagon,
    Hexagon,
    Arrow,
    Star,
    Heart,
    Lightning
}

public enum SelectionMode
{
    None,
    Selecting,
    Moving
}

public enum BlendMode
{
    Normal,
    Multiply,
    Screen,
    Overlay
}

public enum ShapeFillMode
{
    Outline,
    Fill,
    OutlineAndFill
}
