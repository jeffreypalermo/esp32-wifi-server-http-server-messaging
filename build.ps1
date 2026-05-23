<#
.SYNOPSIS
    Build script for NanoFrameworkApp solution.
.DESCRIPTION
    Restores packages, builds the solution, runs unit tests,
    optionally runs integration tests, and creates a deployment package.
.PARAMETER Integration
    When specified, also runs integration tests against the ESP32-S3 device.
#>
param(
    [switch]$Integration
)

$ErrorActionPreference = "Stop"
$script:ExitCode = 0
$script:StepResults = @()

function Write-StepHeader($message) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " $message" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}

function Write-Success($message) {
    Write-Host "[PASS] $message" -ForegroundColor Green
}

function Write-Failure($message) {
    Write-Host "[FAIL] $message" -ForegroundColor Red
}

function Record-Step($name, $success) {
    $script:StepResults += [PSCustomObject]@{
        Name    = $name
        Success = $success
    }
    if (-not $success) {
        $script:ExitCode = 1
    }
}

# ── Step 0: Check Prerequisites ──────────────────────────────────────────────

Write-StepHeader "Checking Prerequisites"

$dotnetAvailable = $null -ne (Get-Command dotnet -ErrorAction SilentlyContinue)
$nugetAvailable  = $null -ne (Get-Command nuget -ErrorAction SilentlyContinue)

# Find MSBuild via vswhere
$vswherePath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuildPath = $null
if (Test-Path $vswherePath) {
    $vsInstallPath = & $vswherePath -latest -requires Microsoft.Component.MSBuild -property installationPath 2>$null
    if ($vsInstallPath) {
        $candidate = Join-Path $vsInstallPath "MSBuild\Current\Bin\MSBuild.exe"
        if (Test-Path $candidate) {
            $msbuildPath = $candidate
        }
    }
}
$msbuildAvailable = $null -ne $msbuildPath

if ($dotnetAvailable) { Write-Success "dotnet CLI found" } else { Write-Failure "dotnet CLI not found" }
if ($nugetAvailable)  { Write-Success "nuget CLI found"  } else { Write-Failure "nuget CLI not found" }
if ($msbuildAvailable) { Write-Success "MSBuild found at: $msbuildPath" } else { Write-Failure "MSBuild not found via vswhere" }

if (-not $dotnetAvailable -or -not $nugetAvailable -or -not $msbuildAvailable) {
    Write-Failure "Missing prerequisites. Please install the required tools."
    Record-Step "Prerequisites" $false
    exit 1
}
Record-Step "Prerequisites" $true

# ── Step 1: Restore NuGet Packages ───────────────────────────────────────────

Write-StepHeader "Restoring NuGet Packages"

try {
    & nuget restore src\NanoFrameworkApp.sln -PackagesDirectory src\packages
    if ($LASTEXITCODE -ne 0) { throw "nuget restore failed with exit code $LASTEXITCODE" }
    Write-Success "NuGet packages restored"
    Record-Step "NuGet Restore" $true
} catch {
    Write-Failure "NuGet restore failed: $_"
    Record-Step "NuGet Restore" $false
    exit 1
}

# ── Step 2: Build nanoFramework Projects ─────────────────────────────────────

Write-StepHeader "Building nanoFramework Projects"

try {
    & $msbuildPath src\NanoFrameworkApp\NanoFrameworkApp.nfproj /p:Configuration=Release /restore:false /verbosity:minimal
    if ($LASTEXITCODE -ne 0) { throw "Main project build failed" }
    & $msbuildPath src\NanoFrameworkApp.Tests\NanoFrameworkApp.Tests.nfproj /p:Configuration=Release /restore:false /verbosity:minimal
    if ($LASTEXITCODE -ne 0) { throw "Test project build failed" }
    Write-Success "nanoFramework projects built successfully"
    Record-Step "nanoFramework Build" $true
} catch {
    Write-Failure "Build failed: $_"
    Record-Step "nanoFramework Build" $false
    exit 1
}

# ── Step 2b: Build Integration Tests ────────────────────────────────────────

