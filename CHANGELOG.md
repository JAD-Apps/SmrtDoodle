# Changelog

All notable changes to SmrtDoodle will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

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
