using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SmrtDoodle.Helpers;
using SmrtDoodle.Models;
using SmrtDoodle.Services;
using SmrtDoodle.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using SelectionMode = SmrtDoodle.Models.SelectionMode;

namespace SmrtDoodle;

public sealed partial class MainWindow : Window
{
    // Services
    private FileService _fileService = null!;
    private readonly ClipboardService _clipboardService = new();
    private readonly IpcService _ipcService = new();

    // Canvas state
    private CanvasSettings _settings = new();
    private readonly ObservableCollection<Layer> _layers = new();
    private int _activeLayerIndex;
    private Layer? ActiveLayer => _activeLayerIndex >= 0 && _activeLayerIndex < _layers.Count ? _layers[_activeLayerIndex] : null;

    // Tools
    private readonly Dictionary<DrawingTool, ITool> _tools = new();
    private DrawingTool _currentToolType = DrawingTool.Pencil;
    private ITool CurrentTool => _tools[_currentToolType];

    // Colors
    private Color _primaryColor = Color.FromArgb(255, 0, 0, 0);
    private Color _secondaryColor = Color.FromArgb(255, 255, 255, 255);

    // Drawing state
    private bool _isPointerDown;
    private BitmapUndoAction? _currentUndoAction;
    private readonly UndoRedoManager _undoManager = new();
    private float _zoomFactor = 1.0f;
    private bool _resourcesCreated;

    // Preview for line/shape
    private Vector2 _previewStart, _previewEnd;
    private bool _isPreviewing;

    // Active drawing color (primary or secondary based on mouse button)
    private Color _activeDrawColor;

    // Color palette
    private readonly List<SolidColorBrush> _paletteColors = new();

    public MainWindow()
    {
        InitializeComponent();
        _fileService = new FileService(this);
        Title = "SmrtDoodle";
        ExtendsContentIntoTitleBar = false;
        InitializeTools();
        InitializeColorPalette();
        ParseLaunchArgs();
        _undoManager.StateChanged += (_, _) => UpdateUndoRedoState();
    }

    private void ParseLaunchArgs()
    {
        var args = Environment.GetCommandLineArgs();
        _ipcService.ParseArguments(args);
        if (_ipcService.IsLaunchedFromSmrtPad)
        {
            InsertIntoDocumentItem.Visibility = Visibility.Visible;
            InsertButton.Visibility = Visibility.Visible;
            Title = "SmrtDoodle - Insert into SmrtPad";
        }
    }

    private void InitializeTools()
    {
        _tools[DrawingTool.Pencil] = new PencilTool();
        _tools[DrawingTool.Brush] = new BrushTool();
        _tools[DrawingTool.Eraser] = new EraserTool();
        _tools[DrawingTool.Fill] = new FillTool();
        _tools[DrawingTool.Text] = new TextTool();
        _tools[DrawingTool.Eyedropper] = new EyedropperTool();
        _tools[DrawingTool.Line] = new LineTool();
        _tools[DrawingTool.Curve] = new CurveTool();
        _tools[DrawingTool.Shape] = new ShapeTool();
        _tools[DrawingTool.Selection] = new SelectionTool();
        _tools[DrawingTool.FreeFormSelection] = new FreeFormSelectionTool();
        HighlightActiveTool();
    }

    private void InitializeColorPalette()
    {
        var colors = new[]
        {
            Colors.Black, Colors.White, Colors.Gray, Colors.DarkRed, Colors.Red, Colors.Orange,
            Colors.Yellow, Colors.Green, Colors.DarkGreen, Colors.Cyan, Colors.Blue, Colors.DarkBlue,
            Colors.Purple, Colors.Magenta, Colors.Pink, Colors.Brown,
            Color.FromArgb(255,192,192,192), Color.FromArgb(255,128,0,0), Color.FromArgb(255,0,128,0),
            Color.FromArgb(255,0,0,128), Color.FromArgb(255,128,128,0), Color.FromArgb(255,0,128,128),
            Color.FromArgb(255,128,0,128), Color.FromArgb(255,255,128,0),
            Color.FromArgb(255,0,255,128), Color.FromArgb(255,128,0,255),
            Color.FromArgb(255,255,200,200), Color.FromArgb(255,200,255,200),
            Color.FromArgb(255,200,200,255), Color.FromArgb(255,255,255,200)
        };
        foreach (var c in colors)
            _paletteColors.Add(new SolidColorBrush(c));
        ColorPaletteGrid.ItemsSource = _paletteColors;
    }

    #region Canvas Events

    private void DrawingCanvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
    {
        InitializeCanvas(sender);
        _resourcesCreated = true;
    }

    private void InitializeCanvas(ICanvasResourceCreator device)
    {
        if (_layers.Count == 0)
        {
            var bg = new Layer("Background");
            bg.Initialize(device, _settings.Width, _settings.Height, _settings.Dpi);
            using (var ds = bg.Bitmap!.CreateDrawingSession())
                ds.Clear(_settings.BackgroundColor);
            _layers.Add(bg);
            _activeLayerIndex = 0;
        }
        RefreshLayerList();
        UpdateCanvasSize();
        UpdateStatusBar();
    }

    private void DrawingCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var ds = args.DrawingSession;
        ds.Clear(Colors.Transparent);

        // Draw checkerboard pattern for transparency
        DrawCheckerboard(ds, _settings.Width * _zoomFactor, _settings.Height * _zoomFactor);

        ds.Transform = Matrix3x2.CreateScale(_zoomFactor);

        // Draw background
        ds.FillRectangle(0, 0, _settings.Width, _settings.Height, _settings.BackgroundColor);

        // Draw all visible layers
        foreach (var layer in _layers)
        {
            if (layer is { IsVisible: true, Bitmap: not null })
                ds.DrawImage(layer.Bitmap, 0, 0, new Rect(0, 0, _settings.Width, _settings.Height), layer.Opacity);
        }

        // Draw preview for line/shape/curve tools
        if (_isPreviewing && (_currentToolType == DrawingTool.Line || _currentToolType == DrawingTool.Shape
            || _currentToolType == DrawingTool.Curve))
        {
            DrawPreview(ds);
        }