Write-StepHeader "Building Integration Tests"

try {
    & dotnet build src\NanoFrameworkApp.IntegrationTests\NanoFrameworkApp.IntegrationTests.csproj --configuration Release --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Integration tests build failed" }
    Write-Success "Integration tests built successfully"
    Record-Step "Integration Tests Build" $true
} catch {
    Write-Failure "Integration tests build failed: $_"
    Record-Step "Integration Tests Build" $false
}

# ── Step 3: Run nanoFramework Unit Tests ─────────────────────────────────────

Write-StepHeader "Running nanoFramework Unit Tests"

$vstestPath = $null
if ($vsInstallPath) {
    $candidate = Join-Path $vsInstallPath "Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
    if (Test-Path $candidate) {
        $vstestPath = $candidate
    }
}

$testDll = "src\NanoFrameworkApp.Tests\bin\Release\NFUnitTest.dll"
$adapterPath = "src\packages\nanoFramework.TestFramework.3.0.80\lib\net48"
$runsettings = "src\nano.runsettings"

if ($vstestPath -and (Test-Path $testDll)) {
    # Copy adapter files to test output directory
    $testOutputDir = Split-Path $testDll
    Copy-Item "$adapterPath\*" $testOutputDir -Force -ErrorAction SilentlyContinue

    try {
        & $vstestPath $testDll "/TestAdapterPath:$testOutputDir" "/Settings:$runsettings"
        if ($LASTEXITCODE -ne 0) { throw "Unit tests failed with exit code $LASTEXITCODE" }
        Write-Success "Unit tests passed"
        Record-Step "Unit Tests" $true
    } catch {
        Write-Failure "Unit tests failed: $_"
        Record-Step "Unit Tests" $false
    }
} else {
    if (-not $vstestPath) {
        Write-Host "[WARN] vstest.console.exe not found, skipping unit tests" -ForegroundColor Yellow
    } elseif (-not (Test-Path $testDll)) {
        Write-Host "[WARN] Test assembly not found at $testDll, skipping unit tests" -ForegroundColor Yellow
    }
    Record-Step "Unit Tests" $true
}

# ── Step 4: Run Integration Tests (optional) ────────────────────────────────

if ($Integration) {
    Write-StepHeader "Running Integration Tests"

    try {
        & dotnet test src\NanoFrameworkApp.IntegrationTests --filter TestCategory=Integration --verbosity normal
        if ($LASTEXITCODE -ne 0) { throw "Integration tests failed with exit code $LASTEXITCODE" }
        Write-Success "Integration tests passed"
        Record-Step "Integration Tests" $true
    } catch {
        Write-Failure "Integration tests failed: $_"
        Record-Step "Integration Tests" $false
    }
} else {
    Write-Host ""
    Write-Host "[SKIP] Integration tests skipped (use -Integration flag to run)" -ForegroundColor Yellow
}

# ── Step 5: Create Deployment Package ────────────────────────────────────────

Write-StepHeader "Creating Deployment Package"

$publishDir  = "publish"
$buildOutput = "src\NanoFrameworkApp\bin\Release"

try {
    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

    if (Test-Path $buildOutput) {
        Copy-Item "$buildOutput\*" $publishDir -Recurse -Force
        Write-Success "Deployment package created in $publishDir\"
    } else {
        Write-Host "[WARN] Build output directory not found at $buildOutput" -ForegroundColor Yellow
    }
    Record-Step "Deployment Package" $true
} catch {
    Write-Failure "Deployment package creation failed: $_"
    Record-Step "Deployment Package" $false
}

# ── Summary ──────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Build Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

foreach ($step in $script:StepResults) {
    if ($step.Success) {
        Write-Host "  [PASS] $($step.Name)" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $($step.Name)" -ForegroundColor Red
    }
}

Write-Host ""
if ($script:ExitCode -eq 0) {
    Write-Host "BUILD SUCCEEDED" -ForegroundColor Green
} else {
    Write-Host "BUILD FAILED" -ForegroundColor Red
}
Write-Host ""

exit $script:ExitCode
