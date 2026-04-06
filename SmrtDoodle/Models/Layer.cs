using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SmrtDoodle.Models;

public class Layer : IDisposable
{
    private bool _disposed;

    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;
    public bool IsLocked { get; set; }
    public BlendMode BlendMode { get; set; } = BlendMode.Normal;
    public CanvasRenderTarget? Bitmap { get; set; }

    // Layer grouping
    public string? ParentGroupId { get; set; }

    // Layer mask (grayscale: white=visible, black=hidden)
    public CanvasRenderTarget? MaskBitmap { get; set; }
    public bool IsMaskEnabled { get; set; }
    public bool IsEditingMask { get; set; }

    // Layer effects
    public List<LayerEffect> Effects { get; } = new();

    public Layer(string name)
    {
        Name = name;
    }

    public void Initialize(ICanvasResourceCreator resourceCreator, int width, int height, float dpi)
    {
        Bitmap?.Dispose();
        Bitmap = new CanvasRenderTarget(resourceCreator, width, height, dpi);
        using var ds = Bitmap.CreateDrawingSession();
        ds.Clear(Microsoft.UI.Colors.Transparent);
    }

    public void Clear()
    {
        if (Bitmap == null) return;
        using var ds = Bitmap.CreateDrawingSession();
        ds.Clear(Microsoft.UI.Colors.Transparent);
    }

    public Layer Clone(ICanvasResourceCreator resourceCreator)
    {
        var clone = new Layer($"{Name} (Copy)")
        {
            IsVisible = IsVisible,
            Opacity = Opacity,
            BlendMode = BlendMode,
            ParentGroupId = ParentGroupId,
            IsMaskEnabled = IsMaskEnabled
        };
        if (Bitmap != null)
        {
            clone.Bitmap = new CanvasRenderTarget(resourceCreator, (float)Bitmap.SizeInPixels.Width, (float)Bitmap.SizeInPixels.Height, Bitmap.Dpi);
            using var ds = clone.Bitmap.CreateDrawingSession();
            ds.DrawImage(Bitmap);
        }
        if (MaskBitmap != null)
        {
            clone.MaskBitmap = new CanvasRenderTarget(resourceCreator, (float)MaskBitmap.SizeInPixels.Width, (float)MaskBitmap.SizeInPixels.Height, MaskBitmap.Dpi);
            using var ds = clone.MaskBitmap.CreateDrawingSession();
            ds.DrawImage(MaskBitmap);
        }
        foreach (var effect in Effects)
            clone.Effects.Add(effect.Clone());
        return clone;
    }

    /// <summary>
    /// Initializes a white (fully visible) mask for this layer.
    /// </summary>
    public void InitializeMask(ICanvasResourceCreator resourceCreator, int width, int height, float dpi)
    {
        MaskBitmap?.Dispose();
        MaskBitmap = new CanvasRenderTarget(resourceCreator, width, height, dpi);
        using var ds = MaskBitmap.CreateDrawingSession();
        ds.Clear(Microsoft.UI.Colors.White);
        IsMaskEnabled = true;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Bitmap?.Dispose();
            Bitmap = null;
            MaskBitmap?.Dispose();
            MaskBitmap = null;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
