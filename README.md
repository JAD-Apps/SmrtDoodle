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

Unit tests run locally:

```
dotnet test SmrtDoodle.Tests -p:Platform=x64
```

UI integration tests require a Windows remote test machine running [Appium](https://appium.io/) with the `appium-windows-driver` plugin. See **Remote UI Testing** below.

## Remote UI Testing

`SmrtDoodle.UITests` is an MSTest project that drives the app on a remote machine via WinRM + Appium.

### Prerequisites

- Remote machine running Windows 10/11 with .NET 8 and Windows App SDK 1.8 installed
- Appium 3.x + `appium-windows-driver` running on port 4723 on the remote machine
- WinRM enabled on the remote machine (`winrm quickconfig`)

### Setup

Create a `.env` file in the solution root (never committed — already in `.gitignore`):

```
UITEST_REMOTE_HOST=192.168.0.x
UITEST_REMOTE_WINRM_USERNAME=YourUser
UITEST_REMOTE_WINRM_PASSWORD=YourPassword
UITEST_DEPLOY_CONFIGURATION=Debug
```

### Deploy and Run

```powershell
# Deploy the Debug build to the remote machine
.\SmrtDoodle.UITests\Scripts\Deploy-Remote.ps1 -UseBuildOutput

# Run UI tests (tests connect to the already-deployed app)
dotnet test SmrtDoodle.UITests -p:Platform=x64
```

## Project Structure

```
SmrtDoodle/
├── Models/          # Data models (CanvasSettings, Layer, Enums, UndoRedoManager)
├── Services/        # App services (FileService, ClipboardService, IpcService)
├── Tools/           # Drawing tool implementations (ITool, BasicTools, AdvancedTools, CurveTool, FreeFormSelectionTool)
├── Helpers/         # Converters and image utilities (FloodFill)
├── Program.cs       # Custom WinUI 3 entry point (Bootstrap.Initialize / Shutdown)
├── MainWindow.xaml  # Main application window
└── App.xaml         # Application entry point

SmrtDoodle.UITests/
├── Scripts/         # Deploy-Remote.ps1 — builds and copies to remote test machine via PS Remoting
├── AppiumTestBase.cs         # Base class: Appium session management, element helpers, drag/shortcut utilities
├── BrushControlTests.cs      # Brush style, size, and rendering tests
├── CanvasInteractionTests.cs # Pencil and brush drawing on canvas
├── ColorControlTests.cs      # Color palette and swatch interaction
├── ContextMenuTests.cs       # Right-click context menus
├── LayerPanelTests.cs        # Layer visibility, opacity, and blend mode controls
├── MenuBarTests.cs           # File/Edit/View/Image/Help menus
├── SelectionAndClipboardTests.cs # Selection tools, copy, cut, paste
├── ShapeControlTests.cs      # Shape type and fill mode controls
├── StatusBarTests.cs         # Status bar display
├── ToolButtonTests.cs        # Ribbon tool button activation
├── EdgeCaseAndStressTests.cs # Rapid input and stress scenarios
└── ViewToggleAndZoomTests.cs # Zoom, grid, and ruler toggles

SmrtDoodle (Package)/ # MSIX packaging project
```

## License

This project is provided as-is. See the repository for details.
