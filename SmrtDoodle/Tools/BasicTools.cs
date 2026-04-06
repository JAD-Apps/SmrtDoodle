using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using SmrtDoodle.Models;
using System;
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
        // Single-click: draw a solid dot at full stroke radius so it's always visible
        var radius = Math.Max(1f, strokeWidth / 2f);
        ds.FillCircle(point, radius, color);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        DrawFilledStroke(ds, LastPoint, point, color, Math.Max(1f, strokeWidth / 2f));
        LastPoint = point;
    }

    /// <summary>
    /// Draws a continuous solid stroke by stamping filled circles at tight intervals.
    /// Spacing is capped at radius/3 to ensure no visible gaps between stamps.
    /// </summary>
    private static void DrawFilledStroke(CanvasDrawingSession ds, Vector2 from, Vector2 to, Color color, float radius)
    {
        var distance = Vector2.Distance(from, to);
        // Use tight spacing (radius * 0.25) to guarantee gap-free strokes
        var spacing = Math.Max(0.5f, radius * 0.25f);
        var steps = Math.Max(1, (int)MathF.Ceiling(distance / spacing));

        for (int i = 0; i <= steps; i++)
        {
            var t = (float)i / steps;
            var point = Vector2.Lerp(from, to, t);
            ds.FillCircle(point, radius, color);
        }
    }
}

public class BrushTool : ToolBase
{
    private readonly Random _rng = new();

    public override string Name => "Brush";
    public override string Icon => "\uE790";

