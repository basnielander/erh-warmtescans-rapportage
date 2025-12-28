# FLIR Assembly Loading Troubleshooting Guide

## Problem: Cannot load Flir.Cronos.Filter.Adapter.DLL or one of its dependencies

This error occurs because the FLIR Atlas Cronos SDK uses native (unmanaged) DLLs that need to be properly deployed with your application.

## Root Causes

1. **Native DLLs not copied to output directory**
2. **Platform mismatch (x86 vs x64)**
3. **Missing Visual C++ Redistributables**
4. **Assembly probing path issues**

## Solution 1: Manual DLL Copy (Quick Fix)

### Step 1: Run the Copy Script

**PowerShell:**
```powershell
cd ERH.HeatScans.Reporting.Server.Framework
.\Copy-FLIR-DLLs.ps1
```

**Command Prompt:**
```cmd
cd ERH.HeatScans.Reporting.Server.Framework
Copy-FLIR-DLLs.bat
```

### Step 2: Verify Files Copied

Check that these files exist in `bin\` directory:
- `Flir.Cronos.Filter.Adapter.dll`
- `atlas_c_sdk.dll`
- `avcodec-58.dll`
- `avdevice-58.dll`
- `avfilter-7.dll`
- `avformat-58.dll`
- `avutil-56.dll`
- Other FLIR-related DLLs

## Solution 2: Ensure Correct Platform Target

### Verify Project Configuration

1. **In Visual Studio:**
   - Configuration Manager ? Active solution platform ? **x64**
   - Or: Build ? Configuration Manager ? Platform = **x64**

2. **In Project Properties:**
   - Right-click project ? Properties
   - Build tab ? Platform target: **x64**
   - Compile tab ? Prefer 32-bit: **Unchecked**

### Why This Matters

The FLIR SDK requires x64 architecture. If your project is set to AnyCPU or x86, the native DLLs won't load.

## Solution 3: Install Visual C++ Redistributables

The FLIR native libraries depend on Microsoft Visual C++ Runtime:

### Download and Install:
1. [Visual C++ 2015-2022 Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe)
2. [Visual C++ 2015-2022 Redistributable (x86)](https://aka.ms/vs/17/release/vc_redist.x86.exe) - if needed

### Verify Installation:
```powershell
Get-ItemProperty HKLM:\Software\Microsoft\VisualStudio\14.0\VC\Runtimes\x64
```

## Solution 4: Update Web.config Assembly Probing

The `Web.config` has been updated to include assembly probing paths. Verify this section exists:

```xml
<runtime>
  <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
    <probing privatePath="bin;bin\x64;x64" />
    <!-- Other binding redirects -->
  </assemblyBinding>
</runtime>
```

## Solution 5: Pre-Build Event (Automated)

The project already has a PreBuildEvent that should copy DLLs automatically:

```xml
<PreBuildEvent>
  xcopy "C:\Projects\Nuget\packages\Flir.Atlas.Cronos.7.6.0\build\net452\win-$(PlatformTarget)\lib\*.*" "$(TargetDir)" /Y /I
</PreBuildEvent>
```

### If This Fails:

1. **Check the path exists:**
   ```
   C:\Projects\Nuget\packages\Flir.Atlas.Cronos.7.6.0\build\net452\win-x64\lib\
   ```

2. **Verify NuGet package is restored:**
   - Right-click solution ? Restore NuGet Packages
   - Or run: `nuget restore`

3. **Check Visual Studio Output window** for PreBuildEvent errors

## Solution 6: Copy to IIS Application Directory (For IIS Deployment)

If deploying to IIS, ensure DLLs are copied to the application's bin directory:

```powershell
# From project directory
$iisAppPath = "C:\inetpub\wwwroot\YourApp\bin"
Copy-Item "bin\*.dll" -Destination $iisAppPath -Force
```

## Solution 7: Set PATH Environment Variable

Add the bin directory to the PATH (temporary fix for testing):

```powershell
$env:PATH = "$env:PATH;C:\Projects\ERH\warmtescans-rapportage\ERH.HeatScans.Reporting\ERH.HeatScans.Reporting.Server.Framework\bin"
```

## Solution 8: Use Assembly Resolver (Code-based)

Add this to `Global.asax.cs` to manually resolve assemblies:

```csharp
protected void Application_Start()
{
    // Add assembly resolver for FLIR native DLLs
    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    
    GlobalConfiguration.Configure(WebApiConfig.Register);
    // ... rest of your code
}

