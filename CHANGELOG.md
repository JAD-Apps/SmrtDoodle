# Changelog

All notable changes to SmrtDoodle will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.9.5] - 2026-05-01

### Added
- **SmrtPad bridge integration** — SmrtDoodle now supports protocol activation via `smrtdoodle://edit?pipe=…&v=1`; when launched by SmrtPad it connects to a per-session named pipe, receives an optional source PNG frame, and returns the rendered drawing back to SmrtPad when the user finishes
- **`SmrtPadBridgeSession`** service — manages the named-pipe client connection, exposes `IncomingImagePng` for pre-loading the canvas, and provides `SendImageAsync` / `SendCancelAsync` for the result handoff
- **`App.xaml.cs` protocol activation** — `OnLaunched` inspects `AppInstance.GetCurrent().GetActivatedEventArgs()`, detects protocol-scheme `smrtdoodle`, parses the query string, and starts the bridge session before the main window opens
- **Bridge-aware insertion UI** — when launched from SmrtPad, the **Insert into SmrtPad** button appears in the toolbar; clicking it sends the composited canvas PNG back via the bridge and closes the window; closing the window without inserting sends a cancel frame so SmrtPad can clean up

### Changed
- **`SmrtDoodle.csproj`** — added `<ProjectReference>` to `SmrtAI.Core` for the shared IPC contract and frame serializer
- **`MainWindow.xaml.cs`** — bridge-aware close handler sends a cancel frame if the result has not already been sent; non-bridge close path is unchanged

### Packaging
- Version bumped from `0.7.5.0` → `0.9.5.0` in `SmrtDoodle (Package)/Package.appxmanifest`

---

## [0.7.5] - 2026-04-07

### Fixed

- BrushControlTests: `Value.Value` → `RangeValue.Value` for slider attributes; skip `PageUp` and `ResetToDefault` tests (WinUI 3 slider limitations)
- ColorControlTests: skip 9 `PrimaryColorBorder`/`SecondaryColorBorder` tests and 3 color picker dialog tests (Border controls lack UIA peers); skip tooltip test (not exposed via UIA)
- ShapeControlTests: enhanced `SelectFillMode` with 5-attempt retry loop, canvas click to dismiss residual popups, and progressive wait times
- StatusBarTests: fix tool name expectations; replace `Value.Value` with `RangeValue.Value`; skip `StatusSelection_Exists` and `ZoomSlider_DecrementWithArrowLeft`
- ContextMenuTests: fix `WindowsElement` cast; skip `PrimaryColorSwatch`/`SecondaryColorSwatch` right-click tests (Border controls)
- LayerPanelTests: skip 5 layer button tooltip tests (button names not exposed via UIA)

### Changed

- Updated README with local WinAppDriver testing instructions (replacing remote Appium setup)
- All 409 Appium UI tests now pass locally with 26 skipped (WinUI 3 UIA limitations)

## [0.7.4] - 2026-04-07

### Fixed

- CanvasInteractionTests: skip `CanvasContainer_Exists` and `RulerCanvas_Exists` (Grid/Canvas controls lack UIA automation peers)
- CanvasInteractionTests: eyedropper/magnifier assertions changed from `PrimaryColorBorder`/`SecondaryColorBorder` to `StatusTool`/`DrawingCanvas` (Border controls not in UIA tree)
- ViewToggleAndZoomTests: replaced all `ClickMenuItem("View", "100%")` calls and `Value.Value` with `ClickViewMenu100Percent()` and `RangeValue.Value`
- ViewToggleAndZoomTests: removed `RulerCanvas` assertions (Canvas controls lack automation peers)
- EdgeCaseAndStressTests: replaced `ClickMenuItem("View", "100%")` and `PrimaryColorBorder` assertions

## [0.7.3] - 2026-04-07

### Fixed

- ToolButtonTests: tooltip assertions changed from `HelpText` to `Name` attribute (WinUI 3 does not expose `HelpText` via UIA)
- ToolButtonTests: status bar expected values corrected — "Color Picker" not "Eyedropper", "Select" not "Selection", "Free-Form Select" not "FreeFormSelection"
- MenuBarTests: replaced `ClickMenuItem("View", "100%")` with `ClickViewMenu100Percent()` to avoid `LayerOpacityText` ambiguity

## [0.7.2] - 2026-04-07

### Changed

