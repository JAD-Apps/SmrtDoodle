using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;

namespace SmrtDoodle.Models;

public interface IUndoRedoAction : IDisposable
{
    string Description { get; }
    long EstimatedBytes { get; }
    void Undo();
    void Redo();
}

/// <summary>
/// Diff-based undo action that only stores the changed region of a layer bitmap.
/// Compares before/after pixel data to find the minimal bounding rectangle of changes,
/// then stores only those pixels — dramatically reducing memory usage for large canvases.
/// </summary>
public class BitmapUndoAction : IUndoRedoAction
{
    private readonly Layer _layer;
    private readonly ICanvasResourceCreator _resourceCreator;
    private CanvasRenderTarget? _fullBeforeCapture; // Temp: full bitmap before stroke
    private CanvasRenderTarget? _beforePatch;       // Stored: only the changed region (before)
    private CanvasRenderTarget? _afterPatch;        // Stored: only the changed region (after)
    private int _patchX, _patchY, _patchWidth, _patchHeight;
    private bool _disposed;
    private bool _isFullBitmap; // Fallback: store full bitmap if diff fails

    public string Description { get; }
    public long EstimatedBytes { get; private set; }

    public BitmapUndoAction(Layer layer, ICanvasResourceCreator resourceCreator, string description)
    {
        _layer = layer;
        _resourceCreator = resourceCreator;
        Description = description;
    }

    public void CaptureBeforeState()
    {
        if (_layer.Bitmap == null) return;
        _fullBeforeCapture = new CanvasRenderTarget(_resourceCreator,
            (float)_layer.Bitmap.SizeInPixels.Width, (float)_layer.Bitmap.SizeInPixels.Height, _layer.Bitmap.Dpi);
        using var ds = _fullBeforeCapture.CreateDrawingSession();
        ds.DrawImage(_layer.Bitmap);
    }

    public void CaptureAfterState()
    {
        if (_layer.Bitmap == null || _fullBeforeCapture == null) return;

        var width = (int)_layer.Bitmap.SizeInPixels.Width;
        var height = (int)_layer.Bitmap.SizeInPixels.Height;

        var beforePixels = _fullBeforeCapture.GetPixelColors();
        var afterPixels = _layer.Bitmap.GetPixelColors();

        // Find the bounding rectangle of changed pixels
        int minX = width, minY = height, maxX = -1, maxY = -1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                if (idx >= beforePixels.Length || idx >= afterPixels.Length) continue;
                var b = beforePixels[idx];
                var a = afterPixels[idx];
                if (b.A != a.A || b.R != a.R || b.G != a.G || b.B != a.B)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (maxX < 0 || maxY < 0)
        {
            // No change detected — store nothing
            _fullBeforeCapture?.Dispose();
            _fullBeforeCapture = null;
            EstimatedBytes = 0;
            return;
        }

        _patchX = minX;
        _patchY = minY;
        _patchWidth = maxX - minX + 1;
        _patchHeight = maxY - minY + 1;

        // If the dirty rect covers >50% of the canvas, just store full bitmaps (simpler and not much waste)
        var totalPixels = (long)width * height;
        var patchPixels = (long)_patchWidth * _patchHeight;

        if (patchPixels > totalPixels / 2)
        {
            _isFullBitmap = true;
            _beforePatch = _fullBeforeCapture;
            _fullBeforeCapture = null; // Transfer ownership
            _afterPatch = new CanvasRenderTarget(_resourceCreator, width, height, _layer.Bitmap.Dpi);
            using var ds = _afterPatch.CreateDrawingSession();
            ds.DrawImage(_layer.Bitmap);
            EstimatedBytes = totalPixels * 4 * 2; // 2 full RGBA bitmaps
            return;
        }

        // Extract only the changed region as small patches
        var dpi = _layer.Bitmap.Dpi;
        _beforePatch = new CanvasRenderTarget(_resourceCreator, _patchWidth, _patchHeight, dpi);
        _afterPatch = new CanvasRenderTarget(_resourceCreator, _patchWidth, _patchHeight, dpi);

        // Copy the dirty rect from before/after
        var patchRect = new Windows.Foundation.Rect(_patchX, _patchY, _patchWidth, _patchHeight);
        var destRect = new Windows.Foundation.Rect(0, 0, _patchWidth, _patchHeight);

        using (var ds = _beforePatch.CreateDrawingSession())
        {
            ds.Clear(Microsoft.UI.Colors.Transparent);
            ds.DrawImage(_fullBeforeCapture, destRect, patchRect);
        }
        using (var ds = _afterPatch.CreateDrawingSession())
        {
            ds.Clear(Microsoft.UI.Colors.Transparent);
            ds.DrawImage(_layer.Bitmap, destRect, patchRect);
        }

        EstimatedBytes = patchPixels * 4 * 2; // 2 RGBA patches

        // Release the full before capture
        _fullBeforeCapture.Dispose();
        _fullBeforeCapture = null;
    }

