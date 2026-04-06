using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SmrtDoodle.Helpers;

/// <summary>
/// Tracks dirty regions for partial canvas redraw optimization.
/// Accumulates invalidated rects and provides the union for efficient re-render.
/// </summary>
public class DirtyRectTracker
{
    private Rect _dirtyRect = Rect.Empty;
    private bool _fullInvalidate = true;

    /// <summary>Whether any region needs redrawing.</summary>
    public bool IsDirty => _fullInvalidate || !_dirtyRect.IsEmpty;

    /// <summary>Whether the entire canvas must be redrawn.</summary>
    public bool IsFullInvalidate => _fullInvalidate;

    /// <summary>The accumulated dirty region.</summary>
    public Rect DirtyRegion => _fullInvalidate ? Rect.Empty : _dirtyRect;

    /// <summary>Mark a specific rect as needing redraw.</summary>
    public void Invalidate(Rect rect)
    {
        if (_fullInvalidate) return;
        if (_dirtyRect.IsEmpty)
            _dirtyRect = rect;
        else
        {
            _dirtyRect.Union(rect);
        }
    }

    /// <summary>Mark a circular region as dirty (e.g., brush stroke).</summary>
    public void InvalidateCircle(float cx, float cy, float radius)
    {
        Invalidate(new Rect(cx - radius, cy - radius, radius * 2, radius * 2));
    }

    /// <summary>Mark the entire canvas as needing full redraw.</summary>
    public void InvalidateAll()
    {
        _fullInvalidate = true;
        _dirtyRect = Rect.Empty;
    }

    /// <summary>Reset dirty state after a redraw.</summary>
    public void Clear()
    {
        _fullInvalidate = false;
        _dirtyRect = Rect.Empty;
    }
}

/// <summary>
/// Throttles canvas invalidation to a target frame rate to prevent excessive redraws.
/// </summary>
public class RenderThrottler
{
    private readonly int _minFrameIntervalMs;
    private long _lastRenderTicks;
    private bool _pendingInvalidate;

    public RenderThrottler(int targetFps = 60)
    {
        _minFrameIntervalMs = 1000 / targetFps;
    }

    /// <summary>
    /// Returns true if enough time has passed since last render.
    /// If not ready, sets a pending flag so the next check can trigger.
    /// </summary>
    public bool ShouldRender()
    {
        var now = Environment.TickCount64;
        if (now - _lastRenderTicks >= _minFrameIntervalMs)
        {
            _lastRenderTicks = now;
            _pendingInvalidate = false;
            return true;
        }
        _pendingInvalidate = true;
        return false;
    }

    /// <summary>Whether a render was deferred and should be flushed.</summary>
    public bool HasPendingRender => _pendingInvalidate;

    /// <summary>Force next ShouldRender to return true.</summary>
    public void ForceNextRender()
    {
        _lastRenderTicks = 0;
        _pendingInvalidate = false;
    }
}

/// <summary>
/// Spatial grid for tiled rendering. Divides the canvas into fixed-size tiles and
/// tracks which tiles overlap the viewport for efficient viewport culling.
/// </summary>
public class TileGrid
{
    private readonly int _tileSize;
    private int _columns;
    private int _rows;
    private bool[]? _dirtyTiles;

    /// <summary>Tile size in pixels (default 512).</summary>
    public int TileSize => _tileSize;

    /// <summary>Number of tile columns.</summary>
    public int Columns => _columns;

    /// <summary>Number of tile rows.</summary>
    public int Rows => _rows;

    /// <summary>Total number of tiles.</summary>
    public int TileCount => _columns * _rows;

    public TileGrid(int tileSize = 512)
    {
        _tileSize = tileSize > 0 ? tileSize : 512;
    }

    /// <summary>
    /// Recalculate grid dimensions for a new canvas size.
    /// Marks all tiles dirty.
    /// </summary>
    public void Resize(int canvasWidth, int canvasHeight)
    {
        _columns = Math.Max(1, (int)Math.Ceiling((double)canvasWidth / _tileSize));
        _rows = Math.Max(1, (int)Math.Ceiling((double)canvasHeight / _tileSize));
        _dirtyTiles = new bool[_columns * _rows];
        InvalidateAll();
    }

    /// <summary>Mark all tiles as dirty.</summary>
    public void InvalidateAll()
    {
        if (_dirtyTiles == null) return;
        Array.Fill(_dirtyTiles, true);
    }

    /// <summary>Clear all dirty flags.</summary>
    public void ClearAll()
    {
        if (_dirtyTiles == null) return;
        Array.Fill(_dirtyTiles, false);
    }

    /// <summary>Mark tiles that intersect the given rect as dirty.</summary>
    public void InvalidateRect(Rect rect)
    {
        if (_dirtyTiles == null) return;
        int startCol = Math.Max(0, (int)(rect.X / _tileSize));
        int startRow = Math.Max(0, (int)(rect.Y / _tileSize));
        int endCol = Math.Min(_columns - 1, (int)((rect.X + rect.Width) / _tileSize));
        int endRow = Math.Min(_rows - 1, (int)((rect.Y + rect.Height) / _tileSize));

        for (int r = startRow; r <= endRow; r++)
            for (int c = startCol; c <= endCol; c++)
                _dirtyTiles[r * _columns + c] = true;
    }

    /// <summary>Check if a specific tile is dirty.</summary>
    public bool IsTileDirty(int col, int row)
    {
        if (_dirtyTiles == null || col < 0 || col >= _columns || row < 0 || row >= _rows)
            return true;
        return _dirtyTiles[row * _columns + col];
    }

    /// <summary>Get the pixel rect for a specific tile.</summary>
    public Rect GetTileRect(int col, int row)
    {
        return new Rect(col * _tileSize, row * _tileSize, _tileSize, _tileSize);
    }

    /// <summary>
    /// Get indices of tiles visible within the given viewport rect.
    /// Used for viewport culling — only render tiles the user can see.
    /// </summary>
    public IEnumerable<(int Col, int Row)> GetVisibleTiles(Rect viewport)
    {
        if (_dirtyTiles == null) yield break;

        int startCol = Math.Max(0, (int)(viewport.X / _tileSize));
        int startRow = Math.Max(0, (int)(viewport.Y / _tileSize));
        int endCol = Math.Min(_columns - 1, (int)((viewport.X + viewport.Width) / _tileSize));
        int endRow = Math.Min(_rows - 1, (int)((viewport.Y + viewport.Height) / _tileSize));

        for (int r = startRow; r <= endRow; r++)
            for (int c = startCol; c <= endCol; c++)
                yield return (c, r);
    }

    /// <summary>
    /// Get indices of tiles that are both visible AND dirty — the minimal redraw set.
    /// </summary>
    public IEnumerable<(int Col, int Row)> GetDirtyVisibleTiles(Rect viewport)
    {
        foreach (var (col, row) in GetVisibleTiles(viewport))
        {
            if (IsTileDirty(col, row))
                yield return (col, row);
        }
    }
}
