# NuGet Package Restoration Troubleshooting Guide

## Quick Fix Steps

### Method 1: Use the Restore Script (Recommended)
```powershell
cd ERH.HeatScans.Reporting.Server.Framework
.\RestorePackages.ps1
```

### Method 2: Visual Studio Package Manager Console
1. Open Visual Studio
2. Go to **Tools > NuGet Package Manager > Package Manager Console**
3. Run:
```powershell
Update-Package -ProjectName ERH.HeatScans.Reporting.Server.Framework -Reinstall
```

### Method 3: Manual NuGet Command
```powershell
cd ERH.HeatScans.Reporting.Server.Framework
nuget restore packages.config -SolutionDirectory ..
```

### Method 4: Visual Studio UI
1. Right-click on the project in Solution Explorer
2. Select **"Manage NuGet Packages..."**
3. Click on the **"Installed"** tab
4. Click **"Restore"** button at the top

## Common Issues and Solutions

### Issue 1: "Unable to find version X of package Y"

**Cause:** Package version not available or network issue

**Solution:**
1. Check internet connection
2. Clear NuGet cache:
```powershell
nuget locals all -clear
```
3. Update package source:
```powershell
nuget sources Add -Name "nuget.org" -Source "https://api.nuget.org/v3/index.json"
```

### Issue 2: "This project references NuGet package(s) that are missing"

**Cause:** Package references in .csproj don't match packages.config

**Solution:**
1. Close Visual Studio
2. Delete `bin` and `obj` folders:
```powershell
Remove-Item -Recurse -Force .\bin, .\obj
```
3. Run RestorePackages.ps1
4. Reopen Visual Studio and rebuild

### Issue 3: "Could not install package... Package restore failed"

**Cause:** Target framework mismatch or corrupted package

**Solution:**
1. Verify target framework in .csproj is `v4.8.1`
2. Clear local cache:
```powershell
Remove-Item -Recurse -Force $env:USERPROFILE\.nuget\packages
```
3. Restore packages again

### Issue 4: "The type or namespace name 'X' could not be found"

**Cause:** Assembly references not properly loaded

**Solution:**
1. Ensure packages are restored
2. Close Visual Studio completely
3. Delete `.vs` folder (hidden):
```powershell
Remove-Item -Recurse -Force .\.vs
```
4. Delete `bin` and `obj` folders
5. Reopen Visual Studio
6. Clean and rebuild solution

### Issue 5: "Assembly binding redirect conflicts"

**Cause:** Multiple versions of the same assembly

**Solution:**
Add binding redirects to Web.config (already included):
```xml
<runtime>
  <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
    <dependentAssembly>
      <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" />
      <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
    </dependentAssembly>
    <!-- Other redirects... -->
  </assemblyBinding>
</runtime>
```

### Issue 6: "Package 'X' is not compatible with 'net481'"

**Cause:** Package doesn't support .NET Framework 4.8.1

**Solution:**
1. Find compatible version:
```powershell
nuget list Microsoft.AspNet.WebApi -AllVersions
```
2. Update packages.config with compatible version
3. Restore packages

## Verification Steps

After restoration, verify:

### 1. Check packages folder
```powershell
Get-ChildItem ..\packages -Directory | Measure-Object
```
Expected: ~30 package folders

### 2. Check key assemblies
```powershell
Test-Path ..\packages\Google.Apis.Drive.v3.1.68.0.3371\lib\net462\Google.Apis.Drive.v3.dll
Test-Path ..\packages\Microsoft.AspNet.WebApi.5.3.0\lib\net45\System.Web.Http.dll
Test-Path ..\packages\Unity.5.11.10\lib\net48\Unity.Container.dll
```
All should return `True`

### 3. Build the project
```powershell
msbuild ERH.HeatScans.Reporting.Server.Framework.csproj /t:Build /p:Configuration=Debug
```
Should complete without errors

## Required Packages List

These packages should be restored:

### Core Web API (Microsoft)
- Microsoft.AspNet.WebApi (5.3.0)
- Microsoft.AspNet.WebApi.Client (6.0.0)
- Microsoft.AspNet.WebApi.Core (5.3.0)
- Microsoft.AspNet.WebApi.WebHost (5.3.0)
- Microsoft.AspNet.WebApi.Cors (5.3.0)
- Microsoft.AspNet.Cors (5.3.0)

### Google APIs
- Google.Apis (1.68.0)
- Google.Apis.Auth (1.68.0)
- Google.Apis.Core (1.68.0)
- Google.Apis.Drive.v3 (1.68.0.3371)

### Dependency Injection
- Unity (5.11.10)
- Unity.Abstractions (5.11.7)
- Unity.AspNet.WebApi (5.11.2)
- Unity.Container (5.11.11)
- CommonServiceLocator (2.0.7)

