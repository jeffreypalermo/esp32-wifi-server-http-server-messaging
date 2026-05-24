<#
.SYNOPSIS
    Deploy nanoFramework app to Seeed Studio XIAO ESP32-S3 or ESP32-C3.
.DESCRIPTION
    Builds and flashes nanoFramework firmware, then deploys the application.
.PARAMETER ComPort
    COM port where the device is connected (e.g., COM5). Auto-detected if not specified.
.PARAMETER Target
    SoC target: esp32s3 (default) or esp32c3.
.PARAMETER FirmwareOnly
    Only flash the nanoFramework firmware, don't deploy the app.
.PARAMETER AppOnly
    Only deploy the app (assumes firmware is already flashed).
#>
param(
    [string]$ComPort,
    [ValidateSet("esp32s3", "esp32c3")]
    [string]$Target = "esp32s3",
    [switch]$FirmwareOnly,
    [switch]$AppOnly
)

$ErrorActionPreference = "Stop"

# Map target name to nanoff flash target
$nanoffTarget = if ($Target -eq "esp32c3") { "XIAO_ESP32C3" } else { "ESP32_S3_BLE" }
$socLabel     = if ($Target -eq "esp32c3") { "XIAO ESP32-C3" } else { "XIAO ESP32-S3" }
$ssid         = if ($Target -eq "esp32c3") { "NanoFramework-ESP32-C3" } else { "NanoFramework-ESP32-S3" }

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " nanoFramework $socLabel Deployment" -ForegroundColor Cyan
Write-Host " Target: $Target  (nanoff: $nanoffTarget)" -ForegroundColor Cyan
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
    Write-Host "[2] Flashing nanoFramework firmware to $socLabel..." -ForegroundColor Yellow
    Write-Host "    Target: $nanoffTarget" -ForegroundColor Gray
    Write-Host "    Port:   $ComPort" -ForegroundColor Gray
    Write-Host ""
    
    nanoff --target $nanoffTarget --serialport $ComPort --update
    
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
    
    # ESP32-C3 SuperMini boards without auto-reset wiring need esptool to exit bootloader.
    if ($Target -eq "esp32c3") {
        Write-Host "  Resetting ESP32-C3 out of bootloader via esptool..." -ForegroundColor Gray
        $chip = "esp32c3"
        python -m esptool --chip $chip --port $ComPort --baud 115200 run 2>$null
    }
    
    Write-Host "  Waiting for device to reboot..." -ForegroundColor Gray
    Start-Sleep -Seconds 10
}
else {
    Write-Host "[2] Skipping firmware flash (--AppOnly)" -ForegroundColor Gray
}

Write-Host ""

# ── Step 3: Build Application for Target ─────────────────────────────────────

if (-not $FirmwareOnly) {
    Write-Host "[3] Building application for $socLabel..." -ForegroundColor Yellow
    & "$PSScriptRoot\build.ps1" -Target $Target
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

# ── Step 4: Deploy Application ───────────────────────────────────────────────

if (-not $FirmwareOnly) {
    Write-Host "[4] Deploying application..." -ForegroundColor Yellow
    
    $publishDir = Join-Path $PSScriptRoot "publish"
    
    # Collect all PE files and bundle into deployment.bin
    $peFiles = Get-ChildItem $publishDir -Filter "*.pe" | Sort-Object Name
    $deploymentBin = Join-Path $publishDir "deployment.bin"
    
    $stream = [System.IO.File]::Create($deploymentBin)
    foreach ($pe in $peFiles) {
        $bytes = [System.IO.File]::ReadAllBytes($pe.FullName)
        $stream.Write($bytes, 0, $bytes.Length)
        $padding = (4 - ($bytes.Length % 4)) % 4
        if ($padding -gt 0) { $stream.Write((New-Object byte[] $padding), 0, $padding) }
    }
    $stream.Close()
    
    Write-Host "  Deploying $($peFiles.Count) assemblies to $ComPort..." -ForegroundColor Gray
    
    $maxRetries = 3
    $deployed = $false
    for ($attempt = 1; $attempt -le $maxRetries; $attempt++) {
        nanoff --nanodevice --serialport $ComPort --deploy --image $deploymentBin
        if ($LASTEXITCODE -eq 0) { $deployed = $true; break }
        if ($attempt -lt $maxRetries) {
            Write-Host ""
            Write-Host "  Deployment attempt $attempt failed. Retrying in 8s..." -ForegroundColor Yellow
            Write-Host "  If the board is not responding, press the RESET button now." -ForegroundColor Yellow
            Start-Sleep -Seconds 8
        }
    }
    
    if (-not $deployed) {
        Write-Host ""
        Write-Host "ERROR: Application deployment failed after $maxRetries attempts!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "  Application deployed successfully!" -ForegroundColor Green
}
else {
    Write-Host "[4] Skipping app deployment (--FirmwareOnly)" -ForegroundColor Gray
}

# ── Done ─────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Deployment Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host " Next steps:" -ForegroundColor Yellow
Write-Host "   1. The device will reboot automatically"
Write-Host "   2. Connect to WiFi: '$ssid' (open/no password)"
Write-Host "   3. Open browser: http://192.168.4.1"
Write-Host "   4. Click 'Flash LED' to test!"
Write-Host ""
Write-Host " Troubleshooting:" -ForegroundColor Gray
Write-Host "   - If WiFi doesn't appear, press RESET on the board"
Write-Host "   - Use 'nanoff --listports' to verify connection"
Write-Host "   - Check serial output with a terminal at 115200 baud"
Write-Host ""
