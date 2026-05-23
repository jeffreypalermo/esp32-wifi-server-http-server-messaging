# Quick Start: .NET nanoFramework ESP32

## 1. Check Device Connection

```powershell
cd D:\esp32-seed-nano-framework

# List available COM ports
nanoff --listports

# You should see your ESP32 on a COM port (e.g., COM3)
```

## 2. Flash Firmware to ESP32

Replace `COM3` with your actual COM port from step 1:

```powershell
# For generic ESP32
nanoff --platform esp32 --serialport COM3 --update

# Or for specific board (example: XIAO_ESP32C3)
nanoff --target XIAO_ESP32C3 --serialport COM3 --update
```

Wait for the operation to complete. You should see:
```
Successfully flashed firmware to device
```

## 3. Verify Device is Running nanoFramework

```powershell
# Check if device is detected and responsive
nanoff --listdevices
```

## 4. Install Visual Studio Extension (Optional but Recommended)

For debugging with Visual Studio:

1. Open **Visual Studio 2022**
2. Go to **Extensions > Manage Extensions**
3. Search for "nanoFramework"
4. Click **Install**
5. Restart Visual Studio

## 5. Deploy Sample Application

The sample Blinky application is ready in `src\NanoFrameworkApp\`

### Using Visual Studio:
1. Open `src\NanoFrameworkApp\NanoFrameworkApp.csproj` in Visual Studio
2. Press **F5** to build and deploy
3. Watch your LED blink!

### Using Command Line (dotnet):
```powershell
cd src\NanoFrameworkApp
dotnet build
# Then deploy manually through Visual Studio Device Explorer
```

## 6. Next Steps

- **Learn GPIO**: See `samples/gpio-tutorial.md`
- **I2C Sensors**: See `samples/i2c-guide.md`
- **Networking**: See `samples/networking-guide.md`
- **Join Community**: https://discordapp.com/invite/gCyBu8T

## Troubleshooting

### Device not flashing?
```powershell
# Try with mass erase
nanoff --platform esp32 --serialport COM3 --masserase --update
```

### Device not detected in Visual Studio?
1. Verify connection: `nanoff --listdevices`
2. Check Device Manager for COM port drivers
3. Reinstall USB drivers from manufacturer

### NuGet packages won't restore?
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear
```

---

**For complete guide**: See `SETUP_GUIDE.md`
