using Microsoft.Graphics.Canvas;
using System;
using Windows.UI;

namespace SmrtDoodle.Models;

/// <summary>
/// Type of non-destructive adjustment.
/// </summary>
public enum AdjustmentType
{
    BrightnessContrast,
    HueSaturationLightness,
    ColorBalance,
    Levels,
    Curves
}

/// <summary>
/// A non-destructive adjustment layer that modifies appearance during rendering
/// without altering the underlying pixel data.
/// </summary>
public class AdjustmentLayer : Layer
{
    public AdjustmentType AdjustmentType { get; set; }

    // Brightness/Contrast
    public float Brightness { get; set; }  // -100 to 100
    public float Contrast { get; set; }    // -100 to 100

    // Hue/Saturation/Lightness
    public float HueShift { get; set; }     // -180 to 180
    public float SaturationAdjust { get; set; }  // -100 to 100
    public float LightnessAdjust { get; set; }   // -100 to 100

    // Color Balance
    public float CyanRed { get; set; }       // -100 to 100
    public float MagentaGreen { get; set; }  // -100 to 100
    public float YellowBlue { get; set; }    // -100 to 100

    // Levels
    public float InputBlack { get; set; }    // 0-255
    public float InputWhite { get; set; } = 255f;
    public float InputGamma { get; set; } = 1.0f;
    public float OutputBlack { get; set; }   // 0-255
    public float OutputWhite { get; set; } = 255f;

    public AdjustmentLayer(string name, AdjustmentType type) : base(name)
    {
        AdjustmentType = type;
    }

    /// <summary>
    /// Applies this adjustment to a pixel color. Called during composition.
    /// </summary>
    public Color ApplyToPixel(Color input)
    {
        return AdjustmentType switch
        {
            AdjustmentType.BrightnessContrast => ApplyBrightnessContrast(input),
            AdjustmentType.HueSaturationLightness => ApplyHSL(input),
            AdjustmentType.ColorBalance => ApplyColorBalance(input),
            AdjustmentType.Levels => ApplyLevels(input),
            _ => input
        };
    }

    private Color ApplyBrightnessContrast(Color c)
    {
        float factor = (259f * (Contrast + 255f)) / (255f * (259f - Contrast));
        int r = Clamp((int)(factor * (c.R - 128) + 128 + Brightness));
        int g = Clamp((int)(factor * (c.G - 128) + 128 + Brightness));
        int b = Clamp((int)(factor * (c.B - 128) + 128 + Brightness));
        return Color.FromArgb(c.A, (byte)r, (byte)g, (byte)b);
    }

    private Color ApplyHSL(Color c)
    {
        RgbToHsl(c.R, c.G, c.B, out float h, out float s, out float l);
        h = (h + HueShift / 360f) % 1f;
        if (h < 0) h += 1f;
        s = Math.Clamp(s + SaturationAdjust / 100f, 0f, 1f);
        l = Math.Clamp(l + LightnessAdjust / 100f, 0f, 1f);
        HslToRgb(h, s, l, out byte r, out byte g, out byte b);
        return Color.FromArgb(c.A, r, g, b);
    }

    private Color ApplyColorBalance(Color c)
    {
        int r = Clamp(c.R + (int)(CyanRed * 2.55f));
        int g = Clamp(c.G + (int)(MagentaGreen * 2.55f));
        int b = Clamp(c.B + (int)(YellowBlue * 2.55f));
        return Color.FromArgb(c.A, (byte)r, (byte)g, (byte)b);
    }

    private Color ApplyLevels(Color c)
    {
        float r = ApplyLevelChannel(c.R);
        float g = ApplyLevelChannel(c.G);
        float b = ApplyLevelChannel(c.B);
        return Color.FromArgb(c.A, (byte)r, (byte)g, (byte)b);
    }

    private float ApplyLevelChannel(byte value)
    {
        float normalized = Math.Clamp((value - InputBlack) / Math.Max(1f, InputWhite - InputBlack), 0f, 1f);
        float gamma = MathF.Pow(normalized, 1f / Math.Max(0.01f, InputGamma));
        return Math.Clamp(OutputBlack + gamma * (OutputWhite - OutputBlack), 0f, 255f);
    }

    private static int Clamp(int value) => Math.Clamp(value, 0, 255);

    private static void RgbToHsl(byte r, byte g, byte b, out float h, out float s, out float l)
    {
        float rf = r / 255f, gf = g / 255f, bf = b / 255f;
        float max = Math.Max(rf, Math.Max(gf, bf));
        float min = Math.Min(rf, Math.Min(gf, bf));
        float delta = max - min;
        l = (max + min) / 2f;

        if (delta < 0.001f)
        {
            h = s = 0f;
            return;
        }

        s = l > 0.5f ? delta / (2f - max - min) : delta / (max + min);

        if (max == rf) h = ((gf - bf) / delta + (gf < bf ? 6f : 0f)) / 6f;
        else if (max == gf) h = ((bf - rf) / delta + 2f) / 6f;
        else h = ((rf - gf) / delta + 4f) / 6f;
    }

    private static void HslToRgb(float h, float s, float l, out byte r, out byte g, out byte b)
    {
        if (s < 0.001f)
        {
            r = g = b = (byte)(l * 255);
            return;
        }

        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;
        r = (byte)(HueToRgb(p, q, h + 1f / 3f) * 255);
        g = (byte)(HueToRgb(p, q, h) * 255);
        b = (byte)(HueToRgb(p, q, h - 1f / 3f) * 255);
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
}
