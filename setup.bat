@echo off
REM .NET nanoFramework Setup Script for Windows
REM This script helps with initial setup of .NET nanoFramework development environment

setlocal enabledelayedexpansion

echo.
echo ============================================================
echo .NET nanoFramework Setup Script
echo ============================================================
echo.

REM Check if dotnet is installed
echo [1/5] Checking .NET installation...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found. Please install .NET SDK from https://dotnet.microsoft.com/download
    exit /b 1
)
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK installed: !DOTNET_VERSION!

REM Check if nanoff is installed
echo.
echo [2/5] Checking nanoff installation...
nanoff --version >nul 2>&1
if errorlevel 1 (
    echo [INFO] nanoff not found, installing...
    dotnet tool install -g nanoff
    if errorlevel 1 (
        echo ERROR: Failed to install nanoff
        exit /b 1
    )
    echo [OK] nanoff installed successfully
) else (
    for /f "tokens=*" %%i in ('nanoff --version') do set NANOFF_VERSION=%%i
    echo [OK] nanoff already installed: !NANOFF_VERSION!
)

REM List available COM ports
echo.
echo [3/5] Detecting connected devices...
nanoff --listports
echo.

REM List available targets
echo [4/5] Available ESP32 targets:
echo.
nanoff --listtargets --platform esp32 | findstr /V "Copyright nanoFramework"

REM Create project structure if not exists
echo.
echo [5/5] Setting up project structure...
if not exist "src" mkdir src
if not exist "firmware" mkdir firmware
if not exist "tools" mkdir tools
echo [OK] Project directories created/verified

echo.
echo ============================================================
echo Setup complete! 
echo ============================================================
echo.
echo Next steps:
echo 1. Identify your ESP32 COM port from the list above
echo 2. Flash firmware: nanoff --platform esp32 --serialport COMX --update
echo 3. For detailed guide: See SETUP_GUIDE.md
echo 4. For quick reference: See QUICK_START.md
echo.
echo Documentation:
echo - Official: https://docs.nanoframework.net/
echo - Samples: https://github.com/nanoframework/Samples
echo - Community: https://discordapp.com/invite/gCyBu8T
echo.
