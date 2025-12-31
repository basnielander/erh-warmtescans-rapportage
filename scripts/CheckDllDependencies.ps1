# Script to check DLL dependencies
# This script helps identify which DLLs are referenced by Flir.Cronos.Filter.Adapter.dll

param(
    [string]$DllPath = "..\lib\Flir.Cronos.Filter.Adapter.dll"
)

Write-Host "Checking DLL Dependencies..." -ForegroundColor Cyan
Write-Host "Target DLL: $DllPath" -ForegroundColor Yellow
Write-Host ""

# Check if DLL exists
if (!(Test-Path $DllPath)) {
    Write-Host "ERROR: DLL not found at $DllPath" -ForegroundColor Red
    exit 1
}

Write-Host "Method 1: Using System.Reflection" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

try {
    # Load in reflection-only context to avoid loading dependencies
    $assembly = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom((Resolve-Path $DllPath))
    
    Write-Host "Assembly: $($assembly.FullName)" -ForegroundColor White
    Write-Host ""
    Write-Host "Referenced Assemblies:" -ForegroundColor Cyan
    
    $references = $assembly.GetReferencedAssemblies()
    foreach ($ref in $references) {
        Write-Host "  - $($ref.Name) (Version: $($ref.Version))" -ForegroundColor White
        
        # Try to locate the assembly
        $found = $false
        
        # Check GAC
        try {
            $gacAssembly = [System.Reflection.Assembly]::ReflectionOnlyLoad($ref.FullName)
            Write-Host "    ? Found in GAC: $($gacAssembly.Location)" -ForegroundColor Green
            $found = $true
        } catch {
            Write-Host "    ? Not in GAC" -ForegroundColor Yellow
        }
        
        # Check local directory
        $libPath = Join-Path (Split-Path $DllPath) "$($ref.Name).dll"
        if (Test-Path $libPath) {
            Write-Host "    ? Found locally: $libPath" -ForegroundColor Green
            $found = $true
        }
        
        if (!$found) {
            Write-Host "    ?? WARNING: Assembly not found!" -ForegroundColor Red
        }
        
        Write-Host ""
    }
    
} catch {
    Write-Host "Error loading assembly: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

Write-Host ""
Write-Host "Method 2: Using dumpbin (if available)" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Try to find dumpbin
$dumpbin = $null
$vsPaths = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe"
)

foreach ($path in $vsPaths) {
    $found = Get-ChildItem $path -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $dumpbin = $found.FullName
        break
    }
}

if ($dumpbin) {
    Write-Host "Found dumpbin at: $dumpbin" -ForegroundColor White
    Write-Host ""
    Write-Host "Native Dependencies:" -ForegroundColor Cyan
    & $dumpbin /DEPENDENTS (Resolve-Path $DllPath) | Where-Object { $_ -match "\.dll" }
} else {
    Write-Host "dumpbin.exe not found. Install Visual Studio C++ tools to use this feature." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Method 3: Check current directory DLLs" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

$libDir = Split-Path $DllPath
Write-Host "Checking directory: $libDir" -ForegroundColor White
Write-Host ""

Get-ChildItem "$libDir\*.dll" | ForEach-Object {
    Write-Host "  $($_.Name)" -ForegroundColor White
}

Write-Host ""
Write-Host "Method 4: Check bin/Debug directory" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

$binDebug = Join-Path (Split-Path $DllPath -Parent) "..\ERH.FLIR\bin\Debug"
if (Test-Path $binDebug) {
    Write-Host "Checking directory: $binDebug" -ForegroundColor White
    Write-Host ""
    
    Get-ChildItem "$binDebug\*.dll" | ForEach-Object {
        Write-Host "  $($_.Name)" -ForegroundColor White
    }
} else {
    Write-Host "Directory not found: $binDebug" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "========" -ForegroundColor Cyan
Write-Host "1. Check the referenced assemblies above for missing DLLs marked with ??" -ForegroundColor White
Write-Host "2. Enable Fusion Log (run EnableFusionLog.ps1 as admin) for detailed runtime info" -ForegroundColor White
Write-Host "3. Run your application and check C:\FusionLogs for detailed binding failures" -ForegroundColor White
Write-Host "4. Look for 'WRN: The same bind was seen before' or 'ERR: Failed to complete setup'" -ForegroundColor White
