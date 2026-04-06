# SmrtDoodle

A professional image editor built with **WinUI 3** and **Win2D** for Windows.

SmrtDoodle provides a familiar, intuitive canvas for sketches, diagrams, photo editing, and digital painting — with full layer support, 25 Photoshop-compatible blend modes, AI-powered tools, and extensive file format support including PSD.

## Features

### Drawing & Painting
- **19 Drawing Tools** — Pencil, Brush, Eraser, Line, Curve, Shape, Flood Fill, Text, Eyedropper, Magnifier, Gradient, Blur, Sharpen, Smudge, Clone Stamp, Pattern Fill, Measure, and two selection tools
- **Brush Styles** — Normal, Calligraphy, Airbrush, Oil, Crayon, Marker, Natural Pencil, and Watercolor
- **Shape Library** — 12 shapes (Rectangle, Ellipse, Triangle, Star, Arrow, Heart, Lightning, and more) with outline, fill, or combined modes
- **Gradient Tool** — Linear, Radial, Angle, Reflected, and Diamond gradient modes
- **Retouch Tools** — Blur, Sharpen, and Smudge with adjustable strength
- **Clone Stamp** — Alt+Click to set source, paint to duplicate regions
- **Pattern Fill** — Checkerboard, Diagonal Lines, Dots, Crosshatch, and Brick patterns

### Layers & Compositing
- **Layer System** — Multiple layers with visibility, opacity, blend modes, grouping, masks, and effects
- **25 Blend Modes** — Full Photoshop-compatible set: Normal, Dissolve, Multiply, Screen, Overlay, Soft Light, Hard Light, Color Dodge, Color Burn, and more
- **Layer Effects** — Drop Shadow, Inner Shadow, Outer Glow, and Stroke
- **Adjustment Layers** — Non-destructive Brightness/Contrast, Hue/Saturation, Color Balance, Levels, and Curves
- **Layer Masks** — Grayscale mask compositing

### File Format Support
- **Native Format** — `.sdd` ZIP-based project format preserving layers, metadata, and thumbnail
- **PSD/PSDT** — Import and export with layer preservation (name, visibility, opacity, blend modes)
- **Image Formats** — PNG, JPEG, BMP, GIF, TIFF, WebP, ICO, SVG, TGA, DDS, and PDF via Magick.NET

### AI Tools (Pro)
- Remove Background, Upscale (2×/4×), Content-Aware Fill, Auto-Colorize, Style Transfer, and Noise Reduction
- Powered by Windows AI platform (`systemAIModels` capability)

### Performance & Canvas
- **Tiled Rendering** — 512 px tile grid with viewport culling and dirty-tile tracking for 8K canvas support
- **Optimized Undo/Redo** — Diff-based dirty-rect undo with 512 MB memory budget
- **Canvas Settings** — Configurable width, height, DPI, background color, grid, and ruler
- **Zoom** — 10–800% with checkerboard transparency preview

### Accessibility & Localization
- **Screen Reader Support** — AutomationProperties on all interactive elements, landmark regions, and live announcements
- **Keyboard Navigation** — AccessKey bindings for all 19 tools and full keyboard shortcut support
- **High Contrast** — Four-theme high contrast resource dictionary
- **8 Languages** — English, Spanish, German, French, Japanese, Chinese, Arabic, and Portuguese with RTL support

### Integration
- **Printing** — Native print dialog via PrintDocument with Fit to Page, Actual Size, and Custom DPI modes
- **SmrtPad Integration** — Named-pipe IPC for inserting drawings directly into [SmrtPad](https://github.com/John-Donnelly/SmrtPad) documents
- **Clipboard** — Copy, cut, paste, paste from file, and paste as new image
- **Drag & Drop** — Drop image files onto the canvas to open them
- **Color Palette** — 28-color MS Paint-standard palette with primary/secondary swatches and swap button
- **Ribbon Toolbar** — SmrtPad-style grouped ribbon with Fluent Design theming

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
├── Models/          # Data models (CanvasSettings, Layer, LayerGroup, AdjustmentLayer, LayerEffect, Enums, UndoRedoManager)
├── Services/        # App services (FileService, ClipboardService, IpcService, AIService, FormatConversionService, ProjectService, PrintService, LicenseService, LoggingService, RecentFilesService)
├── Tools/           # Drawing tool implementations (ITool, BasicTools, AdvancedTools, CurveTool, FreeFormSelectionTool, GradientTool, RetouchTools, CloneStampTool, PatternFillTool, MeasureTool)
├── Helpers/         # Converters, image utilities, BlendModeHelper, RenderOptimization, BackgroundOperation
├── Strings/         # Localized resource files (en-US, es-ES, de-DE, fr-FR, ja-JP, zh-CN, ar-SA, pt-BR)
├── Themes/          # High contrast and theme resource dictionaries
├── Program.cs       # Custom WinUI 3 entry point (Bootstrap.Initialize / Shutdown)
├── MainWindow.xaml  # Main application window with ribbon toolbar
└── App.xaml         # Application entry point with theme registration

SmrtDoodle.Tests/
├── Helpers/         # BlendModeHelper, RenderOptimization, TileGrid, BackgroundOperation, PerformanceBenchmark tests
├── Models/          # Layer, LayerGroup, LayerEffect, AdjustmentLayer, CanvasSettings, UndoRedoManager tests
├── Services/        # AIService, Clipboard, FormatConversion, IPC, License, Logging, Print, ProjectData, RecentFiles tests
├── Tools/           # Tool property and functional tests
└── UI/              # Accessibility, AI menu, high contrast, and ribbon bar tests

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
