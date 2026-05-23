#!/usr/bin/env pwsh
# .NET nanoFramework Setup Script
# This script helps with initial setup on Windows using PowerShell

param(
    [switch]$InstallTools = $false,
    [switch]$ListDevices = $false,
    [string]$ComPort = ""
)

$ErrorActionPreference = "Stop"

function Write-Header {
    param([string]$Title)
    Write-Host "`n" -NoNewline
    Write-Host "═" * 50 -ForegroundColor Cyan
    Write-Host $Title -ForegroundColor Cyan
    Write-Host "═" * 50 -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[✓]" -ForegroundColor Green -NoNewline
    Write-Host " $Message"
}

function Write-Info {
    param([string]$Message)
    Write-Host "[ℹ]" -ForegroundColor Blue -NoNewline
    Write-Host " $Message"
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[✗]" -ForegroundColor Red -NoNewline
    Write-Host " $Message"
}

Write-Header ".NET nanoFramework Setup"

# Check .NET installation
Write-Info "Checking .NET SDK..."
try {
    $dotnetVersion = & dotnet --version 2>&1
    Write-Success ".NET SDK version: $dotnetVersion"
} catch {
    Write-Error-Custom ".NET SDK not found"
    Write-Host "`nPlease install .NET SDK from: https://dotnet.microsoft.com/download"
    exit 1
}

# Check/Install nanoff
Write-Info "Checking nanoff (nanoFramework Firmware Flasher)..."
try {
    $nanoffVersion = & nanoff --version 2>&1
    Write-Success "nanoff installed: $($nanoffVersion[0])"
} catch {
    if ($InstallTools) {
        Write-Info "Installing nanoff..."
        & dotnet tool install -g nanoff
        Write-Success "nanoff installed successfully"
    } else {
        Write-Error-Custom "nanoff not found"
        Write-Host "`nTo install: dotnet tool install -g nanoff"
        Write-Host "Or run this script with -InstallTools parameter"
        exit 1
    }
}

# List devices if requested
if ($ListDevices) {
    Write-Header "Connected Devices"
    Write-Info "COM Ports:"
    & nanoff --listports
    
    Write-Info "`nAvailable ESP32 Targets:"
    & nanoff --listtargets --platform esp32 | Select-Object -First 30
    exit 0
}

# Setup project structure
Write-Info "Setting up project structure..."
foreach ($dir in @("src", "firmware", "tools")) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Success "Created directory: $dir"
    } else {
        Write-Success "Directory exists: $dir"
    }
}

# Flash firmware if COM port provided
if ($ComPort) {
    Write-Header "Flashing Firmware"
    Write-Info "Flashing to port $ComPort..."
    Write-Info "This may take a few minutes, please wait..."
    & nanoff --platform esp32 --serialport $ComPort --update
    Write-Success "Firmware flashing complete!"
    exit 0
}

# Show summary
Write-Header "Setup Complete"
Write-Host @"
Next steps:

1. IDENTIFY DEVICE:
   .\setup.ps1 -ListDevices

2. FLASH FIRMWARE:
   .\setup.ps1 -ComPort COM3

3. CREATE PROJECT:
   Use Visual Studio or:
   dotnet new console -n MyProject

4. LEARN MORE:
   - Setup Guide: SETUP_GUIDE.md
   - Quick Start: QUICK_START.md
   - Official Docs: https://docs.nanoframework.net/

OPTIONS:
   .\setup.ps1 -ListDevices      List connected devices
   .\setup.ps1 -ComPort COM3     Flash firmware to COM3
   .\setup.ps1 -InstallTools     Install missing tools
"@
