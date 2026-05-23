# .NET nanoFramework ESP32 Setup Guide

This guide follows the official setup instructions from https://nanoframework.net/

## Prerequisites Installed ✅

- ✅ .NET 10.0 SDK
- ✅ .NET 3.1 Runtime (for nanoff tool)
- ✅ nanoff v2.5.144 (nanoFramework Firmware Flasher)
- ✅ Windows environment with COM port support

## Current System Status

### Installed Tools
- **nanoff version**: 2.5.144+5ec221f455
- **.NET SDK**: 10.0.300-preview.0.26177.108
- **Available COM ports**: COM3
- **Available ESP32 targets**: 
  - M5StickCPlus (v1.16.0.602, v1.16.0.593, v1.16.0.590)
  - XIAO_ESP32C3 (v1.16.0.602, v1.16.0.593, v1.16.0.590)
  - M5StickC (v1.16.0.602, v1.16.0.593, v1.16.0.590)
  - M5Core2 (v1.16.0.602, v1.16.0.593, v1.16.0.590)
  - M5Core (v1.16.0.602, v1.16.0.593, v1.16.0.590)
  - ESP32_WT32_ETH01 (v1.16.0.602, v1.16.0.593, v1.16.0.590)
  - ESP32_S3_BLE_UART and more...

## Setup Steps

### 1. Visual Studio Extension Installation

To develop .NET nanoFramework applications with Visual Studio:

1. **Download Visual Studio 2022** (Community, Professional, or Enterprise)
   - Install **.NET desktop development** workload
   - Install **.NET Core cross-platform development** workload

2. **Install nanoFramework Extension**
   - Open Visual Studio
   - Go to **Extensions > Manage Extensions**
   - Search for "nanoFramework"
   - Click **Install**
   - Restart Visual Studio when prompted

### 2. Flash Firmware to Device

Before running nanoFramework applications, you must flash the firmware to your ESP32 device:

```powershell
# List connected devices
nanoff --listports

# Flash firmware to ESP32 (generic)
nanoff --platform esp32 --serialport COM3 --update

# Or flash specific ESP32 board (example with XIAO_ESP32C3)
nanoff --target XIAO_ESP32C3 --serialport COM3 --update
```

### 3. Create Your First Project

Once firmware is flashed:

1. **Create a new nanoFramework project**
   - File > New > Project
   - Search for "nanoFramework"
   - Select "Blank Application (nanoFramework)"
   - Click Next, name your project, and Create

2. **Add GPIO package** (for LED blinking example)
   - Right-click **References** > **Manage NuGet Packages**
   - Search for "nanoFramework"
   - Install **nanoFramework.System.Device.Gpio**

3. **Update mscorlib**
   - Right-click **References** > **Manage NuGet Packages**
   - Click **Updates** tab
   - Check **Include Prerelease**
   - Select **mscorlib** and update

### 4. Build and Deploy

```
1. Click Build > Build Solution (Ctrl+Shift+B)
2. Open View > Other Windows > Device Explorer
3. Right-click device and click **Ping** to verify connection
4. Press F5 to debug or right-click project > **Deploy**
```

### 5. Hello World - Blinky Example

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

class Program
{
    static void Main()
    {
        // Use GPIO pin 2 for ESP32 (change as needed for your board)
        const int pinNumber = 2;
        
        using (var gpio = new GpioController())
        {
            gpio.OpenPin(pinNumber, PinMode.Output);

            while (true)
            {
                gpio.Write(pinNumber, PinValue.High);
                Thread.Sleep(1000);
                
                gpio.Write(pinNumber, PinValue.Low);
                Thread.Sleep(1000);
            }
        }
    }
}
```

## Common Commands

```powershell
# List all COM ports
nanoff --listports

# List available ESP32 targets
nanoff --listtargets --platform esp32

# List available STM32 targets
nanoff --listtargets --platform stm32

# Check device details
nanoff --listdevices

# Get details of connected device
nanoff --devicedetails --platform esp32

# Flash with specific firmware version
nanoff --platform esp32 --serialport COM3 --fwversion 1.16.0.602 --update

# Flash and mass erase
nanoff --platform esp32 --serialport COM3 --masserase --update

# Clear firmware cache
nanoff --clearcache

# Get help
nanoff --help
```

## Troubleshooting

### Device Not Detected
1. Check Device Manager for COM port and drivers
2. Update drivers from manufacturer website or Windows Update
3. Try different USB cable/port
4. Reset device manually

### Firmware Flash Fails
1. Ensure correct COM port: `nanoff --listports`
2. Ensure device is in bootloader mode (may require holding BOOT button)
3. Try with mass erase: `nanoff --platform esp32 --serialport COM3 --masserase --update`
4. Check drivers are installed

### Visual Studio Extension Issues
1. Update to latest Visual Studio version
2. Uninstall and reinstall extension
3. Check Device Explorer connection with **Ping**

## Resources

- **Official Website**: https://nanoframework.net/
- **Official Documentation**: https://docs.nanoframework.net/
- **GitHub Repository**: https://github.com/nanoframework/
- **Discord Community**: https://discordapp.com/invite/gCyBu8T
- **IoT Device Samples**: https://github.com/nanoframework/nanoFramework.IoT.Device
- **Code Samples**: https://github.com/nanoframework/Samples

## Next Steps

1. Flash firmware to your ESP32 device
2. Install Visual Studio and nanoFramework extension
3. Create first Hello World (Blinky) project
4. Explore device samples for GPIO, I2C, SPI, ADC, etc.
5. Join Discord community for help

## Quick Reference

| Task | Command |
|------|---------|
| Install nanoff | `dotnet tool install -g nanoff` |
| Update nanoff | `dotnet tool update -g nanoff` |
| List COM ports | `nanoff --listports` |
| Flash ESP32 | `nanoff --platform esp32 --serialport COM3 --update` |
| List targets | `nanoff --listtargets --platform esp32` |
| Flash specific target | `nanoff --target XIAO_ESP32C3 --serialport COM3 --update` |
| Check version | `nanoff --version` |
| Get help | `nanoff --help` |

