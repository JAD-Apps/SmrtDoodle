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
    Crop,
    Magnifier,
    Gradient,
    Blur,
    Sharpen,
    Smudge,
    CloneStamp,
    PatternFill,
    Measure
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
    // Normal group
    Normal,
    Dissolve,

    // Darken group
    Darken,
    Multiply,
    ColorBurn,
    LinearBurn,
    DarkerColor,

    // Lighten group
    Lighten,
    Screen,
    ColorDodge,
    LinearDodge,
    LighterColor,

    // Contrast group
    Overlay,
    SoftLight,
    HardLight,
    VividLight,
    LinearLight,
    PinLight,
    HardMix,

    // Inversion group
    Difference,
    Exclusion,

    // Component group
    Hue,
    Saturation,
    Color,
    Luminosity
}

public enum ShapeFillMode
{
    Outline,
    Fill,
    OutlineAndFill
}

public enum BrushStyle
{
    Normal,
    Calligraphy,
    Airbrush,
    Oil,
    Crayon,
    Marker,
    NaturalPencil,
    Watercolor
}
