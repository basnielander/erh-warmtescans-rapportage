# ?? FLIR DLL Loading - Quick Reference Card

## ? Quick Fix (30 seconds)

```powershell
cd ERH.HeatScans.Reporting.Server.Framework
.\Copy-FLIR-DLLs.ps1
```

**Then:** Press F5 in Visual Studio

---

## ?? The Problem
```
? Could not load file or assembly 'Flir.Cronos.Filter.Adapter.dll'
```

## ? The Solution
FLIR native DLLs must be in `bin\` directory

---

## ?? 3 Most Common Fixes

### 1?? Run Copy Script (90% of cases)
```powershell
.\Copy-FLIR-DLLs.ps1
```

### 2?? Check Platform = x64
Build ? Configuration Manager ? Platform = **x64**

### 3?? Install Visual C++ Runtime
Download: https://aka.ms/vs/17/release/vc_redist.x64.exe

---

## ?? Quick Diagnostic

### Check if DLLs are present:
```powershell
Get-ChildItem bin\Flir.Cronos.Filter.Adapter.dll
```
**Expected:** File found (1.3 MB)

### Check platform setting:
```powershell
Get-Content *.csproj | Select-String "PlatformTarget"
```
**Expected:** `<PlatformTarget>x64</PlatformTarget>`

---

## ?? Required Files in bin\

? Flir.Cronos.Filter.Adapter.dll (1.3 MB)
? Flir.Cronos.Filter.dll
? atlas_c_sdk.dll
? ImageProcessingWrapper.dll
? 11+ other FLIR DLLs

**Total FLIR DLLs:** 15
**Total DLLs in bin:** ~75

---

## ?? When to Run Copy Script

- ? After Clean Build
- ? After Restore NuGet Packages
- ? After Cloning Repository
- ? Before Publishing/Deployment
- ? When error appears

---

## ?? Automated Fixes Applied

1. ? **Assembly Resolver** in Global.asax.cs
2. ? **Probing Paths** in Web.config
3. ? **PreBuildEvent** in .csproj
4. ? **Copy Scripts** available

---

## ?? Documentation

| File | Purpose |
|------|---------|
| **FIX-SUMMARY.md** | Complete fix overview |
| **FLIR-QUICK-FIX.md** | Quick fix guide |
| **FLIR-ASSEMBLY-TROUBLESHOOTING.md** | Detailed solutions |

---

## ?? System Requirements

- **Platform:** x64 (not AnyCPU or x86)
- **.NET Framework:** 4.8
- **Visual C++:** 2015-2022 Redistributable
- **Visual Studio:** 2019 or later

---

## ?? Test After Fix

```bash
# Start app
Press F5

# Test endpoint
GET https://localhost:7209/api/folders/heatscanimage/{fileId}

# Should return: JPEG image (no errors)
```

---

## ?? Still Not Working?

### Check these 3 things:

1. **DLLs present?**
   ```powershell
   Get-ChildItem bin\*Flir*.dll | Measure-Object
   # Should show: Count = 15
   ```

2. **Platform correct?**
   Configuration Manager ? Platform = x64 ?

3. **VC++ installed?**
   Control Panel ? Programs ? Visual C++ 2015-2022 ?

### If still failing:
See: `FLIR-ASSEMBLY-TROUBLESHOOTING.md`

---

## ?? Save This Command

```powershell
# Your go-to fix command:
cd ERH.HeatScans.Reporting.Server.Framework ; .\Copy-FLIR-DLLs.ps1
```

**Bookmark this file for quick reference!**

---

## ? Success Indicators

- ? Build succeeds
- ? No FileNotFoundException
- ? No BadImageFormatException  
- ? Application runs
- ? Thermal imaging works

---

## ?? TL;DR

**Problem:** FLIR DLL not found
**Solution:** Run `Copy-FLIR-DLLs.ps1`
**Platform:** Must be x64
**Runtime:** Need VC++ 2015-2022

**Fixed!** ?
