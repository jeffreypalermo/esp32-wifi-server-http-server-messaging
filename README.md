# .NET nanoFramework ESP32 Development Environment

> **Status**: ✅ Fully installed and configured following official setup from https://nanoframework.net/

This repository contains a complete setup for developing .NET nanoFramework applications on ESP32 microcontrollers.

## What is .NET nanoFramework?

.NET nanoFramework is a free and open-source platform that enables C# developers to write embedded applications for microcontrollers (MCUs). It provides:

- **Full C# Support**: Write embedded code using the C# language you know
- **Visual Studio Integration**: Use Visual Studio IDE with live debugging
- **Rich Library Support**: GPIO, I2C, SPI, PWM, ADC, DAC, Networking, etc.
- **Cloud Connectivity**: Azure IoT, AWS IoT support
- **Multiple Hardware Platforms**: ESP32, STM32, Raspberry Pi Pico, and more

## System Status ✅

| Component | Status | Version |
|-----------|--------|---------|
| .NET SDK | ✅ Installed | 10.0.300-preview.0.26177.108 |
| .NET 3.1 Runtime | ✅ Installed | 3.1.32 |
| nanoff Tool | ✅ Installed | 2.5.144 |
| ESP32 Support | ✅ Available | Multiple targets available |
| COM Port | ✅ Detected | COM3 |

## Quick Start

### 1. List Available Devices

```powershell
# PowerShell
.\setup.ps1 -ListDevices

# Or directly
nanoff --listports                                    # List COM ports
nanoff --listtargets --platform esp32                # List ESP32 targets
```

### 2. Flash Firmware to ESP32

```powershell
# Generic ESP32 (replace COM3 with your port)
nanoff --platform esp32 --serialport COM3 --update

# Or specific board
nanoff --target XIAO_ESP32C3 --serialport COM3 --update
```

**Wait for completion** - You should see "Successfully flashed firmware to device"

### 3. Verify Connection

```powershell
nanoff --listdevices
```

### 4. Create & Deploy Application

**Option A: Visual Studio (Recommended)**
1. Install Visual Studio 2022
2. Install nanoFramework extension: Extensions > Manage Extensions > Search "nanoFramework"
3. Create new project: File > New > Project > "Blank Application (nanoFramework)"
4. Add GPIO package: Manage NuGet Packages > Search "nanoFramework.System.Device.Gpio"
5. Press F5 to build, deploy, and debug

**Option B: Command Line**
```powershell
cd src/NanoFrameworkApp
dotnet build
# Deploy via Visual Studio Device Explorer
```

## Project Structure

```
D:\esp32-seed-nano-framework/
├── src/
│   └── NanoFrameworkApp/           # Sample Blinky application
├── firmware/                        # Firmware files (downloaded by nanoff)
├── tools/                          # Utility scripts
├── SETUP_GUIDE.md                  # Complete setup instructions
├── QUICK_START.md                  # Quick reference
├── setup.ps1                       # PowerShell setup script
├── setup.bat                       # Batch setup script
└── README.md                       # This file
```

## Available Tools & Commands

### PowerShell Setup Script

```powershell
# Show help and available devices
.\setup.ps1 -ListDevices

# Flash firmware to specific COM port
.\setup.ps1 -ComPort COM3

# Install missing tools
.\setup.ps1 -InstallTools
```

### nanoff Commands

```powershell
# Firmware flashing
nanoff --platform esp32 --serialport COM3 --update          # Generic ESP32
nanoff --target XIAO_ESP32C3 --serialport COM3 --update     # Specific board
nanoff --platform esp32 --serialport COM3 --masserase --update  # With full erase

# Device information
nanoff --listports                                          # List COM ports
nanoff --listdevices                                        # List connected devices
nanoff --listtargets --platform esp32                       # List available targets
nanoff --devicedetails --platform esp32                     # Get device details

# Maintenance
nanoff --clearcache                                         # Clear cached firmware
nanoff --help                                              # Show all options
nanoff --version                                           # Show version
```

## Sample Application: LED Blinky

The `src/NanoFrameworkApp/` folder contains a basic blinky example:

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

class Program
{
    const int LED_PIN = 2;  // GPIO 2 (change for your board)

