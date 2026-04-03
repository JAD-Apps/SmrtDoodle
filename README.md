# SmrtDoodle

A lightweight drawing and painting application built with **WinUI 3** and **Win2D** for Windows.

SmrtDoodle provides a familiar, intuitive canvas for quick sketches, diagrams, and image editing — with layer support, undo/redo, and a full set of drawing tools.

## Features

- **Ribbon Toolbar** — SmrtPad-style grouped ribbon with Tools, Brush, Shapes, Selection, and Colors groups; fits at default window size without overflow
- **Drawing Tools** — Pencil, Brush, Eraser, Line, Curve, and Shape tools with configurable stroke width and colors
- **Brush Styles** — Normal, Calligraphy, Airbrush, Oil, Crayon, Marker, Natural Pencil, and Watercolor
- **Shape Library** — Rectangle, Ellipse, Triangle, Star, Arrow, Heart, Lightning, and more with outline, fill, or combined modes
- **Layers** — Multiple layers with visibility toggle, opacity control, blend modes, and duplication
- **Selection** — Rectangular and free-form selection with move support and transparent selection mode
- **Flood Fill** — Tolerance-based fill tool for quick coloring
- **Text Tool** — Add text directly to the canvas
- **Eyedropper** — Pick colors from the canvas
- **Magnifier** — Left-click to zoom in, right-click to zoom out
- **Undo / Redo** — Full bitmap-level undo/redo history (up to 50 steps)
- **Clipboard** — Copy, cut, paste, paste from file, and paste as new image
- **File I/O** — Open and save images in PNG, JPEG, BMP, and GIF formats
- **Canvas Settings** — Configurable width, height, DPI, background color, grid, and ruler
- **Color Palette** — 28-color MS Paint-standard palette with primary/secondary color swatches and swap button
- **Zoom** — Zoom in and out with checkerboard transparency preview
- **SmrtPad Integration** — Launch from [SmrtPad](https://github.com/John-Donnelly/SmrtPad) to insert drawings directly into documents via IPC

## Requirements

- Windows 10 version 1809 (build 17763) or later
- .NET 8
- Windows App SDK 1.8+

## Building

Open `SmrtDoodle.slnx` in Visual Studio 2022 (17.12+) or Visual Studio 2026, then build and run:

```
dotnet build -p:Platform=x64
```

Or press **F5** in Visual Studio with the packaging project set as the startup project.

## Running Tests

```
dotnet test -p:Platform=x64
```

## Project Structure

```
SmrtDoodle/
├── Models/          # Data models (CanvasSettings, Layer, Enums, UndoRedoManager)
├── Services/        # App services (FileService, ClipboardService, IpcService)
├── Tools/           # Drawing tool implementations (ITool, BasicTools, AdvancedTools, CurveTool, FreeFormSelectionTool)
├── Helpers/         # Converters and image utilities (FloodFill)
├── MainWindow.xaml  # Main application window
└── App.xaml         # Application entry point

SmrtDoodle.Tests/
├── Models/          # Model and enum tests
├── Services/        # IPC service tests
├── Tools/           # Tool tests
└── Helpers/         # Converter tests

SmrtDoodle (Package)/ # MSIX packaging project
```

## License

This project is provided as-is. See the repository for details.
