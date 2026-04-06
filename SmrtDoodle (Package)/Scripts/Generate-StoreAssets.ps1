<#
.SYNOPSIS
    Generates all required Microsoft Store visual assets from the base Square44x44Logo and StoreLogo.
    
.DESCRIPTION
    Uses System.Drawing to resize the base logo into all required asset sizes and scales
    for MSIX/Store submission. Run from the repository root.

.NOTES
    Requires PowerShell 5.1+ on Windows.
    Output: SmrtDoodle (Package)/Images/
#>

param(
    [string]$ImagesDir = "$PSScriptRoot\..\SmrtDoodle (Package)\Images"
)

Add-Type -AssemblyName System.Drawing

$ImagesDir = (Resolve-Path $ImagesDir -ErrorAction Stop).Path
Write-Host "Generating store assets in: $ImagesDir" -ForegroundColor Cyan

# Find the best source image (largest existing square logo)
$sourceFile = Join-Path $ImagesDir "StoreLogo.png"
if (-not (Test-Path $sourceFile)) {
    $sourceFile = Join-Path $ImagesDir "Square44x44Logo.scale-200.png"
}
if (-not (Test-Path $sourceFile)) {
    Write-Error "No source image found. Please place StoreLogo.png or Square44x44Logo.scale-200.png in Images/."
    exit 1
}

function Resize-Image {
    param(
        [string]$Source,
        [string]$Output,
        [int]$Width,
        [int]$Height
    )
    
    if (Test-Path $Output) {
        Write-Host "  [SKIP] $([System.IO.Path]::GetFileName($Output)) (exists)" -ForegroundColor DarkGray
        return
    }
    
    $src = [System.Drawing.Image]::FromFile($Source)
    $dest = New-Object System.Drawing.Bitmap($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($dest)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.DrawImage($src, 0, 0, $Width, $Height)
    $dest.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $dest.Dispose()
    $src.Dispose()
    Write-Host "  [OK]   $([System.IO.Path]::GetFileName($Output)) (${Width}x${Height})" -ForegroundColor Green
}

Write-Host "`nSource: $sourceFile" -ForegroundColor Yellow

# --- Square44x44Logo ---
$scales = @{ 100 = 44; 125 = 55; 150 = 66; 200 = 88; 400 = 176 }
Write-Host "`n--- Square44x44Logo ---"
foreach ($s in $scales.GetEnumerator()) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "Square44x44Logo.scale-$($s.Key).png") -Width $s.Value -Height $s.Value
}
# Target sizes
foreach ($size in @(16, 24, 32, 48, 256)) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "Square44x44Logo.targetsize-${size}.png") -Width $size -Height $size
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "Square44x44Logo.targetsize-${size}_altform-unplated.png") -Width $size -Height $size
}

# --- Square71x71Logo ---
$scales = @{ 100 = 71; 125 = 89; 150 = 107; 200 = 142; 400 = 284 }
Write-Host "`n--- Square71x71Logo ---"
foreach ($s in $scales.GetEnumerator()) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "Square71x71Logo.scale-$($s.Key).png") -Width $s.Value -Height $s.Value
}

# --- Square150x150Logo ---
$scales = @{ 100 = 150; 125 = 188; 150 = 225; 200 = 300; 400 = 600 }
Write-Host "`n--- Square150x150Logo ---"
foreach ($s in $scales.GetEnumerator()) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "Square150x150Logo.scale-$($s.Key).png") -Width $s.Value -Height $s.Value
}

# --- Square310x310Logo ---
$scales = @{ 100 = 310; 125 = 388; 150 = 465; 200 = 620; 400 = 1240 }
Write-Host "`n--- Square310x310Logo ---"
foreach ($s in $scales.GetEnumerator()) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "Square310x310Logo.scale-$($s.Key).png") -Width $s.Value -Height $s.Value
}

# --- Wide310x150Logo ---
$scales = @{ 100 = @(310,150); 125 = @(388,188); 150 = @(465,225); 200 = @(620,300); 400 = @(1240,600) }
Write-Host "`n--- Wide310x150Logo ---"
foreach ($s in $scales.GetEnumerator()) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "Wide310x150Logo.scale-$($s.Key).png") -Width $s.Value[0] -Height $s.Value[1]
}

# --- StoreLogo ---
$scales = @{ 100 = 50; 125 = 63; 150 = 75; 200 = 100; 400 = 200 }
Write-Host "`n--- StoreLogo ---"
foreach ($s in $scales.GetEnumerator()) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "StoreLogo.scale-$($s.Key).png") -Width $s.Value -Height $s.Value
}

# --- SplashScreen ---
$scales = @{ 100 = @(620,300); 125 = @(775,375); 150 = @(930,450); 200 = @(1240,600); 400 = @(2480,1200) }
Write-Host "`n--- SplashScreen ---"
foreach ($s in $scales.GetEnumerator()) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "SplashScreen.scale-$($s.Key).png") -Width $s.Value[0] -Height $s.Value[1]
}

# --- LockScreenLogo ---
$scales = @{ 100 = 24; 125 = 30; 150 = 36; 200 = 48; 400 = 96 }
Write-Host "`n--- LockScreenLogo ---"
foreach ($s in $scales.GetEnumerator()) {
    Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "LockScreenLogo.scale-$($s.Key).png") -Width $s.Value -Height $s.Value
}

# --- Badge Logo ---
Write-Host "`n--- BadgeLogo ---"
Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "BadgeLogo.scale-100.png") -Width 24 -Height 24
Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "BadgeLogo.scale-200.png") -Width 48 -Height 48
Resize-Image -Source $sourceFile -Output (Join-Path $ImagesDir "BadgeLogo.scale-400.png") -Width 96 -Height 96

Write-Host "`nDone! All store assets generated." -ForegroundColor Green
Write-Host "Total files:" (Get-ChildItem $ImagesDir -Filter "*.png" | Measure-Object).Count