    public void Undo()
    {
        if (_beforePatch == null || _layer.Bitmap == null) return;

        if (_isFullBitmap)
        {
            using var ds = _layer.Bitmap.CreateDrawingSession();
            ds.Clear(Microsoft.UI.Colors.Transparent);
            ds.DrawImage(_beforePatch);
        }
        else
        {
            // Restore only the changed patch region
            using var ds = _layer.Bitmap.CreateDrawingSession();
            ds.Blend = CanvasBlend.Copy;
            ds.DrawImage(_beforePatch, _patchX, _patchY);
            ds.Blend = CanvasBlend.SourceOver;
        }
    }

    public void Redo()
    {
        if (_afterPatch == null || _layer.Bitmap == null) return;

        if (_isFullBitmap)
        {
            using var ds = _layer.Bitmap.CreateDrawingSession();
            ds.Clear(Microsoft.UI.Colors.Transparent);
            ds.DrawImage(_afterPatch);
        }
        else
        {
            using var ds = _layer.Bitmap.CreateDrawingSession();
            ds.Blend = CanvasBlend.Copy;
            ds.DrawImage(_afterPatch, _patchX, _patchY);
            ds.Blend = CanvasBlend.SourceOver;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _fullBeforeCapture?.Dispose();
            _beforePatch?.Dispose();
            _afterPatch?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

public class UndoRedoManager : IDisposable
{
    private readonly Stack<IUndoRedoAction> _undoStack = new();
    private readonly Stack<IUndoRedoAction> _redoStack = new();
    private bool _disposed;
    private long _totalBytes;

    public int MaxHistory { get; set; } = 50;

    /// <summary>Maximum memory budget for undo history in bytes. Default 512MB.</summary>
    public long MaxMemoryBytes { get; set; } = 512L * 1024 * 1024;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public event EventHandler? StateChanged;

    public void Push(IUndoRedoAction action)
    {
        _undoStack.Push(action);
        _totalBytes += action.EstimatedBytes;

        foreach (var item in _redoStack)
        {
            _totalBytes -= item.EstimatedBytes;
            item.Dispose();
        }
        _redoStack.Clear();

        // Trim history by count limit
        while (_undoStack.Count > MaxHistory)
        {
            TrimOldest();
        }

        // Trim history by memory budget
        while (_totalBytes > MaxMemoryBytes && _undoStack.Count > 1)
        {
            TrimOldest();
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TrimOldest()
    {
        var items = new Stack<IUndoRedoAction>();
        while (_undoStack.Count > 1)
            items.Push(_undoStack.Pop());
        var oldest = _undoStack.Pop();
        _totalBytes -= oldest.EstimatedBytes;
        oldest.Dispose();
        while (items.Count > 0)
            _undoStack.Push(items.Pop());
    }

    public void Undo()
    {
        if (!CanUndo) return;
        var action = _undoStack.Pop();
        action.Undo();
        _redoStack.Push(action);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Redo()
    {
        if (!CanRedo) return;
        var action = _redoStack.Pop();
        action.Redo();
        _undoStack.Push(action);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        foreach (var item in _undoStack) item.Dispose();
        foreach (var item in _redoStack) item.Dispose();
        _undoStack.Clear();
        _redoStack.Clear();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