    public BrushStyle CurrentStyle { get; set; } = BrushStyle.Normal;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
        DrawBrushDab(ds, point, point, color, strokeWidth, isFirstDab: true);
    }

    public override void OnPointerMoved(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        if (!IsDrawing) return;
        DrawBrushDab(ds, LastPoint, point, color, strokeWidth, isFirstDab: false);
        LastPoint = point;
    }

    private void DrawBrushDab(CanvasDrawingSession ds, Vector2 from, Vector2 to, Color color, float strokeWidth, bool isFirstDab)
    {
        /// <summary>
        /// Stamps filled circles along a line with configurable spacing.
        /// Default spacing factor 0.2 produces dense, gap-free strokes.
        /// </summary>
        static void StampFilledCircles(CanvasDrawingSession canvas, Vector2 start, Vector2 end, Color stampColor, float radius, float spacingFactor = 0.2f)
        {
            var r = Math.Max(1f, radius);
            var distance = Vector2.Distance(start, end);
            var spacing = Math.Max(0.5f, r * spacingFactor);
            var steps = Math.Max(1, (int)MathF.Ceiling(distance / spacing));

            for (int i = 0; i <= steps; i++)
            {
                var t = (float)i / steps;
                var p = Vector2.Lerp(start, end, t);
                canvas.FillCircle(p, r, stampColor);
            }
        }

        switch (CurrentStyle)
        {
            case BrushStyle.Normal:
            {
                // Solid, gap-free brush with tight spacing
                StampFilledCircles(ds, from, to, color, strokeWidth * 0.5f, 0.2f);
                break;
            }
            case BrushStyle.Calligraphy:
            {
                // Elongated elliptical dabs with velocity-based width variation
                var dist = Vector2.Distance(from, to);
                var steps = Math.Max(1, (int)MathF.Ceiling(dist / Math.Max(1f, strokeWidth * 0.15f)));

                // Velocity affects the minor axis — faster strokes are thinner
                var velocity = dist / Math.Max(1, steps);
                var velocityFactor = Math.Clamp(1.0f - velocity * 0.02f, 0.3f, 1.0f);

                var major = Math.Max(1f, strokeWidth * 0.8f);
                var minor = Math.Max(1f, strokeWidth * 0.25f * velocityFactor);

                for (int i = 0; i <= steps; i++)
                {
                    var t = (float)i / steps;
                    var center = Vector2.Lerp(from, to, t);

                    // Rotate the ellipse along the stroke direction
                    var direction = to - from;
                    var angle = dist > 0.5f ? MathF.Atan2(direction.Y, direction.X) : MathF.PI / 4f;

                    // Use a transform to rotate the ellipse
                    var prevTransform = ds.Transform;
                    ds.Transform = Matrix3x2.CreateRotation(angle, center) * prevTransform;
                    ds.FillEllipse(center.X, center.Y, major, minor, color);
                    ds.Transform = prevTransform;
                }
                break;
            }
            case BrushStyle.Airbrush:
            {
                // Dense scatter spray with Gaussian-like falloff from center
                var dist = Vector2.Distance(from, to);
                var particleCount = Math.Max(20, (int)(dist * strokeWidth * 0.6f));
                if (isFirstDab) particleCount = Math.Max(30, (int)(strokeWidth * 3));

                for (int i = 0; i < particleCount; i++)
                {
                    var t = (float)_rng.NextDouble();
                    var center = Vector2.Lerp(from, to, t);

                    // Box-Muller for Gaussian distribution — particles cluster near center
                    var u1 = Math.Max(1e-6, _rng.NextDouble());
                    var u2 = _rng.NextDouble();
                    var gaussRadius = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
                    gaussRadius = Math.Clamp(gaussRadius, -2.5f, 2.5f);

                    var angle = (float)(_rng.NextDouble() * Math.PI * 2);
                    var radius = Math.Abs(gaussRadius) * strokeWidth * 0.4f;
                    var px = center.X + MathF.Cos(angle) * radius;
                    var py = center.Y + MathF.Sin(angle) * radius;

                    // Alpha decreases with distance from center (Gaussian falloff)
                    var normalizedDist = Math.Abs(gaussRadius) / 2.5f;
                    var alpha = (byte)Math.Max(10, (int)(180 * (1.0 - normalizedDist * normalizedDist)));
                    var sprayColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                    ds.FillCircle(px, py, Math.Max(0.5f, 1.5f - normalizedDist), sprayColor);
                }
                break;
            }
            case BrushStyle.Oil:
            {
                // Thick, opaque strokes with a highlight streak for 3D appearance
                StampFilledCircles(ds, from, to, color, strokeWidth * 0.75f, 0.15f);

                // Semi-transparent highlight shifted upward simulates light reflection
                var highlight = Color.FromArgb(50, 255, 255, 255);
                var highlightOffset = new Vector2(strokeWidth * 0.05f, -strokeWidth * 0.2f);
                StampFilledCircles(ds, from + highlightOffset, to + highlightOffset, highlight, strokeWidth * 0.25f, 0.3f);

                // Subtle dark edge at bottom for depth
                var shadow = Color.FromArgb(25, 0, 0, 0);
                var shadowOffset = new Vector2(0, strokeWidth * 0.15f);
                StampFilledCircles(ds, from + shadowOffset, to + shadowOffset, shadow, strokeWidth * 0.15f, 0.4f);
                break;
            }
            case BrushStyle.Crayon:
            {
                // Rough, textured strokes with grain pattern — scattered jittered dots
                var dist = Vector2.Distance(from, to);
                var steps = Math.Max(5, (int)(dist / Math.Max(1f, strokeWidth * 0.15f)));
                if (isFirstDab) steps = Math.Max(8, (int)(strokeWidth * 1.5f));

                for (int i = 0; i < steps; i++)
                {
                    var t = (float)i / steps;
                    var pt = Vector2.Lerp(from, to, t);

                    // Scatter multiple grain particles per step for texture density
                    var grainCount = Math.Max(2, (int)(strokeWidth * 0.4f));
                    for (int g = 0; g < grainCount; g++)
                    {
                        var jx = (float)(_rng.NextDouble() - 0.5) * strokeWidth * 0.7f;
                        var jy = (float)(_rng.NextDouble() - 0.5) * strokeWidth * 0.7f;

                        // Only draw if within the brush radius (circular mask)
                        if (jx * jx + jy * jy > strokeWidth * strokeWidth * 0.12f) continue;

                        var alpha = (byte)(_rng.Next(120, 240));
                        var c = Color.FromArgb(alpha, color.R, color.G, color.B);
                        var dotSize = Math.Max(0.5f, strokeWidth * 0.15f * (float)(0.5 + _rng.NextDouble()));
                        ds.FillCircle(pt.X + jx, pt.Y + jy, dotSize, c);
                    }
                }
                break;
            }
            case BrushStyle.Marker:
            {
                // Semi-transparent, flat-tipped marker — uses CanvasComposite mode approach
                // Alpha accumulates to create the characteristic marker overlap darkening
                var alpha = (byte)Math.Min(100, (int)color.A);
                var markerColor = Color.FromArgb(alpha, color.R, color.G, color.B);
                StampFilledCircles(ds, from, to, markerColor, strokeWidth * 0.9f, 0.15f);
                break;
            }
            case BrushStyle.NaturalPencil:
            {
                // Fine graphite-like line with grain texture and pressure-varying alpha
                var dist = Vector2.Distance(from, to);
                var steps = Math.Max(3, (int)(dist / Math.Max(0.5f, strokeWidth * 0.08f)));
                if (isFirstDab) steps = Math.Max(5, (int)(strokeWidth * 2));

                // Velocity simulates pressure — slower = darker, faster = lighter
                var velocityAlphaFactor = Math.Clamp(1.0f - dist * 0.005f, 0.4f, 1.0f);

                for (int i = 0; i < steps; i++)
                {
                    var t = (float)i / steps;
                    var pt = Vector2.Lerp(from, to, t);

                    // Fine micro-jitter for graphite grain texture
                    var jx = (float)(_rng.NextDouble() - 0.5) * strokeWidth * 0.2f;
                    var jy = (float)(_rng.NextDouble() - 0.5) * strokeWidth * 0.2f;

                    var baseAlpha = (int)(180 * velocityAlphaFactor);
                    var alpha = (byte)Math.Clamp(_rng.Next(baseAlpha - 40, baseAlpha + 20), 60, 230);
                    var c = Color.FromArgb(alpha, color.R, color.G, color.B);

                    // Small dot radius for fine pencil texture
                    var dotRadius = Math.Max(0.3f, strokeWidth * 0.15f * (float)(0.6 + _rng.NextDouble() * 0.4));
                    ds.FillCircle(pt.X + jx, pt.Y + jy, dotRadius, c);
                }
                break;
            }
            case BrushStyle.Watercolor:
            {
                // Soft, translucent layers with wet-edge darkening effect
                var baseAlpha = (byte)Math.Min(35, (int)color.A);
                var innerColor = Color.FromArgb(baseAlpha, color.R, color.G, color.B);
                StampFilledCircles(ds, from, to, innerColor, strokeWidth * 1.1f, 0.2f);

                // Outer wash — very transparent, larger radius for soft bleeding
                var outerAlpha = (byte)(baseAlpha / 3);
                var outerColor = Color.FromArgb(outerAlpha, color.R, color.G, color.B);
                StampFilledCircles(ds, from, to, outerColor, strokeWidth * 1.6f, 0.35f);

                // Wet-edge effect: slightly darker ring at the edge of the stroke
                var edgeAlpha = (byte)Math.Min(25, (int)baseAlpha);
                var edgeColor = Color.FromArgb(edgeAlpha, 
                    (byte)Math.Max(0, color.R - 30), 
                    (byte)Math.Max(0, color.G - 30), 
                    (byte)Math.Max(0, color.B - 30));
                var edgeOffset = strokeWidth * 0.08f;
                var dist = Vector2.Distance(from, to);
                var edgeSteps = Math.Max(2, (int)MathF.Ceiling(dist / Math.Max(1f, strokeWidth * 0.4f)));
                for (int i = 0; i <= edgeSteps; i++)
                {
                    var t = (float)i / edgeSteps;
                    var pt = Vector2.Lerp(from, to, t);
                    // Draw a thin ring at the outer edge
                    ds.DrawCircle(pt, strokeWidth * 1.1f, edgeColor, edgeOffset);
                }
                break;
            }
        }
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

/// <summary>
/// Magnifier tool — zoom is handled by the main window; this is a no-op placeholder.
/// </summary>
public class MagnifierTool : ToolBase
{
    public override string Name => "Magnifier";
    public override string Icon => "\uE71E";
}