    static void Main()
    {
        using (var gpio = new GpioController())
        {
            gpio.OpenPin(LED_PIN, PinMode.Output);
            
            while (true)
            {
                gpio.Write(LED_PIN, PinValue.High);
                Thread.Sleep(1000);
                gpio.Write(LED_PIN, PinValue.Low);
                Thread.Sleep(1000);
            }
        }
    }
}
```

**To deploy:**
1. Adjust `LED_PIN` for your board (common ESP32 pins: 2, 4, 5, 12-19, 21-27)
2. Open in Visual Studio or build with `dotnet build`
3. Deploy with Visual Studio Device Explorer or F5
4. Watch your LED blink!

## ESP32 GPIO Pin Reference

### Common GPIO Pins (Available on most ESP32 boards)
- GPIO 2, 4, 5, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 23, 25, 26, 27

### Pins to Avoid
- GPIO 0: Boot mode (HIGH for normal boot, LOW for download)
- GPIO 1, 3: UART0 (Serial console)
- GPIO 6-11: SPI flash (internal use)

**Refer to your specific board's pinout diagram**

## Available Packages (NuGet)

The most common nanoFramework packages:

```xml
<!-- GPIO support -->
<PackageReference Include="nanoFramework.System.Device.Gpio" />

<!-- I2C communication -->
<PackageReference Include="nanoFramework.System.Device.I2c" />

<!-- SPI communication -->
<PackageReference Include="nanoFramework.System.Device.Spi" />

<!-- PWM support -->
<PackageReference Include="nanoFramework.System.Device.Pwm" />

<!-- ADC support -->
<PackageReference Include="nanoFramework.System.Device.Adc" />

<!-- Serial ports -->
<PackageReference Include="nanoFramework.System.IO.Ports" />

<!-- Networking -->
<PackageReference Include="nanoFramework.System.Net.Http" />

<!-- Azure IoT Hub -->
<PackageReference Include="nanoFramework.Azure.Devices" />

<!-- 100+ IoT Device samples -->
<PackageReference Include="nanoFramework.IoT.Device" />
```

## Troubleshooting

### Device not detected
```powershell
# Verify COM port
nanoff --listports

# Check if in Device Manager under Ports (COM & LPT)
# Update drivers from manufacturer or Windows Update
```

### Firmware flash fails
```powershell
# Try with mass erase
nanoff --platform esp32 --serialport COM3 --masserase --update

# Clear cache and retry
nanoff --clearcache
nanoff --platform esp32 --serialport COM3 --update
```

### Visual Studio Device Explorer not showing device
1. Build the project first (Build > Build Solution)
2. Open Device Explorer: View > Other Windows > Device Explorer
3. Click "Ping" on device to verify connection
4. Check USB driver installation

### Build fails - missing packages
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

## Learning Resources

### Official Documentation
- **Homepage**: https://nanoframework.net/
- **Documentation**: https://docs.nanoframework.net/
- **API Reference**: https://docs.nanoframework.net/api/
- **Samples**: https://github.com/nanoframework/Samples

### Community
- **Discord**: https://discordapp.com/invite/gCyBu8T
- **GitHub**: https://github.com/nanoframework/
- **Discussions**: https://github.com/nanoframework/Home/discussions

### Learning Paths
1. **Beginner**: GPIO, LED blinking, button input
2. **Intermediate**: I2C sensors, ADC, PWM motors
3. **Advanced**: Networking, cloud connectivity, multi-threaded apps
4. **Professional**: Production-grade firmware, OTA updates

## Next Steps

1. ✅ **Environment Setup** (Completed)
2. **Flash Firmware**: `nanoff --platform esp32 --serialport COM3 --update`
3. **Install Visual Studio Extension** (Optional)
4. **Create First Project**: Use sample or create new
5. **Deploy & Debug**: Press F5 or right-click > Deploy
6. **Explore Samples**: Check GPIO, I2C, sensors
7. **Join Community**: Discord for help and ideas

## Tips & Tricks

### Identifying Your ESP32
- Check Device Manager for USB device name
- Look up board model in manufacturer documentation
- Different boards use different pin layouts

### Backup Before Flashing
```powershell
nanoff --platform esp32 --serialport COM3 --backupfile "esp32_backup.bin"
```

### Download Specific Firmware Version
```powershell
nanoff --platform esp32 --serialport COM3 --fwversion 1.16.0.602 --update
```

### List Specific Target Details
```powershell
nanoff --listtargets --platform esp32 | findstr "XIAO"
```

## Support

**Getting Help:**
1. Check official documentation: https://docs.nanoframework.net/
2. Search GitHub issues: https://github.com/nanoframework/
3. Join Discord community: https://discordapp.com/invite/gCyBu8T
4. Ask in community discussions

## License

.NET nanoFramework is licensed under MIT.
Sample code in this repository is also MIT licensed.

---

**Last Updated**: May 2024  
**Version**: nanoFramework 2.5.144 + nanoff 2.5.144

