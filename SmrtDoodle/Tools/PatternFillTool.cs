using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using Windows.UI;

namespace SmrtDoodle.Tools;

public enum PatternType
{
    Checkerboard,
    DiagonalLines,
    Dots,
    Crosshatch,
    Brick
}

/// <summary>
/// Pattern Fill tool — fills a region with a repeating tile pattern.
/// </summary>
public class PatternFillTool : ToolBase
{
    public override string Name => "Pattern Fill";
    public override string Icon => "\uE771";

    public PatternType Pattern { get; set; } = PatternType.Checkerboard;
    public int TileSize { get; set; } = 16;

    public override void OnPointerPressed(CanvasDrawingSession ds, Vector2 point, Color color, float strokeWidth)
    {
        base.OnPointerPressed(ds, point, color, strokeWidth);
    }

    /// <summary>
    /// Fills the entire canvas with the selected pattern.
    /// </summary>
    public void FillWithPattern(CanvasDrawingSession ds, Color primaryColor, Color secondaryColor,
        float canvasWidth, float canvasHeight)
    {
        int w = (int)canvasWidth, h = (int)canvasHeight;
        int tile = Math.Max(2, TileSize);

        switch (Pattern)
        {
            case PatternType.Checkerboard:
                for (int y = 0; y < h; y += tile)
                {
                    for (int x = 0; x < w; x += tile)
                    {
                        var isEven = ((x / tile) + (y / tile)) % 2 == 0;
                        ds.FillRectangle(x, y, tile, tile, isEven ? primaryColor : secondaryColor);
                    }
                }
                break;

            case PatternType.DiagonalLines:
                ds.FillRectangle(0, 0, w, h, secondaryColor);
                for (int i = -h; i < w + h; i += tile)
                {
                    ds.DrawLine(i, 0, i + h, h, primaryColor, Math.Max(1, tile / 4f));
                }
                break;

            case PatternType.Dots:
                ds.FillRectangle(0, 0, w, h, secondaryColor);
                var dotRadius = Math.Max(1f, tile * 0.2f);
                for (int y = tile / 2; y < h; y += tile)
                {
                    for (int x = tile / 2; x < w; x += tile)
                    {
                        ds.FillCircle(x, y, dotRadius, primaryColor);
                    }
                }
                break;

            case PatternType.Crosshatch:
                ds.FillRectangle(0, 0, w, h, secondaryColor);
                var lineWidth = Math.Max(1, tile / 6f);
                for (int i = 0; i < Math.Max(w, h) + tile; i += tile)
                {
                    // Forward diagonal
                    ds.DrawLine(i, 0, i - h, h, primaryColor, lineWidth);
                    // Backward diagonal
                    ds.DrawLine(i - h, 0, i, h, primaryColor, lineWidth);
                }
                break;

            case PatternType.Brick:
                ds.FillRectangle(0, 0, w, h, secondaryColor);
                var brickW = tile * 2;
                var brickH = tile;
                var mortarWidth = Math.Max(1, tile / 8f);
                for (int row = 0; row * brickH < h; row++)
                {
                    var offset = (row % 2 == 0) ? 0 : brickW / 2;
                    var y = row * brickH;
                    // Horizontal mortar line
                    ds.DrawLine(0, y, w, y, primaryColor, mortarWidth);
                    // Vertical mortar lines
                    for (int x = offset; x < w + brickW; x += brickW)
                    {
                        ds.DrawLine(x, y, x, y + brickH, primaryColor, mortarWidth);
                    }
                }
                break;
        }
    }
}