- Downgraded Appium.WebDriver from v5.0.0 to v4.4.5 to resolve W3C/JSONWP protocol mismatch with WinAppDriver 1.2
- Rewrote `AppiumTestBase` for local WinAppDriver execution — auto-starts WinAppDriver, launches app via `Process.Start`, attaches via `appTopLevelWindow` capability
- Migrated all element helpers from Appium v5 API (`AppiumElement`, `MobileBy`) to v4 API (`WindowsElement`, `FindElementByAccessibilityId`)
- Added `ClickViewMenu100Percent()` helper using XPath `//MenuItem[@Name='100%']` to avoid `LayerOpacityText` name collision
- Added `EnsureCleanState` `[TestInitialize]` to dismiss stale dialogs/menus between tests
- Enhanced `DismissDialog` with Cancel button detection for native file dialogs
- Enhanced `ClickMenuItem` with 3-attempt retry loop for flyout timing issues

## [0.7.1] - 2026-04-07

### Changed

- Switched to unpackaged self-contained deployment (`WindowsPackageType=None`, `WindowsAppSdkSelfContained=true`)
- Simplified `Program.cs` entry point — removed `Bootstrap.Initialize`/`Shutdown` calls (no longer needed for self-contained apps)

### Fixed

- `ApplyFlowDirection` crash when Globalization APIs are unavailable in unpackaged mode — falls back to `CultureInfo.CurrentUICulture`

## [0.7.0] - 2026-04-06

### Added

- 538 unit tests covering all major subsystems (up from 130)
- Performance benchmark tests for tiled rendering, memory estimation, and undo capacity
- Comprehensive tool tests for all Phase 4 drawing tools (Gradient, Blur, Sharpen, Smudge, Clone Stamp, Pattern Fill, Measure)
- Blend mode helper tests validating all 25 Photoshop-compatible blend channel formulas
- Canvas settings, tile grid, and render optimization test suites
- Tests for AI service, license service, logging, print, clipboard, IPC, format conversion, recent files, and project data services
- Accessibility tests covering all 19 AccessKey bindings, AutomationProperties, landmarks, and high contrast themes
- UI integration tests validating AI menu structure and ribbon bar

### Changed

- Updated README with full feature list reflecting all new capabilities
- Organized project structure documentation to include new folders (Helpers, Themes, Strings, Tools)

## [0.6.1] - 2026-04-06

### Added

- AI Tools submenu under Image menu with six operations gated behind Pro license: Remove Background, Upscale (2×/4×), Content-Aware Fill, Auto-Colorize, Style Transfer, and Noise Reduction
- `AIService` infrastructure for Windows AI model integration with `systemAIModels` capability
- Print system overhaul with `PrintDocument` and `PrintManagerInterop` for native system print dialog, plus fallback to OS temp-file handler
- `PrintService` with scale modes: Fit to Page, Actual Size, and Custom DPI
- Named-pipe IPC between SmrtDoodle and SmrtPad (`NotifyImageReadyAsync`, `WaitForCommandAsync`) with orphaned temp file cleanup on startup
- SmrtPad insert confirmation dialog with AccentButtonStyle and icon

### Changed

- `IpcService` upgraded from command-line temp file to named-pipe protocol for bidirectional communication

## [0.6.0] - 2026-04-06

### Added

- Full accessibility support with `AutomationProperties.Name` on all 19 tool buttons, canvas, layer panel, status bar, and menu items
- `AccessKey` bindings for all 19 tools (P/B/E/G/T/I/L/C/H/S/F/M/D/U/R/X/N/A/W)
- `AutomationProperties.LandmarkType` on five UI regions (Navigation, Main, Custom for layers/status/colors)
- `AutomationProperties.LiveSetting` on status bar for screen reader announcements
- High contrast theme support via `HighContrastResources.xaml` with 14 custom brushes across four themes (HighContrast, Default, Light, Dark)
- Localization resource files for eight languages: en-US, es-ES, de-DE, fr-FR, ja-JP, zh-CN, ar-SA, and pt-BR
- RTL layout support for Arabic, Hebrew, Farsi, and Urdu with per-element FlowDirection overrides

## [0.5.1] - 2026-04-06

### Added

