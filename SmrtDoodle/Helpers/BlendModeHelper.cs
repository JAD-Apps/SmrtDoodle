using Microsoft.Graphics.Canvas;
using SmrtDoodle.Models;
using System;
using Windows.UI;

namespace SmrtDoodle.Helpers;

/// <summary>
/// Provides pixel-level blend mode operations for compositing layers.
/// Each blend mode function takes base (bottom) and blend (top) color components
/// normalized to 0-1 range and returns the blended result.
/// </summary>
public static class BlendModeHelper
{
    /// <summary>
    /// Composites a source layer onto a destination render target using the specified blend mode and opacity.
    /// </summary>
    public static void ComposeLayer(CanvasRenderTarget destination, CanvasRenderTarget source,
        BlendMode blendMode, float opacity, int width, int height)
    {
        if (blendMode == BlendMode.Normal && Math.Abs(opacity - 1.0f) < 0.001f)
        {
            // Fast path: simple draw for Normal at full opacity
            using var ds = destination.CreateDrawingSession();
            ds.DrawImage(source);
            return;
        }

        if (blendMode == BlendMode.Normal)
        {
            // Normal blend with opacity — Win2D handles this natively
            using var ds = destination.CreateDrawingSession();
            ds.DrawImage(source, 0, 0,
                new Windows.Foundation.Rect(0, 0, width, height), opacity);
            return;
        }

        // Pixel-level blending for non-Normal modes
        var destPixels = destination.GetPixelColors();
        var srcPixels = source.GetPixelColors();
        var pixelCount = Math.Min(destPixels.Length, srcPixels.Length);

        for (int i = 0; i < pixelCount; i++)
        {
            var src = srcPixels[i];
            if (src.A == 0) continue; // Skip fully transparent source pixels

            var dst = destPixels[i];
            var srcAlpha = (src.A / 255f) * opacity;

            if (srcAlpha < 0.001f) continue;

            // Normalize to 0-1 range
            float sr = src.R / 255f, sg = src.G / 255f, sb = src.B / 255f;
            float dr = dst.R / 255f, dg = dst.G / 255f, db = dst.B / 255f;

            // Apply blend mode function
            float br = dr, bg = dg, bb = db;
            switch (blendMode)
            {
                case BlendMode.Multiply:
                    br = dr * sr; bg = dg * sg; bb = db * sb;
                    break;
                case BlendMode.Screen:
                    br = 1f - (1f - dr) * (1f - sr);
                    bg = 1f - (1f - dg) * (1f - sg);
                    bb = 1f - (1f - db) * (1f - sb);
                    break;
                case BlendMode.Overlay:
                    br = dr < 0.5f ? 2f * dr * sr : 1f - 2f * (1f - dr) * (1f - sr);
                    bg = dg < 0.5f ? 2f * dg * sg : 1f - 2f * (1f - dg) * (1f - sg);
                    bb = db < 0.5f ? 2f * db * sb : 1f - 2f * (1f - db) * (1f - sb);
                    break;
                case BlendMode.Darken:
                    br = Math.Min(dr, sr); bg = Math.Min(dg, sg); bb = Math.Min(db, sb);
                    break;
                case BlendMode.Lighten:
                    br = Math.Max(dr, sr); bg = Math.Max(dg, sg); bb = Math.Max(db, sb);
                    break;
                case BlendMode.ColorBurn:
                    br = sr > 0 ? 1f - Math.Min(1f, (1f - dr) / sr) : 0f;
                    bg = sg > 0 ? 1f - Math.Min(1f, (1f - dg) / sg) : 0f;
                    bb = sb > 0 ? 1f - Math.Min(1f, (1f - db) / sb) : 0f;
                    break;
                case BlendMode.ColorDodge:
                    br = sr < 1f ? Math.Min(1f, dr / (1f - sr)) : 1f;
                    bg = sg < 1f ? Math.Min(1f, dg / (1f - sg)) : 1f;
                    bb = sb < 1f ? Math.Min(1f, db / (1f - sb)) : 1f;
                    break;
                case BlendMode.LinearBurn:
                    br = Math.Max(0f, dr + sr - 1f);
                    bg = Math.Max(0f, dg + sg - 1f);
                    bb = Math.Max(0f, db + sb - 1f);
                    break;
                case BlendMode.LinearDodge:
                    br = Math.Min(1f, dr + sr);
                    bg = Math.Min(1f, dg + sg);
                    bb = Math.Min(1f, db + sb);
                    break;
                case BlendMode.SoftLight:
                    br = SoftLightChannel(dr, sr);
                    bg = SoftLightChannel(dg, sg);
                    bb = SoftLightChannel(db, sb);
                    break;
                case BlendMode.HardLight:
                    br = sr < 0.5f ? 2f * dr * sr : 1f - 2f * (1f - dr) * (1f - sr);
                    bg = sg < 0.5f ? 2f * dg * sg : 1f - 2f * (1f - dg) * (1f - sg);
                    bb = sb < 0.5f ? 2f * db * sb : 1f - 2f * (1f - db) * (1f - sb);
                    break;
                case BlendMode.VividLight:
                    br = sr < 0.5f ? (sr > 0 ? 1f - Math.Min(1f, (1f - dr) / (2f * sr)) : 0f)
                                    : (sr < 1f ? Math.Min(1f, dr / (2f * (1f - sr))) : 1f);
                    bg = sg < 0.5f ? (sg > 0 ? 1f - Math.Min(1f, (1f - dg) / (2f * sg)) : 0f)
                                    : (sg < 1f ? Math.Min(1f, dg / (2f * (1f - sg))) : 1f);
                    bb = sb < 0.5f ? (sb > 0 ? 1f - Math.Min(1f, (1f - db) / (2f * sb)) : 0f)
                                    : (sb < 1f ? Math.Min(1f, db / (2f * (1f - sb))) : 1f);
                    break;
                case BlendMode.LinearLight:
                    br = Math.Clamp(dr + 2f * sr - 1f, 0f, 1f);
                    bg = Math.Clamp(dg + 2f * sg - 1f, 0f, 1f);
                    bb = Math.Clamp(db + 2f * sb - 1f, 0f, 1f);
                    break;
                case BlendMode.PinLight:
                    br = sr < 0.5f ? Math.Min(dr, 2f * sr) : Math.Max(dr, 2f * sr - 1f);
                    bg = sg < 0.5f ? Math.Min(dg, 2f * sg) : Math.Max(dg, 2f * sg - 1f);
                    bb = sb < 0.5f ? Math.Min(db, 2f * sb) : Math.Max(db, 2f * sb - 1f);
                    break;
                case BlendMode.HardMix:
                    br = (dr + sr >= 1f) ? 1f : 0f;
                    bg = (dg + sg >= 1f) ? 1f : 0f;
                    bb = (db + sb >= 1f) ? 1f : 0f;
                    break;
                case BlendMode.Difference:
                    br = Math.Abs(dr - sr); bg = Math.Abs(dg - sg); bb = Math.Abs(db - sb);
                    break;
                case BlendMode.Exclusion:
                    br = dr + sr - 2f * dr * sr;
                    bg = dg + sg - 2f * dg * sg;
                    bb = db + sb - 2f * db * sb;
                    break;
                case BlendMode.Hue:
                case BlendMode.Saturation:
                case BlendMode.Color:
                case BlendMode.Luminosity:
                    (br, bg, bb) = ComponentBlend(dr, dg, db, sr, sg, sb, blendMode);
                    break;
                case BlendMode.DarkerColor:
                    var dl = Luminance(dr, dg, db);
                    var sl = Luminance(sr, sg, sb);
                    if (sl < dl) { br = sr; bg = sg; bb = sb; }
                    break;
                case BlendMode.LighterColor:
                    dl = Luminance(dr, dg, db);
                    sl = Luminance(sr, sg, sb);
                    if (sl > dl) { br = sr; bg = sg; bb = sb; }
                    break;
                case BlendMode.Dissolve:
                    // Dissolve: randomly show source or dest based on alpha
                    if (Random.Shared.NextDouble() < srcAlpha)
                    {
                        br = sr; bg = sg; bb = sb;
                        srcAlpha = 1f; // Already made the decision
                    }
                    else continue; // Keep destination pixel
                    break;
                default:
                    br = sr; bg = sg; bb = sb;
                    break;
            }

            // Alpha-blend the result with the destination
            float da = dst.A / 255f;
            float outAlpha = srcAlpha + da * (1f - srcAlpha);
            if (outAlpha > 0.001f)
            {
                float outR = (br * srcAlpha + dr * da * (1f - srcAlpha)) / outAlpha;
                float outG = (bg * srcAlpha + dg * da * (1f - srcAlpha)) / outAlpha;
                float outB = (bb * srcAlpha + db * da * (1f - srcAlpha)) / outAlpha;

                destPixels[i] = Color.FromArgb(
                    (byte)Math.Clamp((int)(outAlpha * 255f), 0, 255),
                    (byte)Math.Clamp((int)(outR * 255f), 0, 255),
                    (byte)Math.Clamp((int)(outG * 255f), 0, 255),
                    (byte)Math.Clamp((int)(outB * 255f), 0, 255));
            }
        }

        destination.SetPixelColors(destPixels);
    }