        // Draw floating selection and selection rectangle
        if (_currentToolType == DrawingTool.Selection)
        {
            var sel = (SelectionTool)_tools[DrawingTool.Selection];

            // Draw the floating selection bitmap at its current position
            if (sel.HasFloatingSelection)
            {
                ds.DrawImage(sel.SelectionBitmap!, (float)sel.SelectionRect.X, (float)sel.SelectionRect.Y);
            }

            if (sel.Mode != SelectionMode.None && sel.SelectionRect.Width > 0)
            {
                var strokeStyle = new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash };
                ds.DrawRectangle(sel.SelectionRect, Colors.DodgerBlue, 1f / _zoomFactor, strokeStyle);
            }
        }

        // Draw free-form selection lasso
        if (_currentToolType == DrawingTool.FreeFormSelection)
        {
            var freesel = (FreeFormSelectionTool)_tools[DrawingTool.FreeFormSelection];
            freesel.DrawLasso(ds, Colors.DodgerBlue, _zoomFactor);
        }

        // Draw grid overlay
        if (_settings.ShowGrid)
            DrawGrid(ds);

        ds.Transform = Matrix3x2.Identity;
    }

    private void DrawCheckerboard(CanvasDrawingSession ds, float w, float h)
    {
        var sz = 8f;
        var c1 = Color.FromArgb(255, 204, 204, 204);
        var c2 = Color.FromArgb(255, 255, 255, 255);
        for (float x = 0; x < w; x += sz)
            for (float y = 0; y < h; y += sz)
                ds.FillRectangle(x, y, sz, sz, ((int)(x / sz) + (int)(y / sz)) % 2 == 0 ? c1 : c2);
    }

    private void DrawPreview(CanvasDrawingSession ds)
    {
        var strokeWidth = (float)StrokeSizeSlider.Value;
        if (_currentToolType == DrawingTool.Line)
        {
            var style = new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash };
            ds.DrawLine(_previewStart, _previewEnd, _activeDrawColor, strokeWidth, style);
        }
        else if (_currentToolType == DrawingTool.Curve && _tools[DrawingTool.Curve] is CurveTool curveTool)
        {
            curveTool.DrawPreview(ds, _activeDrawColor, strokeWidth);
        }
        else if (_currentToolType == DrawingTool.Shape && _tools[DrawingTool.Shape] is ShapeTool shapeTool)
        {
            shapeTool.DrawShape(ds, _previewStart, _previewEnd, _activeDrawColor, strokeWidth);
        }
    }

    private void DrawGrid(CanvasDrawingSession ds)
    {
        var gridColor = Color.FromArgb(60, 128, 128, 128);
        for (float x = 0; x < _settings.Width; x += _settings.GridSpacing)
            ds.DrawLine(x, 0, x, _settings.Height, gridColor, 0.5f / _zoomFactor);
        for (float y = 0; y < _settings.Height; y += _settings.GridSpacing)
            ds.DrawLine(0, y, _settings.Width, y, gridColor, 0.5f / _zoomFactor);
    }

    private void RulerCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        // Ruler drawing handled in overlay
        if (!_settings.ShowRuler) return;
        var ds = args.DrawingSession;
        var rulerColor = Color.FromArgb(200, 80, 80, 80);
        var bg = Color.FromArgb(180, 240, 240, 240);
        var rulerH = 20f;

        // Horizontal ruler
        ds.FillRectangle(0, 0, (float)sender.ActualWidth, rulerH, bg);
        for (float x = 0; x < _settings.Width * _zoomFactor; x += 50 * _zoomFactor)
        {
            var label = $"{(int)(x / _zoomFactor)}";
            ds.DrawLine(x, 0, x, rulerH, rulerColor, 0.5f);
            ds.DrawText(label, x + 2, 2, rulerColor, new CanvasTextFormat { FontSize = 9 });
        }

        // Vertical ruler
        ds.FillRectangle(0, 0, rulerH, (float)sender.ActualHeight, bg);
        for (float y = 0; y < _settings.Height * _zoomFactor; y += 50 * _zoomFactor)
        {
            var label = $"{(int)(y / _zoomFactor)}";
            ds.DrawLine(0, y, rulerH, y, rulerColor, 0.5f);
            ds.DrawText(label, 2, y + 2, rulerColor, new CanvasTextFormat { FontSize = 9 });
        }
    }

    #endregion

    #region Pointer Events

    private void DrawingCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (ActiveLayer == null || ActiveLayer.IsLocked || ActiveLayer.Bitmap == null) return;

        var pos = GetCanvasPoint(e);
        _isPointerDown = true;
        DrawingCanvas.CapturePointer(e.Pointer);
        var strokeWidth = (float)StrokeSizeSlider.Value;

        // Right-click uses secondary color, left-click uses primary
        var pointerPoint = e.GetCurrentPoint(DrawingCanvas);
        _activeDrawColor = pointerPoint.Properties.IsRightButtonPressed ? _secondaryColor : _primaryColor;

        // Eyedropper: right-click picks into secondary color
        if (_currentToolType == DrawingTool.Eyedropper)
        {
            PickColor(pos, pointerPoint.Properties.IsRightButtonPressed);
            return;
        }

        // Text tool manages its own undo in InsertTextAsync
        if (_currentToolType == DrawingTool.Text)
        {
            var textTool = (TextTool)_tools[DrawingTool.Text];
            textTool.OnPointerPressed(null!, pos, _activeDrawColor, strokeWidth);
            _ = InsertTextAsync(pos);
            return;
        }

        // Start undo capture for tools that modify pixels
        _currentUndoAction = new BitmapUndoAction(ActiveLayer, DrawingCanvas, CurrentTool.Name);
        _currentUndoAction.CaptureBeforeState();

        if (_currentToolType == DrawingTool.Fill)
        {
            FloodFill.Execute(ActiveLayer.Bitmap, (int)pos.X, (int)pos.Y, _activeDrawColor);
            FinalizeStroke();
            return;
        }

        if (_currentToolType == DrawingTool.Line || _currentToolType == DrawingTool.Shape
            || _currentToolType == DrawingTool.Curve)
        {
            _previewStart = pos;
            _previewEnd = pos;
            _isPreviewing = true;
            using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
            CurrentTool.OnPointerPressed(ds, pos, _activeDrawColor, strokeWidth);
            return;
        }

        if (_currentToolType == DrawingTool.Selection)
        {
            var sel = (SelectionTool)_tools[DrawingTool.Selection];

            // When clicking inside an existing selection to move it, lift the pixels first
            if (sel.Mode != SelectionMode.None && sel.SelectionRect.Contains(new Point(pos.X, pos.Y)))
            {
                if (!sel.HasFloatingSelection)
                {
                    sel.LiftPixels(ActiveLayer.Bitmap, _settings.Dpi);
                }
            }
            else if (sel.HasFloatingSelection)
            {
                // Clicking outside the selection commits the floating pixels
                using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
                sel.CommitFloatingSelection(ds);
            }

            using (var ds2 = ActiveLayer.Bitmap.CreateDrawingSession())
            {
                CurrentTool.OnPointerPressed(ds2, pos, _activeDrawColor, strokeWidth);
            }
            DrawingCanvas.Invalidate();
            return;
        }

        if (_currentToolType == DrawingTool.FreeFormSelection)
        {
            using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
            CurrentTool.OnPointerPressed(ds, pos, _activeDrawColor, strokeWidth);
            DrawingCanvas.Invalidate();
            return;
        }

        using (var ds = ActiveLayer.Bitmap.CreateDrawingSession())
        {
            CurrentTool.OnPointerPressed(ds, pos, _activeDrawColor, strokeWidth);
        }
        _fileService.HasUnsavedChanges = true;
        DrawingCanvas.Invalidate();
    }

    private void DrawingCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pos = GetCanvasPoint(e);
        StatusPosition.Text = $"{(int)pos.X}, {(int)pos.Y} px";
        UpdateSelectionStatus();

        if (!_isPointerDown || ActiveLayer?.Bitmap == null) return;
        var strokeWidth = (float)StrokeSizeSlider.Value;

        if (_currentToolType == DrawingTool.Line || _currentToolType == DrawingTool.Shape
            || _currentToolType == DrawingTool.Curve)
        {
            _previewEnd = pos;
            using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
            CurrentTool.OnPointerMoved(ds, pos, _activeDrawColor, strokeWidth);
            DrawingCanvas.Invalidate();
            return;
        }

        if (_currentToolType == DrawingTool.Selection)
        {
            using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
            CurrentTool.OnPointerMoved(ds, pos, _activeDrawColor, strokeWidth);
            DrawingCanvas.Invalidate();
            return;
        }

        if (_currentToolType == DrawingTool.FreeFormSelection)
        {
            using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
            CurrentTool.OnPointerMoved(ds, pos, _activeDrawColor, strokeWidth);
            DrawingCanvas.Invalidate();
            return;
        }

        using (var ds = ActiveLayer.Bitmap.CreateDrawingSession())
        {
            CurrentTool.OnPointerMoved(ds, pos, _activeDrawColor, strokeWidth);
        }
        DrawingCanvas.Invalidate();
    }

    private void DrawingCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isPointerDown) return;
        _isPointerDown = false;
        DrawingCanvas.ReleasePointerCapture(e.Pointer);

        if (ActiveLayer?.Bitmap == null) return;
        var pos = GetCanvasPoint(e);
        var strokeWidth = (float)StrokeSizeSlider.Value;

        if (_currentToolType == DrawingTool.Line || _currentToolType == DrawingTool.Shape
            || _currentToolType == DrawingTool.Curve)
        {
            _isPreviewing = false;
            using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
            CurrentTool.OnPointerReleased(ds, pos, _activeDrawColor, strokeWidth);
        }
        else
        {
            using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
            CurrentTool.OnPointerReleased(ds, pos, _activeDrawColor, strokeWidth);
        }

        FinalizeStroke();
    }

    private Vector2 GetCanvasPoint(PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(DrawingCanvas).Position;
        return new Vector2((float)(point.X / _zoomFactor), (float)(point.Y / _zoomFactor));
    }

    private void FinalizeStroke()
    {
        if (_currentUndoAction != null)
        {
            _currentUndoAction.CaptureAfterState();
            _undoManager.Push(_currentUndoAction);
            _currentUndoAction = null;
        }
        _fileService.HasUnsavedChanges = true;
        DrawingCanvas.Invalidate();
    }

    #endregion

    #region Tool Selection

    private void Tool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is AppBarButton btn && btn.Tag is string tag)
        {
            if (Enum.TryParse<DrawingTool>(tag, out var tool))
            {
                _currentToolType = tool;
                StatusTool.Text = CurrentTool.Name;
                HighlightActiveTool();
                SyncShapeToolOptions();
            }
        }
    }

    private void ShapeTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SyncShapeToolOptions();
    }

    private void FillModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SyncShapeToolOptions();
    }

    private void SyncShapeToolOptions()
    {
        if (_tools.TryGetValue(DrawingTool.Shape, out var tool) && tool is ShapeTool shapeTool)
        {
            shapeTool.CurrentShapeType = (ShapeType)ShapeTypeCombo.SelectedIndex;
            shapeTool.FillMode = (ShapeFillMode)FillModeCombo.SelectedIndex;
            shapeTool.SecondaryColor = _secondaryColor;
        }
    }

    private void HighlightActiveTool()
    {
        var toolName = _currentToolType.ToString();
        var buttons = new[] { BtnPencil, BtnBrush, BtnEraser, BtnFill, BtnText, BtnEyedropper, BtnLine, BtnCurve, BtnShape, BtnSelect, BtnFreeSelect };
        foreach (var btn in buttons)
        {
            btn.BorderBrush = btn.Tag?.ToString() == toolName
                ? new SolidColorBrush(Colors.DodgerBlue)
                : new SolidColorBrush(Colors.Transparent);
            btn.BorderThickness = btn.Tag?.ToString() == toolName
                ? new Thickness(0, 0, 0, 2)
                : new Thickness(0);
        }
    }

    #endregion

    #region Color

    private void ColorPalette_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SolidColorBrush brush)
        {
            _primaryColor = brush.Color;
            PrimaryColorBorder.Background = new SolidColorBrush(_primaryColor);
        }
    }

    private async void PrimaryColor_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var color = await ShowColorPickerDialogAsync(_primaryColor);
        if (color.HasValue)
        {
            _primaryColor = color.Value;
            PrimaryColorBorder.Background = new SolidColorBrush(_primaryColor);
        }
    }

    private async void SecondaryColor_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var color = await ShowColorPickerDialogAsync(_secondaryColor);
        if (color.HasValue)
        {
            _secondaryColor = color.Value;
            SecondaryColorBorder.Background = new SolidColorBrush(_secondaryColor);
        }
    }

    private async Task<Color?> ShowColorPickerDialogAsync(Color initialColor)
    {
        var picker = new ColorPicker
        {
            Color = initialColor,
            IsAlphaEnabled = true,
            IsColorSpectrumVisible = true,
            IsColorPreviewVisible = true,
            IsHexInputVisible = true
        };
        var dialog = new ContentDialog
        {
            Title = "Choose Color",
            Content = picker,
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? picker.Color : null;
    }

    private void PickColor(Vector2 pos, bool toSecondary = false)
    {
        if (ActiveLayer?.Bitmap == null) return;
        int x = (int)pos.X, y = (int)pos.Y;
        var w = (int)ActiveLayer.Bitmap.SizeInPixels.Width;
        var h = (int)ActiveLayer.Bitmap.SizeInPixels.Height;
        if (x < 0 || x >= w || y < 0 || y >= h) return;
        var pixels = ActiveLayer.Bitmap.GetPixelColors();
        var color = pixels[y * w + x];
        if (toSecondary)
        {
            _secondaryColor = color;
            SecondaryColorBorder.Background = new SolidColorBrush(_secondaryColor);
        }
        else
        {
            _primaryColor = color;
            PrimaryColorBorder.Background = new SolidColorBrush(_primaryColor);
        }
    }

    #endregion

    #region File Operations

    private async void New_Click(object sender, RoutedEventArgs e)
    {
        if (_fileService.HasUnsavedChanges && !await ConfirmDiscardAsync()) return;
        foreach (var layer in _layers) layer.Dispose();
        _layers.Clear();
        _undoManager.Clear();
        _settings = new CanvasSettings();
        _fileService.CurrentFilePath = null;
        _fileService.HasUnsavedChanges = false;
        InitializeCanvas(DrawingCanvas);
        DrawingCanvas.Invalidate();
        Title = "SmrtDoodle";
    }

    private async void Open_Click(object sender, RoutedEventArgs e)
    {
        if (_fileService.HasUnsavedChanges && !await ConfirmDiscardAsync()) return;
        var file = await _fileService.ShowOpenDialogAsync();
        if (file == null) return;

        var bitmap = await _fileService.LoadImageAsync(DrawingCanvas, file);
        if (bitmap == null) return;

        foreach (var l in _layers) l.Dispose();
        _layers.Clear();
        _undoManager.Clear();

        _settings.Width = (int)bitmap.SizeInPixels.Width;
        _settings.Height = (int)bitmap.SizeInPixels.Height;

        var newLayer = new Layer("Background");
        newLayer.Initialize(DrawingCanvas, _settings.Width, _settings.Height, _settings.Dpi);
        using (var ds = newLayer.Bitmap!.CreateDrawingSession())
            ds.DrawImage(bitmap);
        bitmap.Dispose();
        _layers.Add(newLayer);
        _activeLayerIndex = 0;

        _fileService.CurrentFilePath = file.Path;
        _fileService.HasUnsavedChanges = false;
        Title = $"SmrtDoodle - {file.Name}";
        RefreshLayerList();
        UpdateCanvasSize();
        DrawingCanvas.Invalidate();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_fileService.CurrentFilePath != null)
        {
            var composite = FileService.ComposeLayers(DrawingCanvas, _layers, _settings.Width, _settings.Height, _settings.Dpi, _settings.BackgroundColor);
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(_fileService.CurrentFilePath);
                await _fileService.SaveImageAsync(composite, file);
            }
            finally { composite.Dispose(); }
        }
        else
        {
            SaveAs_Click(sender, e);
        }
    }

    private async void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var file = await _fileService.ShowSaveDialogAsync();
        if (file == null) return;
        var composite = FileService.ComposeLayers(DrawingCanvas, _layers, _settings.Width, _settings.Height, _settings.Dpi, _settings.BackgroundColor);
        try
        {
            await _fileService.SaveImageAsync(composite, file);
            Title = $"SmrtDoodle - {file.Name}";
        }
        finally { composite.Dispose(); }
    }

    private async void Print_Click(object sender, RoutedEventArgs e)
    {
        // WinUI 3 lacks direct PrintManager support — save to temp file and launch OS print dialog
        var composite = FileService.ComposeLayers(DrawingCanvas, _layers, _settings.Width, _settings.Height, _settings.Dpi, _settings.BackgroundColor);
        try
        {
            var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var tempFile = await tempFolder.CreateFileAsync("SmrtDoodle_Print.png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            using var stream = await tempFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            await composite.SaveAsync(stream, CanvasBitmapFileFormat.Png);
            await Windows.System.Launcher.LaunchFileAsync(tempFile, new Windows.System.LauncherOptions { DisplayApplicationPicker = false });
        }
        finally { composite.Dispose(); }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private async void InsertIntoDocument_Click(object sender, RoutedEventArgs e)
    {
        var tempPath = _ipcService.GetOrCreateTempFilePath();
        var composite = FileService.ComposeLayers(DrawingCanvas, _layers, _settings.Width, _settings.Height, _settings.Dpi, _settings.BackgroundColor);
        try
        {
            await _fileService.SaveToTempFileAsync(composite, tempPath);
            Close();
        }
        finally { composite.Dispose(); }
    }

    private async Task<bool> ConfirmDiscardAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Unsaved Changes",
            Content = "You have unsaved changes. Do you want to discard them?",
            PrimaryButtonText = "Discard",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    #endregion

    #region Edit Operations

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        _undoManager.Undo();
        DrawingCanvas.Invalidate();
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        _undoManager.Redo();
        DrawingCanvas.Invalidate();
    }

    private async void Cut_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveLayer?.Bitmap == null) return;
        var sel = _tools[DrawingTool.Selection] as SelectionTool;
        if (sel?.Mode == SelectionMode.None) return;
        await CopySelectionToClipboardAsync(sel!);
        ClearSelectionArea(sel!);
        DrawingCanvas.Invalidate();
    }

    private async void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveLayer?.Bitmap == null) return;
        var sel = _tools[DrawingTool.Selection] as SelectionTool;
        if (sel?.Mode == SelectionMode.None)
        {
            var composite = FileService.ComposeLayers(DrawingCanvas, _layers, _settings.Width, _settings.Height, _settings.Dpi, _settings.BackgroundColor);
            await _clipboardService.CopyToClipboardAsync(composite);
            composite.Dispose();
        }
        else
        {
            await CopySelectionToClipboardAsync(sel!);
        }
    }

    private async void Paste_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = await _clipboardService.PasteFromClipboard(DrawingCanvas);
        if (bitmap == null || ActiveLayer?.Bitmap == null) return;

        var action = new BitmapUndoAction(ActiveLayer, DrawingCanvas, "Paste");
        action.CaptureBeforeState();
        using (var ds = ActiveLayer.Bitmap.CreateDrawingSession())
            ds.DrawImage(bitmap);
        action.CaptureAfterState();
        _undoManager.Push(action);
        bitmap.Dispose();
        _fileService.HasUnsavedChanges = true;
        DrawingCanvas.Invalidate();
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        _currentToolType = DrawingTool.Selection;
        HighlightActiveTool();
        var sel = (SelectionTool)_tools[DrawingTool.Selection];
        sel.Reset();
        sel.Mode = SelectionMode.Selecting;
        sel.SelectionRect = new Rect(0, 0, _settings.Width, _settings.Height);
        DrawingCanvas.Invalidate();
    }

    private void ClearSelection_Click(object sender, RoutedEventArgs e)
    {
        var sel = (SelectionTool)_tools[DrawingTool.Selection];
        if (sel.HasFloatingSelection && ActiveLayer?.Bitmap != null)
        {
            using var ds = ActiveLayer.Bitmap.CreateDrawingSession();
            sel.CommitFloatingSelection(ds);
            _fileService.HasUnsavedChanges = true;
        }
        sel.Reset();
        DrawingCanvas.Invalidate();
    }

    private async Task CopySelectionToClipboardAsync(SelectionTool sel)
    {
        if (ActiveLayer?.Bitmap == null || sel.SelectionRect.Width < 1) return;
        var cropped = ImageTransforms.Crop(DrawingCanvas, ActiveLayer.Bitmap, sel.SelectionRect, _settings.Dpi);
        await _clipboardService.CopyToClipboardAsync(cropped);
        cropped.Dispose();
    }

    private void ClearSelectionArea(SelectionTool sel)
    {
        if (ActiveLayer?.Bitmap == null) return;
        var action = new BitmapUndoAction(ActiveLayer, DrawingCanvas, "Cut");
        action.CaptureBeforeState();
        using (var ds = ActiveLayer.Bitmap.CreateDrawingSession())
        {
            ds.Blend = CanvasBlend.Copy;
            ds.FillRectangle(sel.SelectionRect, Colors.Transparent);
            ds.Blend = CanvasBlend.SourceOver;
        }
        action.CaptureAfterState();
        _undoManager.Push(action);
        _fileService.HasUnsavedChanges = true;
    }

    private void DeleteSelection_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveLayer?.Bitmap == null) return;
        var sel = _tools[DrawingTool.Selection] as SelectionTool;
        if (sel == null || sel.Mode == SelectionMode.None) return;

        // If there's a floating selection, just discard it (delete the lifted pixels)
        if (sel.HasFloatingSelection)
        {
            sel.Reset();
        }
        else
        {
            ClearSelectionArea(sel);
            sel.Reset();
        }
        DrawingCanvas.Invalidate();
    }

    private async void PasteAsNewImage_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = await _clipboardService.PasteFromClipboard(DrawingCanvas);
        if (bitmap == null) return;

        if (_fileService.HasUnsavedChanges && !await ConfirmDiscardAsync())
        {
            bitmap.Dispose();
            return;
        }

        foreach (var l in _layers) l.Dispose();
        _layers.Clear();
        _undoManager.Clear();

        _settings.Width = (int)bitmap.SizeInPixels.Width;
        _settings.Height = (int)bitmap.SizeInPixels.Height;

        var newLayer = new Layer("Background");
        newLayer.Initialize(DrawingCanvas, _settings.Width, _settings.Height, _settings.Dpi);
        using (var ds = newLayer.Bitmap!.CreateDrawingSession())
            ds.DrawImage(bitmap);
        bitmap.Dispose();
        _layers.Add(newLayer);
        _activeLayerIndex = 0;

        _fileService.CurrentFilePath = null;
        _fileService.HasUnsavedChanges = false;
        Title = "SmrtDoodle";
        RefreshLayerList();
        UpdateCanvasSize();
        DrawingCanvas.Invalidate();
    }

    private void UpdateUndoRedoState()
    {
        UndoMenuItem.IsEnabled = _undoManager.CanUndo;
        RedoMenuItem.IsEnabled = _undoManager.CanRedo;
    }

    #endregion

    #region Image Transforms

    private async void Resize_Click(object sender, RoutedEventArgs e)
    {
        var aspectRatio = (double)_settings.Width / _settings.Height;
        var maintainAspect = new CheckBox { Content = "Maintain aspect ratio", IsChecked = true };
        var percentageMode = new CheckBox { Content = "Resize by percentage" };

        var widthBox = new NumberBox { Header = "Width (px)", Value = _settings.Width, Minimum = 1, Maximum = 10000, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var heightBox = new NumberBox { Header = "Height (px)", Value = _settings.Height, Minimum = 1, Maximum = 10000, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var percentBox = new NumberBox { Header = "Scale (%)", Value = 100, Minimum = 1, Maximum = 1000, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact, Visibility = Visibility.Collapsed };

        var skewHBox = new NumberBox { Header = "Horizontal Skew (°)", Value = 0, Minimum = -89, Maximum = 89, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var skewVBox = new NumberBox { Header = "Vertical Skew (°)", Value = 0, Minimum = -89, Maximum = 89, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };

        bool updatingAspect = false;
        widthBox.ValueChanged += (_, args) =>
        {
            if (updatingAspect || maintainAspect.IsChecked != true) return;
            updatingAspect = true;
            heightBox.Value = Math.Max(1, Math.Round(args.NewValue / aspectRatio));
            updatingAspect = false;
        };
        heightBox.ValueChanged += (_, args) =>
        {
            if (updatingAspect || maintainAspect.IsChecked != true) return;
            updatingAspect = true;
            widthBox.Value = Math.Max(1, Math.Round(args.NewValue * aspectRatio));
            updatingAspect = false;
        };
        percentageMode.Click += (_, _) =>
        {
            var isPct = percentageMode.IsChecked == true;
            percentBox.Visibility = isPct ? Visibility.Visible : Visibility.Collapsed;
            widthBox.Visibility = isPct ? Visibility.Collapsed : Visibility.Visible;
            heightBox.Visibility = isPct ? Visibility.Collapsed : Visibility.Visible;
            maintainAspect.Visibility = isPct ? Visibility.Collapsed : Visibility.Visible;
        };

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(maintainAspect);
        panel.Children.Add(percentageMode);
        panel.Children.Add(widthBox);
        panel.Children.Add(heightBox);
        panel.Children.Add(percentBox);
        panel.Children.Add(new TextBlock { Text = "Skew", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) });
        panel.Children.Add(skewHBox);
        panel.Children.Add(skewVBox);

        var dialog = new ContentDialog
        {
            Title = "Resize / Skew",
            Content = panel,
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

        int newW, newH;
        if (percentageMode.IsChecked == true)
        {
            var scale = percentBox.Value / 100.0;
            newW = Math.Max(1, (int)(_settings.Width * scale));
            newH = Math.Max(1, (int)(_settings.Height * scale));
        }
        else
        {
            newW = (int)widthBox.Value;
            newH = (int)heightBox.Value;
        }

        var skewH = (float)(skewHBox.Value * Math.PI / 180.0);
        var skewV = (float)(skewVBox.Value * Math.PI / 180.0);
        var hasSkew = Math.Abs(skewH) > 0.001f || Math.Abs(skewV) > 0.001f;

        foreach (var layer in _layers)
        {
            if (layer.Bitmap == null) continue;
            var resized = ImageTransforms.Resize(DrawingCanvas, layer.Bitmap, newW, newH, _settings.Dpi);
            layer.Bitmap.Dispose();
            if (hasSkew)
            {
                var skewed = ImageTransforms.Skew(DrawingCanvas, resized, skewH, skewV, _settings.Dpi);
                resized.Dispose();
                layer.Bitmap = skewed;
            }
            else
            {
                layer.Bitmap = resized;
            }
        }

        if (hasSkew)
        {
            // Skew may change effective dimensions
            var firstBitmap = _layers.FirstOrDefault(l => l.Bitmap != null)?.Bitmap;
            if (firstBitmap != null)
            {
                newW = (int)firstBitmap.SizeInPixels.Width;
                newH = (int)firstBitmap.SizeInPixels.Height;
            }
        }

        _settings.Width = newW;
        _settings.Height = newH;
        _undoManager.Clear();
        _fileService.HasUnsavedChanges = true;
        UpdateCanvasSize();
        DrawingCanvas.Invalidate();
    }

    private void Crop_Click(object sender, RoutedEventArgs e)
    {
        var sel = (SelectionTool)_tools[DrawingTool.Selection];
        if (sel.Mode == SelectionMode.None || sel.SelectionRect.Width < 1) return;
        var rect = sel.SelectionRect;
        foreach (var layer in _layers)
        {
            if (layer.Bitmap == null) continue;
            var cropped = ImageTransforms.Crop(DrawingCanvas, layer.Bitmap, rect, _settings.Dpi);
            layer.Bitmap.Dispose();
            layer.Bitmap = cropped;
        }
        _settings.Width = (int)rect.Width;
        _settings.Height = (int)rect.Height;
        sel.Reset();
        _undoManager.Clear();
        _fileService.HasUnsavedChanges = true;
        UpdateCanvasSize();
        DrawingCanvas.Invalidate();
    }

    private void FlipH_Click(object sender, RoutedEventArgs e) => ApplyTransform(ImageTransforms.FlipHorizontal);
    private void FlipV_Click(object sender, RoutedEventArgs e) => ApplyTransform(ImageTransforms.FlipVertical);
    private void Rotate90_Click(object sender, RoutedEventArgs e) => ApplyRotation(ImageTransforms.Rotate90, true);
    private void Rotate180_Click(object sender, RoutedEventArgs e) => ApplyRotation(ImageTransforms.Rotate180, false);
    private void Rotate270_Click(object sender, RoutedEventArgs e) => ApplyRotation(ImageTransforms.Rotate270, true);

    private void InvertColors_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveLayer?.Bitmap == null) return;
        var action = new BitmapUndoAction(ActiveLayer, DrawingCanvas, "Invert Colors");
        action.CaptureBeforeState();
        ImageTransforms.InvertColors(ActiveLayer.Bitmap);
        action.CaptureAfterState();
        _undoManager.Push(action);
        _fileService.HasUnsavedChanges = true;
        DrawingCanvas.Invalidate();
    }

    private void ApplyTransform(Func<ICanvasResourceCreator, CanvasRenderTarget, CanvasRenderTarget> transform)
    {
        foreach (var layer in _layers)
        {
            if (layer.Bitmap == null) continue;
            var result = transform(DrawingCanvas, layer.Bitmap);
            layer.Bitmap.Dispose();
            layer.Bitmap = result;
        }
        _undoManager.Clear();
        _fileService.HasUnsavedChanges = true;
        DrawingCanvas.Invalidate();
    }

    private void ApplyRotation(Func<ICanvasResourceCreator, CanvasRenderTarget, CanvasRenderTarget> transform, bool swapDimensions)
    {
        ApplyTransform(transform);
        if (swapDimensions)
        {
            (_settings.Width, _settings.Height) = (_settings.Height, _settings.Width);
            UpdateCanvasSize();
        }
    }

    private async void CanvasProperties_Click(object sender, RoutedEventArgs e)
    {
        var widthBox = new NumberBox { Header = "Width", Value = _settings.Width, Minimum = 1, Maximum = 10000, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var heightBox = new NumberBox { Header = "Height", Value = _settings.Height, Minimum = 1, Maximum = 10000, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var dpiBox = new NumberBox { Header = "DPI", Value = _settings.Dpi, Minimum = 1, Maximum = 1200, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(widthBox);
        panel.Children.Add(heightBox);
        panel.Children.Add(dpiBox);
        var dialog = new ContentDialog
        {
            Title = "Canvas Properties",
            Content = panel,
            PrimaryButtonText = "Apply",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            _settings.Width = (int)widthBox.Value;
            _settings.Height = (int)heightBox.Value;
            _settings.Dpi = (float)dpiBox.Value;
            UpdateCanvasSize();
            DrawingCanvas.Invalidate();
        }
    }

    #endregion

    #region View

    private void ToggleGrid_Click(object sender, RoutedEventArgs e)
    {
        _settings.ShowGrid = ShowGridToggle.IsChecked;
        DrawingCanvas.Invalidate();
    }

    private void ToggleRuler_Click(object sender, RoutedEventArgs e)
    {
        _settings.ShowRuler = ShowRulerToggle.IsChecked;
        RulerCanvas.Visibility = _settings.ShowRuler ? Visibility.Visible : Visibility.Collapsed;
        RulerCanvas.Invalidate();
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e) => SetZoom(_zoomFactor * 1.25f);
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => SetZoom(_zoomFactor / 1.25f);
    private void Zoom100_Click(object sender, RoutedEventArgs e) => SetZoom(1f);
    private void ZoomFit_Click(object sender, RoutedEventArgs e)
    {
        var viewW = (float)CanvasScrollViewer.ActualWidth - 40;
        var viewH = (float)CanvasScrollViewer.ActualHeight - 40;
        if (viewW <= 0 || viewH <= 0) return;
        var scale = Math.Min(viewW / _settings.Width, viewH / _settings.Height);
        SetZoom(scale);
    }

    private void ZoomSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!_resourcesCreated) return;
        SetZoom((float)e.NewValue / 100f, updateSlider: false);
    }

    private void SetZoom(float zoom, bool updateSlider = true)
    {
        _zoomFactor = Math.Clamp(zoom, 0.1f, 8f);
        if (updateSlider) ZoomSlider.Value = _zoomFactor * 100;
        StatusZoom.Text = $"{_zoomFactor * 100:0}%";
        UpdateCanvasSize();
        DrawingCanvas.Invalidate();
        RulerCanvas.Invalidate();
    }

    #endregion

    #region Layers

    private void AddLayer_Click(object sender, RoutedEventArgs e)
    {
        if (!_resourcesCreated) return;
        var layer = new Layer($"Layer {_layers.Count + 1}");
        layer.Initialize(DrawingCanvas, _settings.Width, _settings.Height, _settings.Dpi);
        _layers.Add(layer);
        _activeLayerIndex = _layers.Count - 1;
        RefreshLayerList();
    }

    private void DeleteLayer_Click(object sender, RoutedEventArgs e)
    {
        if (_layers.Count <= 1 || _activeLayerIndex < 0) return;
        _layers[_activeLayerIndex].Dispose();
        _layers.RemoveAt(_activeLayerIndex);
        _activeLayerIndex = Math.Min(_activeLayerIndex, _layers.Count - 1);
        RefreshLayerList();
        DrawingCanvas.Invalidate();
    }

    private void DuplicateLayer_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveLayer == null) return;
        var clone = ActiveLayer.Clone(DrawingCanvas);
        _layers.Insert(_activeLayerIndex + 1, clone);
        _activeLayerIndex++;
        RefreshLayerList();
        DrawingCanvas.Invalidate();
    }

    private void MoveLayerUp_Click(object sender, RoutedEventArgs e)
    {
        if (_activeLayerIndex >= _layers.Count - 1) return;
        (_layers[_activeLayerIndex], _layers[_activeLayerIndex + 1]) = (_layers[_activeLayerIndex + 1], _layers[_activeLayerIndex]);
        _activeLayerIndex++;
        RefreshLayerList();
        DrawingCanvas.Invalidate();
    }

    private void MoveLayerDown_Click(object sender, RoutedEventArgs e)
    {
        if (_activeLayerIndex <= 0) return;
        (_layers[_activeLayerIndex], _layers[_activeLayerIndex - 1]) = (_layers[_activeLayerIndex - 1], _layers[_activeLayerIndex]);
        _activeLayerIndex--;
        RefreshLayerList();
        DrawingCanvas.Invalidate();
    }

    private void MergeDown_Click(object sender, RoutedEventArgs e)
    {
        if (_activeLayerIndex <= 0 || ActiveLayer?.Bitmap == null) return;
        var below = _layers[_activeLayerIndex - 1];
        if (below.Bitmap == null) return;
        using (var ds = below.Bitmap.CreateDrawingSession())
            ds.DrawImage(ActiveLayer.Bitmap, 0, 0, new Rect(0, 0, _settings.Width, _settings.Height), ActiveLayer.Opacity);
        ActiveLayer.Dispose();
        _layers.RemoveAt(_activeLayerIndex);
        _activeLayerIndex--;
        RefreshLayerList();
        DrawingCanvas.Invalidate();
    }

    private void FlattenImage_Click(object sender, RoutedEventArgs e)
    {
        if (_layers.Count <= 1) return;
        var composite = FileService.ComposeLayers(DrawingCanvas, _layers, _settings.Width, _settings.Height, _settings.Dpi, _settings.BackgroundColor);
        foreach (var layer in _layers) layer.Dispose();
        _layers.Clear();
        var flat = new Layer("Background") { Bitmap = composite };
        _layers.Add(flat);
        _activeLayerIndex = 0;
        RefreshLayerList();
        DrawingCanvas.Invalidate();
    }

    private void LayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LayerListView.SelectedIndex >= 0)
            _activeLayerIndex = LayerListView.SelectedIndex;
    }

    private void LayerVisibility_Click(object sender, RoutedEventArgs e)
    {
        DrawingCanvas.Invalidate();
    }

    private void RefreshLayerList()
    {
        LayerListView.ItemsSource = null;
        LayerListView.ItemsSource = _layers;
        if (_activeLayerIndex >= 0 && _activeLayerIndex < _layers.Count)
            LayerListView.SelectedIndex = _activeLayerIndex;
    }

    #endregion

    #region Text Tool

    private async Task InsertTextAsync(Vector2 position)
    {
        var textBox = new TextBox { PlaceholderText = "Enter text...", AcceptsReturn = true, Height = 100, TextWrapping = TextWrapping.Wrap };
        var fontFamilyCombo = new ComboBox { Header = "Font", Width = 200, VerticalAlignment = VerticalAlignment.Center };
        var commonFonts = new[]
        {
            "Segoe UI", "Arial", "Calibri", "Cambria", "Consolas", "Courier New",
            "Georgia", "Impact", "Lucida Console", "Tahoma", "Times New Roman",
            "Trebuchet MS", "Verdana"
        };
        foreach (var f in commonFonts) fontFamilyCombo.Items.Add(f);
        fontFamilyCombo.SelectedIndex = 0;
        var fontSizeBox = new NumberBox { Header = "Font Size", Value = 20, Minimum = 6, Maximum = 200, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        var boldCheck = new CheckBox { Content = "Bold" };
        var italicCheck = new CheckBox { Content = "Italic" };
        var underlineCheck = new CheckBox { Content = "Underline" };
        var strikeCheck = new CheckBox { Content = "Strikethrough" };
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(textBox);
        panel.Children.Add(fontFamilyCombo);
        panel.Children.Add(fontSizeBox);
        var stylePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        stylePanel.Children.Add(boldCheck);
        stylePanel.Children.Add(italicCheck);
        stylePanel.Children.Add(underlineCheck);
        stylePanel.Children.Add(strikeCheck);
        panel.Children.Add(stylePanel);

        var dialog = new ContentDialog
        {
            Title = "Insert Text",
            Content = panel,
            PrimaryButtonText = "Insert",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary || string.IsNullOrEmpty(textBox.Text)) return;
        if (ActiveLayer?.Bitmap == null) return;

        var action = new BitmapUndoAction(ActiveLayer, DrawingCanvas, "Text");
        action.CaptureBeforeState();

        using (var ds = ActiveLayer.Bitmap.CreateDrawingSession())
        {
            var fontFamily = fontFamilyCombo.SelectedItem as string ?? "Segoe UI";
            var format = new CanvasTextFormat
            {
                FontFamily = fontFamily,
                FontSize = (float)fontSizeBox.Value,
                FontWeight = boldCheck.IsChecked == true ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal,
                FontStyle = italicCheck.IsChecked == true ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal,
                WordWrapping = CanvasWordWrapping.Wrap
            };

            if (underlineCheck.IsChecked == true || strikeCheck.IsChecked == true)
            {
                using var layout = new CanvasTextLayout(ds, textBox.Text, format, _settings.Width - position.X, _settings.Height - position.Y);
                if (underlineCheck.IsChecked == true)
                    layout.SetUnderline(0, textBox.Text.Length, true);
                if (strikeCheck.IsChecked == true)
                    layout.SetStrikethrough(0, textBox.Text.Length, true);
                ds.DrawTextLayout(layout, position, _activeDrawColor);
            }
            else
            {
                ds.DrawText(textBox.Text, position, _activeDrawColor, format);
            }
        }

        action.CaptureAfterState();
        _undoManager.Push(action);
        _fileService.HasUnsavedChanges = true;
        DrawingCanvas.Invalidate();
    }

    #endregion

    #region Helpers

    private void UpdateCanvasSize()
    {
        DrawingCanvas.Width = _settings.Width * _zoomFactor;
        DrawingCanvas.Height = _settings.Height * _zoomFactor;
        RulerCanvas.Width = _settings.Width * _zoomFactor;
        RulerCanvas.Height = _settings.Height * _zoomFactor;
        StatusCanvasSize.Text = $"{_settings.Width} x {_settings.Height} px";
    }

    private void UpdateStatusBar()
    {
        StatusCanvasSize.Text = $"{_settings.Width} x {_settings.Height} px";
        StatusZoom.Text = $"{_zoomFactor * 100:0}%";
        StatusTool.Text = CurrentTool.Name;
        UpdateSelectionStatus();
    }

    private void UpdateSelectionStatus()
    {
        if (_tools.TryGetValue(DrawingTool.Selection, out var tool) && tool is SelectionTool sel
            && sel.Mode != SelectionMode.None && sel.SelectionRect.Width > 0)
        {
            StatusSelection.Text = $"Selection: {(int)sel.SelectionRect.Width} x {(int)sel.SelectionRect.Height} px";
        }
        else
        {
            StatusSelection.Text = string.Empty;
        }
    }

    #endregion
}
