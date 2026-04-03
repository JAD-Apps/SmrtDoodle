<#
.SYNOPSIS
    Builds, publishes, and deploys the SmrtDoodle application to a remote machine
    for Appium UI testing via WinAppDriver.

.PARAMETER RemoteHost
    The IP or hostname of the remote test machine. Default: 192.168.0.100

.PARAMETER Platform
    Build platform. Default: x64

.PARAMETER Configuration
    Build configuration. Default: Release

.PARAMETER Credential
    PSCredential for remote access. If not supplied, you will be prompted.

.PARAMETER SkipBuild
    Skip the build step if the app is already published.

.EXAMPLE
    .\Deploy-Remote.ps1 -RemoteHost 192.168.0.100
    .\Deploy-Remote.ps1 -RemoteHost 192.168.0.100 -Credential (Get-Credential)
    .\Deploy-Remote.ps1 -SkipBuild
#>
param(
    [string]$RemoteHost = "192.168.0.100",
    [string]$Platform = "x64",
    [string]$Configuration = "Release",
    [string]$SolutionRoot = (Resolve-Path "$PSScriptRoot\..\.."),
    [PSCredential]$Credential,
    [switch]$UseBuildOutput,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$AppProject = Join-Path $SolutionRoot "SmrtDoodle\SmrtDoodle.csproj"
$PublishDir = Join-Path $SolutionRoot "publish\$Platform"
$ExePath    = Join-Path $PublishDir "SmrtDoodle.exe"
$BuildOutputDir = Join-Path $SolutionRoot "SmrtDoodle\bin\$Platform\$Configuration\net8.0-windows10.0.19041.0"
$SourceDeployDir = $PublishDir

$runtimeIdentifier = switch ($Platform.ToLowerInvariant()) {
    "x86"   { "win-x86" }
    "x64"   { "win-x64" }
    "arm64" { "win-arm64" }
    default { throw "Unsupported platform '$Platform'. Supported values: x86, x64, ARM64." }
}

function Get-DotEnvValue {
    param(
        [string]$Path,
        [string]$Key
    )

    if (-not (Test-Path $Path)) { return $null }

    $match = Get-Content -Path $Path |
        Where-Object { $_ -match "^\s*$([regex]::Escape($Key))\s*=\s*" } |
        Select-Object -First 1

    if (-not $match) { return $null }

    $value = ($match -split "=", 2)[1].Trim()
    if ($value.StartsWith('"') -and $value.EndsWith('"')) {
        $value = $value.Substring(1, $value.Length - 2)
    }

    return $value
}

# --- Step 1: Build ---
if (-not $SkipBuild) {
    Write-Host "=== Building SmrtDoodle ($Configuration|$Platform) ===" -ForegroundColor Cyan
    if ($UseBuildOutput) {
        dotnet build $AppProject -c $Configuration -p:Platform=$Platform
        if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }
        if (-not (Test-Path (Join-Path $BuildOutputDir "SmrtDoodle.exe"))) { throw "Built exe not found at $BuildOutputDir" }
        $SourceDeployDir = $BuildOutputDir
        Write-Host "Build succeeded: $(Join-Path $BuildOutputDir 'SmrtDoodle.exe')" -ForegroundColor Green
    }
    else {
        dotnet publish $AppProject -c $Configuration -p:Platform=$Platform -r $runtimeIdentifier --self-contained true -p:WindowsAppSDKSelfContained=true -p:PublishTrimmed=false -o $PublishDir
        if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }
        if (-not (Test-Path $ExePath)) { throw "Published exe not found at $ExePath" }
        $SourceDeployDir = $PublishDir
        Write-Host "Build succeeded: $ExePath" -ForegroundColor Green
    }
} else {
    if ($UseBuildOutput) {
        if (-not (Test-Path (Join-Path $BuildOutputDir "SmrtDoodle.exe"))) { throw "No built exe at $BuildOutputDir. Run without -SkipBuild first." }
        $SourceDeployDir = $BuildOutputDir
        Write-Host "Skipping build, using existing build output at $BuildOutputDir" -ForegroundColor Yellow
    }
    else {
        if (-not (Test-Path $ExePath)) { throw "No published exe at $ExePath. Run without -SkipBuild first." }
        $SourceDeployDir = $PublishDir
        Write-Host "Skipping build, using existing publish at $PublishDir" -ForegroundColor Yellow
    }
}

# --- Step 2: Get credentials if needed ---
if (-not $Credential) {
    $envFilePath = Join-Path $SolutionRoot ".env"
    $remoteUser = $env:UITEST_REMOTE_WINRM_USERNAME
    $remotePassword = $env:UITEST_REMOTE_WINRM_PASSWORD

    if ([string]::IsNullOrWhiteSpace($remoteUser)) {
        $remoteUser = Get-DotEnvValue -Path $envFilePath -Key "UITEST_REMOTE_WINRM_USERNAME"
    }

    if ([string]::IsNullOrWhiteSpace($remotePassword)) {
        $remotePassword = Get-DotEnvValue -Path $envFilePath -Key "UITEST_REMOTE_WINRM_PASSWORD"
    }

    if (-not [string]::IsNullOrWhiteSpace($remoteUser) -and -not [string]::IsNullOrWhiteSpace($remotePassword)) {
        $securePassword = ConvertTo-SecureString $remotePassword -AsPlainText -Force
        $Credential = New-Object System.Management.Automation.PSCredential($remoteUser, $securePassword)
        Write-Host "Using remote credentials from environment/.env" -ForegroundColor Green
    }
    else {
        Write-Host "Prompting for credentials to $RemoteHost..." -ForegroundColor Yellow
        $Credential = Get-Credential -Message "Enter credentials for $RemoteHost"
    }
}