    private static float SoftLightChannel(float b, float s)
    {
        if (s <= 0.5f)
            return b - (1f - 2f * s) * b * (1f - b);

        float d = b <= 0.25f
            ? ((16f * b - 12f) * b + 4f) * b
            : MathF.Sqrt(b);
        return b + (2f * s - 1f) * (d - b);
    }

    private static float Luminance(float r, float g, float b) => 0.299f * r + 0.587f * g + 0.114f * b;

    private static (float r, float g, float b) RgbToHsl(float r, float g, float b)
    {
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float l = (max + min) / 2f;
        if (Math.Abs(max - min) < 0.001f)
            return (0f, 0f, l);
        float d = max - min;
        float s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
        float h;
        if (max == r) h = (g - b) / d + (g < b ? 6f : 0f);
        else if (max == g) h = (b - r) / d + 2f;
        else h = (r - g) / d + 4f;
        h /= 6f;
        return (h, s, l);
    }

    private static (float r, float g, float b) HslToRgb(float h, float s, float l)
    {
        if (s < 0.001f) return (l, l, l);
        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;
        return (HueToRgb(p, q, h + 1f / 3f), HueToRgb(p, q, h), HueToRgb(p, q, h - 1f / 3f));
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

    private static (float r, float g, float b) ComponentBlend(
        float dr, float dg, float db, float sr, float sg, float sb, BlendMode mode)
    {
        var (dh, ds, dl) = RgbToHsl(dr, dg, db);
        var (sh, ss, sl) = RgbToHsl(sr, sg, sb);

        return mode switch
        {
            BlendMode.Hue => HslToRgb(sh, ds, dl),
            BlendMode.Saturation => HslToRgb(dh, ss, dl),
            BlendMode.Color => HslToRgb(sh, ss, dl),
            BlendMode.Luminosity => HslToRgb(dh, ds, sl),
            _ => (sr, sg, sb)
        };
    }
}