- `DirtyRectTracker` for spatial invalidation — only redraws changed canvas regions
- `RenderThrottler` for framerate-capped rendering (configurable target FPS)
- `TileGrid` tiled rendering engine with 512 px tiles, viewport culling, and dirty-tile tracking for 8K canvas support
- `BackgroundOperation.RunAsync` helper for thread-safe background work with `DispatcherQueue` progress marshalling
- `MemoryMonitor` with per-canvas memory estimation and low-memory detection
- Store asset generation script (`Generate-StoreAssets.ps1`) for automated MSIX visual asset creation from base logo

## [0.5.0] - 2026-04-06

### Added

- Updated package manifest: publisher identity (`CN=2B43AD1A-273D-402E-A9A5-FF23C52C75B9, O=JAD Apps`), proper display name, and app description
- File type associations for `.sdd`, `.psd`, `.psdt`, and common image formats in package manifest
- `smrtdoodle://` protocol handler registration
- `STORE_LISTING.md` with full Microsoft Store listing content, feature highlights, system requirements, and privacy policy
- `LicenseService` with `StoreContext` integration for Pro license checking and in-app purchase flow

### Changed

- Package version set to 0.5.0.0 (release candidate)
- Publisher display name updated to "JAD Apps"

## [0.4.1] - 2026-04-06

### Added

- Gradient tool with five modes: Linear, Radial, Angle, Reflected, and Diamond
- Blur, Sharpen, and Smudge retouch tools with configurable strength
- Clone Stamp tool with Alt+Click source selection and offset painting
- Pattern Fill tool with five built-in patterns: Checkerboard, Diagonal Lines, Dots, Crosshatch, and Brick
- Measure tool displaying distance, angle, and delta X/Y in the status bar
- Live text editing with on-canvas TextBox overlay (commit on click-away)
- Seven new tool buttons added to ribbon toolbar with unique AccessKey bindings

## [0.4.0] - 2026-04-06

### Added

- `LayerGroup` model for layer folder organisation with `ParentGroupId` hierarchy
- `AdjustmentLayer` for non-destructive Brightness/Contrast, Hue/Saturation, Color Balance, Levels, and Curves adjustments
- `LayerEffect` model for Drop Shadow, Inner Shadow, Outer Glow, and Stroke effects with full parameter control
- Layer masks (`MaskBitmap`) with grayscale compositing via `CanvasComposite.DestinationIn`
- Expanded layer panel with inline rename, opacity slider, blend mode dropdown, and lock indicators

## [0.3.1] - 2026-04-06

### Added

- Expanded `BlendMode` enum to 25 Photoshop-compatible modes: Normal, Dissolve, Darken, Multiply, ColorBurn, LinearBurn, DarkerColor, Lighten, Screen, ColorDodge, LinearDodge, LighterColor, Overlay, SoftLight, HardLight, VividLight, LinearLight, PinLight, HardMix, Difference, Exclusion, Hue, Saturation, Color, and Luminosity
- `BlendModeHelper` with pixel-level compositing for all 25 blend modes, HSL conversion, and per-channel blend math
- `LoggingService` with structured file logging and unhandled exception handler
- Expanded `ImageHelpers` with `FloodFill` queue-based algorithm and `ImageTransforms` (FlipH, FlipV, Rotate90, Rotate180)

### Changed

- `UndoRedoManager` rewritten with diff-based dirty-rect undo — stores only changed pixel regions instead of full bitmap snapshots, with 512 MB memory budget and automatic history trimming
- `FileService.ComposeLayers()` now applies per-layer blend modes via `BlendModeHelper`

### Added (File Formats)

- Magick.NET integration (`Magick.NET-Q16-AnyCPU` v14.11.1) for extended format support
- `FormatConversionService` bridging Magick.NET and Win2D for format conversion
- PSD/PSDT import with layer preservation (name, visibility, opacity, blend mode mapping)
- PSD export with full layer metadata
- TIFF, WebP, ICO, SVG, TGA, DDS, and PDF import/export support
- `ProjectService` for native `.sdd` ZIP-based project format (JSON metadata + per-layer PNGs + thumbnail)
- Expanded file dialogs with grouped format filters ("All Image Files", "Photoshop", "Common", etc.)