private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
{
    var assemblyName = new AssemblyName(args.Name);
    
    // Try to load from bin directory
    var binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
    var assemblyPath = Path.Combine(binPath, assemblyName.Name + ".dll");
    
    if (File.Exists(assemblyPath))
    {
        return Assembly.LoadFrom(assemblyPath);
    }
    
    return null;
}
```

## Solution 9: Check Dependency Walker

Use [Dependencies](https://github.com/lucasg/Dependencies) (modern Dependency Walker) to analyze missing dependencies:

1. Download Dependencies.exe
2. Open `Flir.Cronos.Filter.Adapter.dll` from your bin directory
3. Look for red entries (missing DLLs)
4. Install required dependencies

## Solution 10: Use Fusion Log Viewer

Enable assembly binding logging to see exactly what's failing:

### Enable Fusion Logs:

```powershell
# Run as Administrator
Set-ItemProperty -Path "HKLM:\Software\Microsoft\Fusion" -Name "EnableLog" -Value 1
Set-ItemProperty -Path "HKLM:\Software\Microsoft\Fusion" -Name "ForceLog" -Value 1
Set-ItemProperty -Path "HKLM:\Software\Microsoft\Fusion" -Name "LogFailures" -Value 1
Set-ItemProperty -Path "HKLM:\Software\Microsoft\Fusion" -Name "LogResourceBinds" -Value 1
Set-ItemProperty -Path "HKLM:\Software\Microsoft\Fusion" -Name "LogPath" -Value "C:\FusionLogs"
```

### View Logs:
1. Run application
2. Check `C:\FusionLogs\` for binding failure logs
3. Look for `Flir.Cronos.Filter.Adapter` entries

### Disable After Troubleshooting:
```powershell
Remove-ItemProperty -Path "HKLM:\Software\Microsoft\Fusion" -Name "EnableLog"
```

## Common Error Messages and Solutions

### "Could not load file or assembly 'Flir.Cronos.Filter.Adapter'"
? **Solution:** Run Copy-FLIR-DLLs.ps1 or Copy-FLIR-DLLs.bat

### "BadImageFormatException: is not a valid Win32 application"
? **Solution:** Platform mismatch - ensure project is set to x64

### "The specified module could not be found"
? **Solution:** Missing Visual C++ Redistributables or native dependencies

### "Access is denied"
? **Solution:** Run Visual Studio as Administrator or check file permissions

## Verification Steps

After applying fixes, verify the setup:

### 1. Check DLL Presence
```powershell
Get-ChildItem "bin\*.dll" | Select-Object Name
```

### 2. Check Platform Target
```powershell
dumpbin /headers bin\ERH.HeatScans.Reporting.Server.Framework.dll | findstr machine
# Should show: 8664 machine (x64)
```

### 3. Test Application
```powershell
# Start application
# Navigate to: https://localhost:7209/api/folders/heatscanimage/{fileId}
# Should return image without errors
```

## Quick Checklist

- [ ] Platform target is x64
- [ ] NuGet packages restored
- [ ] FLIR DLLs copied to bin directory
- [ ] Visual C++ Redistributables installed
- [ ] Web.config has probing paths
- [ ] PreBuildEvent runs successfully
- [ ] Application runs without assembly load errors

## For IIS Deployment

Additional steps for IIS:

1. **Application Pool:**
   - Set to **.NET CLR Version v4.x**
   - Set **Enable 32-Bit Applications: False**

2. **Copy DLLs:**
   ```powershell
   Copy-Item "bin\*.dll" "C:\inetpub\wwwroot\YourApp\bin\" -Force
   ```

3. **Set Permissions:**
   - IIS_IUSRS needs Read & Execute on bin directory

## Still Having Issues?

### Collect Diagnostic Information:

```powershell
# Check project platform
Write-Host "Project Platform:" (Get-Content ERH.HeatScans.Reporting.Server.Framework.csproj | Select-String "PlatformTarget")

# List DLLs in bin
Write-Host "`nDLLs in bin:"
Get-ChildItem bin\*.dll | Select-Object Name, Length

# Check FLIR package
Write-Host "`nFLIR Package Files:"
Get-ChildItem C:\Projects\Nuget\packages\Flir.Atlas.Cronos.7.6.0\build\net452\win-x64\lib\*.dll | Select-Object Name
```

### Contact Support With:
1. Full error message and stack trace
2. Platform configuration (x86/x64)
3. Visual Studio version
4. Windows version
5. Output from diagnostic script above

## Summary

The most common solution is **Solution 1** (run Copy-FLIR-DLLs script). If that doesn't work, verify **Solution 2** (x64 platform) and **Solution 3** (Visual C++ Redistributables).
