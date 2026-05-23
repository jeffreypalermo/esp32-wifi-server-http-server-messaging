# Advanced .NET nanoFramework Setup Guide

This guide covers advanced scenarios, troubleshooting, and detailed configuration.

## Table of Contents

1. [Advanced nanoff Commands](#advanced-nanoff-commands)
2. [ESP32 Board-Specific Setup](#esp32-board-specific-setup)
3. [Firmware Management](#firmware-management)
4. [Debugging & Troubleshooting](#debugging--troubleshooting)
5. [Performance Tuning](#performance-tuning)
6. [CI/CD Integration](#cicd-integration)

## Advanced nanoff Commands

### Flashing with Specific Options

```powershell
# Full erase before flashing (for problematic devices)
nanoff --platform esp32 --serialport COM3 --masserase --update

# Flash specific firmware version
nanoff --platform esp32 --serialport COM3 --fwversion 1.16.0.602 --update

# Flash preview/development version
nanoff --platform esp32 --serialport COM3 --preview --update

# Set flash frequency and mode (for stability)
nanoff --platform esp32 --serialport COM3 --flashfreq 40 --flashmode dio --update

# Custom flash memory size (2, 4, 8, or 16 MB)
nanoff --platform esp32 --serialport COM3 --partitiontablesize 8 --update
```

### Device Information

```powershell
# List all connected devices (all platforms)
nanoff --listdevices

# Get ESP32 device details
nanoff --devicedetails --platform esp32 --serialport COM3

# Identify which firmware a device needs
nanoff --identifyfirmware --platform esp32 --serialport COM3

# Check for new nanoff versions
nanoff --suppressnanoffversioncheck
```

### Firmware Management

```powershell
# Download firmware to archive without flashing
nanoff --platform esp32 --updatearchive --archivepath ".\firmware_cache"

# Flash from local archive (no internet needed)
nanoff --platform esp32 --serialport COM3 --fromarchive --archivepath ".\firmware_cache" --update

# Clear all cached firmware files
nanoff --clearcache

# List cache directory contents
dir $env:APPDATA\.nanoff
```

## ESP32 Board-Specific Setup

### Identifying Your ESP32 Board

Different ESP32 variants have different hardware layouts:

```powershell
# List all available ESP32 targets with descriptions
nanoff --listtargets --platform esp32

# Search for specific boards
nanoff --listtargets --platform esp32 | findstr "ESP32"
nanoff --listtargets --platform esp32 | findstr "M5"
nanoff --listtargets --platform esp32 | findstr "XIAO"
```

### Common ESP32 Variants

#### Generic ESP32-DevKit (ESP32-WROOM-32)
```powershell
# GPIO pins available: 0-39 (except reserved)
# LED pin: Usually GPIO 2 or 5
nanoff --platform esp32 --serialport COM3 --update
```

#### M5Stack Series
```powershell
# M5Core
nanoff --target M5Core --serialport COM3 --update

# M5Core2
nanoff --target M5Core2 --serialport COM3 --update

# M5StickC
nanoff --target M5StickC --serialport COM3 --update
```

#### XIAO ESP32C3
```powershell
nanoff --target XIAO_ESP32C3 --serialport COM3 --update
# GPIO pins: 0-5 (most pins multiplexed)
```

#### Custom/Unknown Boards
```powershell
# Use generic ESP32 platform when board not listed
nanoff --platform esp32 --serialport COM3 --update
# Adjust GPIO pins based on board documentation
```

## Firmware Management

### Manual Firmware Backup

```powershell
# Backup current firmware before flashing
nanoff --platform esp32 --serialport COM3 `
    --backupfile "esp32_backup.bin" `
    --backuppath ".\backups"

# Restore from backup
nanoff --platform esp32 --serialport COM3 `
    --binfile ".\backups\esp32_backup.bin" `
    --address "0x1000"
```

### Batch Firmware Updates

```powershell
# Update multiple devices (connected to different COM ports)
$devices = @("COM3", "COM4", "COM5")
foreach ($port in $devices) {
    Write-Host "Flashing $port..."
    nanoff --platform esp32 --serialport $port --update
    Start-Sleep -Seconds 5
}
```

### Firmware Version Management

```powershell
# List available firmware versions
nanoff --listtargets --platform esp32

# Download specific version without flashing
nanoff --platform esp32 --fwversion 1.16.0.602 --updatearchive

# Downgrade to older version
nanoff --platform esp32 --serialport COM3 --fwversion 1.16.0.590 --update --masserase
```

## Debugging & Troubleshooting

### Connection Issues

#### Port Detection Problems
```powershell
# Verify COM port is visible
nanoff --listports

# Check Windows Device Manager
# Settings > Devices > Device Manager > Ports (COM & LPT)

# Manually specify USB vendor/product IDs (advanced)
# May need to install CH340 or similar USB driver
```

#### USB Driver Installation

For common USB-to-serial chips:

```powershell
# CH340 driver (Seeed, many clones)
# Download from: https://www.wch.cn/downloads/CH341SER_EXE.html

# FTDI drivers (higher-end boards)
# Download from: https://ftdichip.com/drivers/

# Use Windows Update for standard CDC drivers
```

### Flashing Failures

#### Device Not Responding
```powershell
# 1. Try mass erase first
nanoff --platform esp32 --serialport COM3 --masserase --update

# 2. Put device in download mode manually:
#    - Connect device
#    - Hold BOOT button
#    - Press and release RST button
#    - Release BOOT button
#    - Run flashing command

# 3. Try different baud rate (if supported)
nanoff --platform esp32 --serialport COM3 --baud 115200 --update
```

#### Checksum/CRC Errors
```powershell
# Clear cache and re-download firmware
nanoff --clearcache
nanoff --platform esp32 --serialport COM3 --update

# Try with verbose output to see more details
nanoff --platform esp32 --serialport COM3 --verbosity diagnostic --update
```

### Application Deployment Issues

#### Device Explorer Not Showing Device

```csharp
// In Visual Studio
// 1. Build solution first: Build > Build Solution
// 2. Open Device Explorer: View > Other Windows > Device Explorer
// 3. If device doesn't show, try these in order:
//    - Click "Refresh" button
//    - Right-click device and click "Ping"
//    - Build > Clean Solution
//    - Reload project
//    - Restart Visual Studio
```

#### Breakpoints Not Working
```csharp
// Ensure you're debugging (F5) not just deploying (Ctrl+F5)
// Some optimizations disable debugging - disable in project properties:
// Project > Properties > Build > Optimization = Off
```

#### Application Hangs or Crashes
```csharp
// Add Debug output to see what's happening
using System.Diagnostics;

// Before Main()
Debug.WriteLine("Starting application...");

// In Main() at key points
Debug.WriteLine("GPIO initialized");
Debug.WriteLine($"Pin value: {gpio.Read(LED_PIN)}");

// Check output window in Visual Studio for messages
```

### Build Errors

#### NuGet Package Resolution Fails
```powershell
# Clear all NuGet caches
dotnet nuget locals all --clear

# Restore with verbose output
dotnet restore --verbosity detailed

# Check if package exists
dotnet package search nanoFramework.System.Device.Gpio
```

#### Incompatible Package Versions
```powershell
# Update all nanoFramework packages to latest
dotnet package update --verbosity detailed

# Or manually update project file (.csproj):
# Remove all nanoFramework package references
# Rebuild to see compatible versions
```

## Performance Tuning

### Optimize Flash Usage

```csharp
// Use string resources efficiently
const string MSG = "Hello";  // String in program memory

// Avoid large arrays in RAM
byte[] buffer = new byte[256];  // Consider using queue instead
```

### Optimize Memory Usage

```csharp
// Reuse objects to reduce GC pressure
GpioController gpio = new GpioController();
// vs
for (int i = 0; i < 100; i++) {
    using (var gpio = new GpioController())  // Creates 100 objects!
    {
        // ...
    }
}

// Use structs for small data
struct Point {
    public int X;
    public int Y;
}
```

### Optimize Execution Speed

```csharp
// Move I2C operations outside tight loops
I2cDevice sensor = I2cDevice.Create(settings);
while (running) {
    byte[] data = sensor.ReadRegister(0x00, 2);  // Slow!
}

// vs
while (running) {
    byte[] data = sensorData;  // Cache results
    Thread.Sleep(10);
}
```

## CI/CD Integration

### Automated Flashing Script (PowerShell)

```powershell
# flash.ps1 - Automated flashing for CI/CD

param(
    [string]$ComPort = "COM3",
    [string]$Target = "esp32",
    [int]$Retries = 3
)

$attempt = 0
$success = $false

while ($attempt -lt $Retries -and -not $success) {
    $attempt++
    Write-Host "Flashing attempt $attempt of $Retries..."
    
    try {
        nanoff --platform $Target --serialport $ComPort --update --masserase
        $success = $true
        Write-Host "✓ Flash successful"
    } catch {
        Write-Host "✗ Flash failed: $_"
        Start-Sleep -Seconds 5
    }
}

if (-not $success) {
    Write-Error "Failed to flash device after $Retries attempts"
    exit 1
}

exit 0
```

### GitHub Actions Workflow

```yaml
name: Build and Deploy nanoFramework

on: [push]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '6.0.x'
    
    - name: Install nanoff
      run: dotnet tool install -g nanoff
    
    - name: Restore dependencies
      run: dotnet restore src/NanoFrameworkApp
    
    - name: Build
      run: dotnet build src/NanoFrameworkApp --no-restore
    
    - name: Publish
      run: dotnet publish src/NanoFrameworkApp -o ./build
```

### Docker Support

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0

# Install nanoff
RUN dotnet tool install -g nanoff

# Add to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /app
COPY . .

RUN dotnet restore
RUN dotnet build
RUN dotnet publish -o /publish

CMD ["dotnet", "/publish/NanoFrameworkApp.dll"]
```

## Advanced Networking

### WiFi Configuration

```csharp
using System.Device.Wifi;

// Connect to WiFi
WiFiAdapter adapter = WiFiAdapter.FindAdapterByName("WiFi");
adapter.Connect("SSID", WiFiReconnectionKind.Automatic, "password");

// Check connection
Debug.WriteLine($"WiFi connected: {adapter.IsConnected}");
if (adapter.IsConnected) {
    Debug.WriteLine($"IP: {adapter.NetworkInterface.IPv4Address}");
}
```

### Azure IoT Hub Connection

```csharp
using nanoFramework.Azure.Devices.Client;

// Connect to Azure IoT Hub
var client = new DeviceClient("YOUR_HUB.azure-devices.net", 
    new DeviceAuthenticationWithRegistrySymmetricKey(
        "DEVICE_ID", "CONNECTION_STRING"));

// Send telemetry
var message = new Message(Encoding.UTF8.GetBytes("Hello Azure!"));
client.SendEventAsync(message);
```

## Resources

- **nanoff GitHub**: https://github.com/nanoframework/nanoFirmwareFlasher
- **Firmware Releases**: https://github.com/nanoframework/Home/releases
- **ESP32 Documentation**: https://docs.espressif.com/
- **Board Pinouts**: https://www.espressif.com/en/products/socs/esp32/resources
- **Community Projects**: https://github.com/nanoframework/

