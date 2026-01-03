# Script to copy FLIR native DLLs to the output directory
$ErrorActionPreference = "Stop"

Write-Host "Copying FLIR Native DLLs..." -ForegroundColor Cyan

$projectDir = $PSScriptRoot
$binDir = Join-Path $projectDir "bin"
$nugetPackagesPath = "C:\Projects\Nuget\packages"
$flirPackagePath = Join-Path $nugetPackagesPath "Flir.Atlas.Cronos.7.6.0\build\net452"

# Determine platform
$platform = "x64"  # Default to x64
if ($env:PLATFORM -eq "x86" -or $env:PLATFORM -eq "Win32") {
    $platform = "x86"
}

Write-Host "Platform: $platform" -ForegroundColor Yellow

# Source paths
$nativeLibPath = Join-Path $flirPackagePath "win-$platform\lib"
$nativeNativePath = Join-Path $flirPackagePath "win-$platform\native"

# Create bin directory if it doesn't exist
if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir | Out-Null
}

# Copy native libraries
if (Test-Path $nativeLibPath) {
    Write-Host "Copying from: $nativeLibPath" -ForegroundColor Gray
    Get-ChildItem -Path $nativeLibPath -Filter "*.dll" | ForEach-Object {
        Copy-Item $_.FullName -Destination $binDir -Force
        Write-Host "  Copied: $($_.Name)" -ForegroundColor Green
    }
}

if (Test-Path $nativeNativePath) {
    Write-Host "Copying from: $nativeNativePath" -ForegroundColor Gray
    Get-ChildItem -Path $nativeNativePath -Filter "*.dll" | ForEach-Object {
        Copy-Item $_.FullName -Destination $binDir -Force
        Write-Host "  Copied: $($_.Name)" -ForegroundColor Green
    }
}

# Also copy managed assemblies
$managedPath = Join-Path $flirPackagePath "win-$platform\managed"
if (Test-Path $managedPath) {
    Write-Host "Copying from: $managedPath" -ForegroundColor Gray
    Get-ChildItem -Path $managedPath -Filter "*.dll" | ForEach-Object {
        Copy-Item $_.FullName -Destination $binDir -Force
        Write-Host "  Copied: $($_.Name)" -ForegroundColor Green
    }
}

Write-Host "FLIR DLLs copied successfully!" -ForegroundColor Green