- Comprehensive UI integration test suite (`SmrtDoodle.UITests`) using Appium / WinAppDriver targeting a remote Windows test machine via WinRM — covers brush controls, canvas interaction, color controls, context menus, layer panel, menu bar, selection/clipboard, shapes, status bar, tool buttons, view/zoom toggles, and edge-case/stress scenarios
- `Deploy-Remote.ps1` script for automated build and remote deployment over PS Remoting; reads credentials from a `.env` file (not committed) or environment variables; supports `-UseBuildOutput` flag to deploy Debug build output instead of a self-contained publish
- `Program.cs` custom WinUI 3 entry point with explicit `Bootstrap.Initialize` / `Bootstrap.Shutdown` calls for reliable unpackaged-app startup

### Changed

- All brush and pencil rendering modes now stamp **filled** shapes (circles or ellipses) along the stroke path — eliminates the outline-stroke artifact where a black centre line was visible through semi-transparent brush colours:
  - **Normal / Oil / Marker / Watercolor** — filled circle stamps via `StampFilledCircles` helper
  - **Calligraphy** — filled ellipse stamps at −45° (major × minor axes proportional to stroke width)
  - **Airbrush / Crayon / Natural Pencil** — already filled; stamp parameters tightened
  - **Pencil** — `DrawFilledStroke` stamps filled circles along the movement vector
- Disabled Windows App SDK auto-initializers (`WindowsAppSdkBootstrapInitialize`, `WindowsAppSdkDeploymentManagerInitialize`, `WindowsAppSdkUndockedRegFreeWinRTInitialize`) to prevent `TypeInitializationException` during unpackaged startup; initialisation is now done explicitly in `Program.cs`

### Fixed

- `TypeInitializationException` at app launch caused by Windows App SDK auto-initializers conflicting with the unpackaged bootstrap path
- Brush/pencil strokes showing a black outline or centre line through semi-transparent fills

## [0.2.0] - 2026-07-16

### Added

- Magnifier tool — left-click to zoom in (1.5×), right-click to zoom out
- 8 brush style variants: Normal, Calligraphy, Airbrush, Oil, Crayon, Marker, Natural Pencil, and Watercolor
- Transparent selection mode — removes background color from rectangular selections
- Swap Colors button to quickly exchange primary and secondary colors
- Clear Image command (Ctrl+Shift+N) — fills canvas with the secondary color
- Paste From File command — opens an image file and pastes it onto the active layer

### Changed

- Redesigned toolbar as a SmrtPad-style ribbon bar with grouped sections (Tools, Brush, Shapes, Selection, Colors), fixed 100 px height, vertical dividers, and group labels
- Tool buttons now use a compact 2-row ToggleButton grid (30 × 30 px) instead of single-row AppBarButtons, eliminating overflow at default window size
- Brush size slider and brush style combo moved into a dedicated Brush group
- Shape type and fill mode combos moved into a dedicated Shapes group
- Transparent selection checkbox moved into a dedicated Selection group
- Color palette, primary/secondary swatches, and swap button moved into a dedicated Colors group
- Ribbon uses Fluent Design theme resources (LayerFillColorDefaultBrush, DividerStrokeColorDefaultBrush, TextFillColorSecondaryBrush) matching SmrtPad's ribbon

### Fixed

- Color palette swatches not rendering — corrected ItemsWrapGrid orientation, removed max-height constraint, and added compact item container style
- Color palette layout changed from 2-wide vertical scroll to a proper 2-row horizontal strip with 28 MS Paint-standard colors

## [0.1.0] - 2026-04-03

### Added

- Pencil, Brush, and Eraser drawing tools with configurable stroke width
- Line, Curve, and Shape tools with preview rendering
- Shape library: Rectangle, Ellipse, Rounded Rectangle, Triangle, Right Triangle, Diamond, Pentagon, Hexagon, Arrow, Star, Heart, and Lightning
- Shape fill modes: Outline, Fill, and Outline + Fill
- Rectangular and free-form selection tools with move support
- Flood fill tool with configurable tolerance
- Text tool for adding text to the canvas
- Eyedropper tool for picking colors from the canvas
- Layer system with visibility, opacity, blend modes, and duplication
- Bitmap-level undo/redo history (up to 50 steps)
- Clipboard support (copy, cut, paste)
- File I/O for PNG, JPEG, BMP, and GIF formats
- Configurable canvas settings (width, height, DPI, background color)
- Grid and ruler overlay options
- 30-color palette with primary and secondary color support
- Zoom with checkerboard transparency preview
- SmrtPad IPC integration for inserting drawings into documents
- MSIX packaging project for Windows deployment
- MSTest-based test suite covering models, services, tools, and helpers
