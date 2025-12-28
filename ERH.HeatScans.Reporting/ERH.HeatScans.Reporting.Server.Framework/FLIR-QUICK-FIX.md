# FLIR Assembly Loading - Quick Fix Guide

## Problem
? **Error:** "Could not load file or assembly 'Flir.Cronos.Filter.Adapter.dll' or one of its dependencies"

## Quick Solution (90% of cases)

### Step 1: Run the DLL Copy Script

**Option A - PowerShell (Recommended):**
```powershell
cd ERH.HeatScans.Reporting.Server.Framework
.\Copy-FLIR-DLLs.ps1
```

**Option B - Command Prompt:**
```cmd
cd ERH.HeatScans.Reporting.Server.Framework
Copy-FLIR-DLLs.bat
```

### Step 2: Rebuild and Run

1. **Clean and Rebuild:**
   - In Visual Studio: Build ? Clean Solution
   - Then: Build ? Rebuild Solution

2. **Run the Application** (F5)

## Why This Happens

The FLIR Atlas Cronos SDK uses **native (unmanaged) C++ DLLs** that must be:
1. ? Copied to the output `bin\` directory
2. ? Match your platform architecture (x64)
3. ? Have required Visual C++ Runtime dependencies

## What the Scripts Do

The copy scripts automatically:
- Locate the FLIR NuGet package files
- Copy all native DLLs from `win-x64\lib` and `win-x64\native` folders
- Place them in your `bin\` directory where the application can find them

## Additional Fixes Applied

### 1. Assembly Resolver (Automatic)
The `Global.asax.cs` now includes an assembly resolver that automatically searches:
- `bin\` directory
- `bin\x64\` subdirectory

### 2. Web.config Probing Paths
The `Web.config` includes assembly probing configuration:
```xml
<probing privatePath="bin;bin\x64;x64" />
```

### 3. PreBuild Event
The project has a PreBuildEvent that should automatically copy DLLs on build:
```
xcopy "...\Flir.Atlas.Cronos.7.6.0\build\net452\win-x64\lib\*.*" "bin\" /Y /I
```

## If Still Not Working

### Check Platform Target (Most Common Issue)

1. **In Visual Studio:**
   - Build ? Configuration Manager
   - Verify **Active solution platform** = **x64**
   
2. **Project Properties:**
   - Right-click project ? Properties
   - Build tab ? Platform target: **x64** (not AnyCPU)

### Install Visual C++ Redistributables

Download and install:
- [Visual C++ 2015-2022 Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe)

### Verify DLLs Were Copied

Check that these files exist in `ERH.HeatScans.Reporting.Server.Framework\bin\`:
```
Flir.Cronos.Filter.Adapter.dll
atlas_c_sdk.dll
avcodec-58.dll
avformat-58.dll
ImageProcessingWrapper.dll
... and other FLIR DLLs
```

## For Complete Troubleshooting

See: `FLIR-ASSEMBLY-TROUBLESHOOTING.md` for detailed solutions including:
- Fusion log viewer usage
- Dependency analysis
- IIS deployment steps
- Manual assembly resolution
- Diagnostic scripts

## Summary

? **Most common fix:** Run `Copy-FLIR-DLLs.ps1` or `Copy-FLIR-DLLs.bat`
? **Second most common:** Ensure project platform is set to **x64**
? **Third most common:** Install Visual C++ Redistributables

---

**Still having issues?** Check `FLIR-ASSEMBLY-TROUBLESHOOTING.md` for advanced solutions.