### JSON & Security
- Newtonsoft.Json (13.0.3)
- System.IdentityModel.Tokens.Jwt (7.0.3)
- Microsoft.IdentityModel.JsonWebTokens (7.0.3)
- Microsoft.IdentityModel.Logging (7.0.3)
- Microsoft.IdentityModel.Tokens (7.0.3)
- Microsoft.IdentityModel.Abstractions (7.0.3)

### System Dependencies
- System.Buffers (4.5.1)
- System.Memory (4.5.5)
- System.Numerics.Vectors (4.5.0)
- System.Runtime.CompilerServices.Unsafe (6.0.0)
- System.Text.Encodings.Web (6.0.0)
- System.Text.Json (6.0.9)
- System.Threading.Tasks.Extensions (4.5.4)
- System.ValueTuple (4.5.0)

### Build Tools
- Microsoft.CodeDom.Providers.DotNetCompilerPlatform (2.0.1)

## Manual Package Installation

If automatic restore fails, install packages manually:

```powershell
# Navigate to project directory
cd ERH.HeatScans.Reporting.Server.Framework

# Install core packages
nuget install Microsoft.AspNet.WebApi -Version 5.3.0 -OutputDirectory ..\packages
nuget install Google.Apis.Drive.v3 -Version 1.68.0.3371 -OutputDirectory ..\packages
nuget install Unity -Version 5.11.10 -OutputDirectory ..\packages
nuget install Unity.AspNet.WebApi -Version 5.11.2 -OutputDirectory ..\packages

# Install dependencies
nuget install Newtonsoft.Json -Version 13.0.3 -OutputDirectory ..\packages
nuget install System.IdentityModel.Tokens.Jwt -Version 7.0.3 -OutputDirectory ..\packages
```

## Alternative: Use .NET CLI (if available)

If you have .NET CLI tools:

```powershell
dotnet restore ERH.HeatScans.Reporting.Server.Framework.csproj --packages ..\packages
```

## NuGet Configuration

Ensure correct NuGet sources:

```powershell
# List sources
nuget sources

# Add nuget.org if missing
nuget sources Add -Name "nuget.org" -Source "https://api.nuget.org/v3/index.json"

# Update source
nuget sources Update -Name "nuget.org" -Source "https://api.nuget.org/v3/index.json"
```

## Clean Environment

Complete clean and restore:

```powershell
# Clean everything
Remove-Item -Recurse -Force .\bin, .\obj, .\.vs
Remove-Item -Recurse -Force ..\packages

# Restore packages
.\RestorePackages.ps1

# Or use Visual Studio
# Right-click solution > Clean Solution
# Right-click solution > Restore NuGet Packages
# Right-click solution > Rebuild Solution
```

## Check NuGet.config

Create or verify `nuget.config` in solution root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <config>
    <add key="repositoryPath" value="packages" />
  </config>
</configuration>
```

## Proxy/Firewall Issues

If behind corporate proxy:

```powershell
# Set proxy
$env:HTTP_PROXY = "http://proxy.company.com:8080"
$env:HTTPS_PROXY = "http://proxy.company.com:8080"

# Or in nuget.config
nuget config -set http_proxy=http://proxy.company.com:8080
nuget config -set https_proxy=http://proxy.company.com:8080
```

## Logs and Diagnostics

Enable verbose logging:

```powershell
nuget restore packages.config -SolutionDirectory .. -Verbosity detailed > restore-log.txt 2>&1
```

Review restore-log.txt for errors.

## Success Indicators

? Package restoration successful when:

1. No errors in Output window
2. All package folders exist in `..\packages`
3. Project builds without errors
4. No yellow warning triangles on references in Solution Explorer
5. Can press F5 and application runs

## Still Having Issues?

### Check Project File

Ensure `.csproj` has correct structure:
- `<ItemGroup>` with `<Reference>` elements
- `<HintPath>` pointing to `..\packages\...`
- `<Import>` statements for Web Application targets

### Check packages.config

Ensure correct format:
```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="PackageName" version="X.X.X" targetFramework="net481" />
</packages>
```

### Contact Support

If all else fails:
1. Share Output window logs
2. Share Package Manager Console logs
3. Provide restore-log.txt contents
4. Specify Visual Studio version
5. Specify .NET Framework version installed

## Quick Command Reference

```powershell
# Restore packages
.\RestorePackages.ps1

# Clear cache
nuget locals all -clear

# List installed packages
nuget list -Source ..\packages

# Verify framework
[System.Reflection.Assembly]::GetExecutingAssembly().ImageRuntimeVersion

# Rebuild from command line
msbuild /t:Clean,Build /p:Configuration=Debug

# Check NuGet version
nuget help | Select-String -Pattern "NuGet Version"
```

---

**Last Updated:** 2025
**For:** ERH.HeatScans.Reporting.Server.Framework (.NET Framework 4.8.1)