# --- Step 3: Deploy via SMB with explicit credentials ---
Write-Host "=== Deploying to $RemoteHost ===" -ForegroundColor Cyan

$ShareName = "SmrtDoodle-Test"
$RemotePath = "C:\$ShareName"
$UncPath = "\\$RemoteHost\$ShareName"

# Try admin share first, fall back to creating a session and xcopy
$adminUnc = "\\$RemoteHost\C`$\SmrtDoodle-Test"
$deployed = $false

# Method 1: Try mapping admin share with credentials
try {
    $netResult = & net use "\\$RemoteHost\C`$" /user:$($Credential.UserName) $($Credential.GetNetworkCredential().Password) 2>&1
    Write-Host "Admin share mapped successfully" -ForegroundColor Green

    if (-not (Test-Path $adminUnc)) {
        New-Item -ItemType Directory -Path $adminUnc -Force | Out-Null
    }
    Copy-Item -Path "$SourceDeployDir\*" -Destination $adminUnc -Recurse -Force
    Write-Host "Files deployed to $adminUnc" -ForegroundColor Green
    $deployed = $true

    & net use "\\$RemoteHost\C`$" /delete /y 2>$null | Out-Null
}
catch {
    Write-Host "Admin share not available: $_" -ForegroundColor Yellow
}

# Method 2: Try PS remoting with credentials
if (-not $deployed) {
    try {
        Write-Host "Trying PS Remoting with credentials..." -ForegroundColor Yellow
        $session = New-PSSession -ComputerName $RemoteHost -Credential $Credential -ErrorAction Stop

        Invoke-Command -Session $session -ScriptBlock {
            param($Path)
            if (Test-Path $Path) {
                Get-ChildItem -Path $Path -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
            }
            if (-not (Test-Path $Path)) { New-Item -ItemType Directory -Path $Path -Force | Out-Null }
        } -ArgumentList $RemotePath

        # Copy files via PS session
        Copy-Item -Path "$SourceDeployDir\*" -Destination $RemotePath -ToSession $session -Recurse -Force
        Write-Host "Files deployed via PS Remoting" -ForegroundColor Green
        $deployed = $true

        # Also ensure WinAppDriver is running
        $WadStatus = Invoke-Command -Session $session -ScriptBlock {
            $proc = Get-Process -Name "WinAppDriver" -ErrorAction SilentlyContinue
            if (-not $proc) {
                $wadPath = "C:\Program Files\Windows Application Driver\WinAppDriver.exe"
                if (Test-Path $wadPath) {
                    Start-Process -FilePath $wadPath -ArgumentList "192.168.0.100 4723" -WindowStyle Hidden
                    Start-Sleep -Seconds 2
                    return "Started"
                }
                return "NotInstalled"
            }
            return "Running"
        }
        Write-Host "WinAppDriver status: $WadStatus" -ForegroundColor $(if ($WadStatus -eq 'Running' -or $WadStatus -eq 'Started') { 'Green' } else { 'Red' })

        Remove-PSSession $session
    }
    catch {
        Write-Host "PS Remoting failed: $_" -ForegroundColor Yellow
    }
}

# Method 3: Robocopy with net use mapped drive
if (-not $deployed) {
    try {
        Write-Host "Trying net use with explicit share..." -ForegroundColor Yellow
        & net use Z: "\\$RemoteHost\C`$" /user:$($Credential.UserName) $($Credential.GetNetworkCredential().Password) 2>&1 | Out-Null

        $destDir = "Z:\SmrtDoodle-Test"
        if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
        & robocopy $SourceDeployDir $destDir /MIR /NJH /NJS /NP 2>&1 | Out-Null
        Write-Host "Files deployed via mapped drive" -ForegroundColor Green
        $deployed = $true

        & net use Z: /delete /y 2>$null | Out-Null
    }
    catch {
        Write-Host "Mapped drive failed: $_" -ForegroundColor Yellow
    }
}

if (-not $deployed) {
    Write-Host ""
    Write-Warning "=== Automatic deployment failed ==="
    Write-Host "Please manually copy the contents of:" -ForegroundColor Yellow
    Write-Host "  $SourceDeployDir" -ForegroundColor White
    Write-Host "to the remote machine at:" -ForegroundColor Yellow
    Write-Host "  $RemoteHost : C:\SmrtDoodle-Test\" -ForegroundColor White
    Write-Host ""
    Write-Host "Then ensure WinAppDriver is running:" -ForegroundColor Yellow
    Write-Host '  & "C:\Program Files\Windows Application Driver\WinAppDriver.exe" 192.168.0.100 4723' -ForegroundColor White
    Write-Host ""
}

# --- Step 4: Verify WinAppDriver reachability ---
Write-Host "=== Verifying WinAppDriver endpoint ===" -ForegroundColor Cyan
try {
    $tcp = New-Object System.Net.Sockets.TcpClient
    $tcp.Connect($RemoteHost, 4723)
    $tcp.Close()
    Write-Host "WinAppDriver is reachable at http://${RemoteHost}:4723" -ForegroundColor Green
}
catch {
    Write-Warning "Cannot reach WinAppDriver at ${RemoteHost}:4723 - ensure it is running on the remote machine"
}

Write-Host ""
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host "Remote app path : C:\SmrtDoodle-Test\SmrtDoodle.exe"
Write-Host "Appium endpoint : http://${RemoteHost}:4723"
Write-Host ""
Write-Host "Run tests with:"
Write-Host "  dotnet test SmrtDoodle.UITests -p:Platform=$Platform"
