<#
.SYNOPSIS
    Deploy nanoFramework app to Seeed Studio XIAO ESP32-S3.
.DESCRIPTION
    Flashes nanoFramework firmware and deploys the application to the ESP32-S3.
.PARAMETER ComPort
    COM port where the device is connected (e.g., COM3). Auto-detected if not specified.
.PARAMETER FirmwareOnly
    Only flash the nanoFramework firmware, don't deploy the app.
.PARAMETER AppOnly
    Only deploy the app (assumes firmware is already flashed).
#>
param(
    [string]$ComPort,
    [switch]$FirmwareOnly,
    [switch]$AppOnly
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " nanoFramework ESP32-S3 Deployment" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ── Step 1: Detect COM Port ──────────────────────────────────────────────────

if (-not $ComPort) {
    Write-Host "[1] Detecting COM port..." -ForegroundColor Yellow
    $portOutput = nanoff --listports 2>&1 | Out-String
    
    # Parse COM ports from output
    $ports = [regex]::Matches($portOutput, 'COM\d+') | ForEach-Object { $_.Value }
    
    if ($ports.Count -eq 0) {
        Write-Host ""
        Write-Host "ERROR: No COM port detected!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Troubleshooting:" -ForegroundColor Yellow
        Write-Host "  1. Make sure the XIAO ESP32-S3 is plugged in via USB-C"
        Write-Host "  2. Check Device Manager for the COM port"
        Write-Host "  3. Put it in bootloader mode:"
        Write-Host "     - Hold BOOT button"
        Write-Host "     - Press RESET while holding BOOT"
        Write-Host "     - Release RESET, then release BOOT"
        Write-Host "  4. Re-run: .\deploy.ps1 -ComPort COM3"
        Write-Host ""
        exit 1
    }
    elseif ($ports.Count -eq 1) {
        $ComPort = $ports[0]
        Write-Host "  Found: $ComPort" -ForegroundColor Green
    }
    else {
        Write-Host "  Multiple ports found: $($ports -join ', ')" -ForegroundColor Yellow
        $ComPort = $ports[0]
        Write-Host "  Using: $ComPort (specify -ComPort to override)" -ForegroundColor Yellow
    }
}
else {
    Write-Host "[1] Using specified port: $ComPort" -ForegroundColor Green
}

Write-Host ""

# ── Step 2: Flash nanoFramework Firmware ─────────────────────────────────────

if (-not $AppOnly) {
    Write-Host "[2] Flashing nanoFramework firmware to ESP32-S3..." -ForegroundColor Yellow
    Write-Host "    Target: ESP32_S3_BLE" -ForegroundColor Gray
    Write-Host "    Port:   $ComPort" -ForegroundColor Gray
    Write-Host ""
    
    # Flash the firmware
    nanoff --target ESP32_S3_BLE --serialport $ComPort --update
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Firmware flash failed!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Try putting the device in bootloader mode:" -ForegroundColor Yellow
        Write-Host "  1. Hold BOOT button" 
        Write-Host "  2. Press RESET while holding BOOT"
        Write-Host "  3. Release RESET, then release BOOT"
        Write-Host "  4. Re-run this script within 10 seconds"
        Write-Host ""
        exit 1
    }
    
    Write-Host ""
    Write-Host "  Firmware flashed successfully!" -ForegroundColor Green
    Write-Host "  Waiting for device to reboot..." -ForegroundColor Gray
    Start-Sleep -Seconds 5
}
else {
    Write-Host "[2] Skipping firmware flash (--AppOnly)" -ForegroundColor Gray
}

Write-Host ""

# ── Step 3: Deploy Application ───────────────────────────────────────────────

if (-not $FirmwareOnly) {
    Write-Host "[3] Deploying application..." -ForegroundColor Yellow
    
    $publishDir = Join-Path $PSScriptRoot "publish"
    
    if (-not (Test-Path $publishDir)) {
        Write-Host "  Build output not found. Running build first..." -ForegroundColor Gray
        & "$PSScriptRoot\build.ps1"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR: Build failed!" -ForegroundColor Red
            exit 1
        }
    }
    
    # Collect all PE files for deployment
    $peFiles = Get-ChildItem $publishDir -Filter "*.pe" | Select-Object -ExpandProperty FullName
    
    Write-Host "  Deploying $($peFiles.Count) assemblies to $ComPort..." -ForegroundColor Gray
    
    # Deploy using nanoff
    $peArgs = $peFiles | ForEach-Object { "--deploy --image `"$_`"" }
    $deployCmd = "nanoff --target ESP32_S3_BLE --serialport $ComPort $($peArgs -join ' ')"
    
    # nanoff deploy command
    nanoff --target ESP32_S3_BLE --serialport $ComPort --deploy --image ($peFiles -join ',')
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "  Trying alternative deploy method..." -ForegroundColor Yellow
        
        # Alternative: deploy each PE file individually
        foreach ($pe in $peFiles) {
            $name = Split-Path $pe -Leaf
            Write-Host "    Deploying $name..." -ForegroundColor Gray
            nanoff --target ESP32_S3_BLE --serialport $ComPort --deploy --image $pe
        }
    }
    
    Write-Host ""
    Write-Host "  Application deployed successfully!" -ForegroundColor Green
}
else {
    Write-Host "[3] Skipping app deployment (--FirmwareOnly)" -ForegroundColor Gray
}

# ── Done ─────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Deployment Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host " Next steps:" -ForegroundColor Yellow
Write-Host "   1. The device will reboot automatically"
Write-Host "   2. Connect to WiFi: 'NanoFramework-ESP32S3' (open/no password)"
Write-Host "   3. Open browser: http://192.168.4.1"
Write-Host "   4. Click 'Flash LED' to test!"
Write-Host ""
Write-Host " Troubleshooting:" -ForegroundColor Gray
Write-Host "   - If WiFi doesn't appear, press RESET on the board"
Write-Host "   - Use 'nanoff --listports' to verify connection"
Write-Host "   - Check serial output with a terminal at 115200 baud"
Write-Host ""
