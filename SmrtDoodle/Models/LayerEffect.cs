using Windows.UI;

namespace SmrtDoodle.Models;

/// <summary>
/// Types of non-destructive layer effects.
/// </summary>
public enum LayerEffectType
{
    DropShadow,
    InnerShadow,
    OuterGlow,
    Stroke
}

/// <summary>
/// Position for stroke effects.
/// </summary>
public enum StrokePosition
{
    Inside,
    Center,
    Outside
}

/// <summary>
/// A non-destructive visual effect applied to a layer during composition.
/// </summary>
public class LayerEffect
{
    public LayerEffectType Type { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Common properties
    public Color EffectColor { get; set; } = Color.FromArgb(128, 0, 0, 0);
    public float Opacity { get; set; } = 0.75f;

    // Shadow/Glow properties
    public float OffsetX { get; set; }
    public float OffsetY { get; set; } = 5f;
    public float BlurRadius { get; set; } = 10f;
    public float Spread { get; set; }

    // Stroke properties
    public float StrokeWidth { get; set; } = 2f;
    public StrokePosition StrokePosition { get; set; } = StrokePosition.Outside;

    public LayerEffect Clone()
    {
        return new LayerEffect
        {
            Type = Type,
            IsEnabled = IsEnabled,
            EffectColor = EffectColor,
            Opacity = Opacity,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            BlurRadius = BlurRadius,
            Spread = Spread,
            StrokeWidth = StrokeWidth,
            StrokePosition = StrokePosition
        };
    }

    public override string ToString() => Type switch
    {
        LayerEffectType.DropShadow => $"Drop Shadow ({BlurRadius}px)",
        LayerEffectType.InnerShadow => $"Inner Shadow ({BlurRadius}px)",
        LayerEffectType.OuterGlow => $"Outer Glow ({Spread}px)",
        LayerEffectType.Stroke => $"Stroke ({StrokeWidth}px {StrokePosition})",
        _ => Type.ToString()
    };
}
