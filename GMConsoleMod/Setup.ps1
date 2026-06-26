# Setup script for GMConsoleMod
# Run this before building if MelonLoader NuGet is not available

Write-Host "=== GMConsoleMod Setup ===" -ForegroundColor Cyan

# Check if MelonLoader is installed globally
$globalMelonPath = "C:\ProgramData\MelonLoader"
$userMelonPath = "$env:USERPROFILE\.config\MelonLoader"

if (Test-Path $globalMelonPath) {
    Write-Host "Found MelonLoader at: $globalMelonPath" -ForegroundColor Green
    $melonDir = $globalMelonPath
} elseif (Test-Path $userMelonPath) {
    Write-Host "Found MelonLoader at: $userMelonPath" -ForegroundColor Green
    $melonDir = $userMelonPath
} else {
    Write-Host "MelonLoader not found. Please install MelonLoader first:" -ForegroundColor Yellow
    Write-Host "1. Download MelonLoader installer from: https://github.com/LavaGang/MelonLoader"
    Write-Host "2. Install to your game directory"
    Write-Host "3. Copy MelonLoader DLLs to the MelonLoader folder in project root"
    Write-Host ""

    # Create placeholder directory
    $projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
    $melonDir = Join-Path $projectRoot "MelonLoader"
    New-Item -ItemType Directory -Path $melonDir -Force | Out-Null

    Write-Host "Created placeholder MelonLoader folder at: $melonDir" -ForegroundColor Yellow
    Write-Host "Please copy MelonLoader.dll and dependencies there."
}

# Create local MelonLoader folder if it doesn't exist
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$localMelonDir = Join-Path $projectRoot "MelonLoader"

if (-not (Test-Path $localMelonDir)) {
    New-Item -ItemType Directory -Path $localMelonDir -Force | Out-Null
    Write-Host "Created: $localMelonDir" -ForegroundColor Yellow
}

# List required DLLs
Write-Host ""
Write-Host "Required DLLs in MelonLoader folder:" -ForegroundColor Cyan
$dlls = @(
    "MelonLoader.dll",
    "0Harmony.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.InputLegacyModule.dll",
    "UnityEngine.UI.dll",
    "UnityEngine.IMGUIModule.dll"
)

foreach ($dll in $dlls) {
    $dllPath = Join-Path $localMelonDir $dll
    if (Test-Path $dllPath) {
        Write-Host "  [OK] $dll" -ForegroundColor Green
    } else {
        Write-Host "  [MISSING] $dll" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Run 'dotnet build' to build the mod."
