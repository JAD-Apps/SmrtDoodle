using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;

namespace SmrtDoodle.Models;

public interface IUndoRedoAction : IDisposable
{
    string Description { get; }
    void Undo();
    void Redo();
}

public class BitmapUndoAction : IUndoRedoAction
{
    private readonly Layer _layer;
    private readonly ICanvasResourceCreator _resourceCreator;
    private CanvasRenderTarget? _beforeState;
    private CanvasRenderTarget? _afterState;
    private bool _disposed;

    public string Description { get; }

    public BitmapUndoAction(Layer layer, ICanvasResourceCreator resourceCreator, string description)
    {
        _layer = layer;
        _resourceCreator = resourceCreator;
        Description = description;
    }

    public void CaptureBeforeState()
    {
        if (_layer.Bitmap == null) return;
        _beforeState = new CanvasRenderTarget(_resourceCreator,
            (float)_layer.Bitmap.SizeInPixels.Width, (float)_layer.Bitmap.SizeInPixels.Height, _layer.Bitmap.Dpi);
        using var ds = _beforeState.CreateDrawingSession();
        ds.DrawImage(_layer.Bitmap);
    }

    public void CaptureAfterState()
    {
        if (_layer.Bitmap == null) return;
        _afterState = new CanvasRenderTarget(_resourceCreator,
            (float)_layer.Bitmap.SizeInPixels.Width, (float)_layer.Bitmap.SizeInPixels.Height, _layer.Bitmap.Dpi);
        using var ds = _afterState.CreateDrawingSession();
        ds.DrawImage(_layer.Bitmap);
    }

    public void Undo()
    {
        if (_beforeState == null || _layer.Bitmap == null) return;
        using var ds = _layer.Bitmap.CreateDrawingSession();
        ds.Clear(Microsoft.UI.Colors.Transparent);
        ds.DrawImage(_beforeState);
    }

    public void Redo()
    {
        if (_afterState == null || _layer.Bitmap == null) return;
        using var ds = _layer.Bitmap.CreateDrawingSession();
        ds.Clear(Microsoft.UI.Colors.Transparent);
        ds.DrawImage(_afterState);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _beforeState?.Dispose();
            _afterState?.Dispose();
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

    public int MaxHistory { get; set; } = 50;
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public event EventHandler? StateChanged;

    public void Push(IUndoRedoAction action)
    {
        _undoStack.Push(action);
        foreach (var item in _redoStack)
            item.Dispose();
        _redoStack.Clear();

        while (_undoStack.Count > MaxHistory)
        {
            var items = new Stack<IUndoRedoAction>();
            while (_undoStack.Count > 1)
                items.Push(_undoStack.Pop());
            _undoStack.Pop().Dispose();
            while (items.Count > 0)
                _undoStack.Push(items.Pop());
        }
        StateChanged?.Invoke(this, EventArgs.Empty);
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
