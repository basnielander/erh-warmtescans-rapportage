# NuGet Package Restore Script for .NET Framework 4.8.1 Project
# Run this script if you're having issues with NuGet package restoration

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "NuGet Package Restore Utility" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = ".\ERH.HeatScans.Reporting.Server.Framework.csproj"
$packagesConfigPath = ".\packages.config"
$solutionDir = ".."

# Check if files exist
if (!(Test-Path $projectPath)) {
    Write-Host "? Error: Project file not found at $projectPath" -ForegroundColor Red
    exit 1
}

if (!(Test-Path $packagesConfigPath)) {
    Write-Host "? Error: packages.config not found at $packagesConfigPath" -ForegroundColor Red
    exit 1
}

Write-Host "? Project file found" -ForegroundColor Green
Write-Host "? packages.config found" -ForegroundColor Green
Write-Host ""

# Check if NuGet.exe exists
$nugetExe = "nuget.exe"
$nugetPath = Join-Path $env:TEMP $nugetExe

if (!(Test-Path $nugetPath)) {
    Write-Host "Downloading NuGet.exe..." -ForegroundColor Yellow
    $nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    
    try {
        Invoke-WebRequest -Uri $nugetUrl -OutFile $nugetPath
        Write-Host "? NuGet.exe downloaded successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "? Error downloading NuGet.exe: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "? NuGet.exe found" -ForegroundColor Green
}

Write-Host ""
Write-Host "Starting NuGet package restoration..." -ForegroundColor Cyan
Write-Host ""

# Run NuGet restore
try {
    & $nugetPath restore $packagesConfigPath -SolutionDirectory $solutionDir -Verbosity detailed
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "=========================================" -ForegroundColor Green
        Write-Host "? Package restoration completed successfully!" -ForegroundColor Green
        Write-Host "=========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "1. Close and reopen Visual Studio" -ForegroundColor White
        Write-Host "2. Clean and rebuild the solution" -ForegroundColor White
        Write-Host "3. If issues persist, delete the 'bin' and 'obj' folders and rebuild" -ForegroundColor White
    }
    else {
        Write-Host ""
        Write-Host "? Package restoration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        Write-Host ""
        Write-Host "Troubleshooting steps:" -ForegroundColor Yellow
        Write-Host "1. Check your internet connection" -ForegroundColor White
        Write-Host "2. Clear NuGet cache: nuget.exe locals all -clear" -ForegroundColor White
        Write-Host "3. Check packages.config for invalid package references" -ForegroundColor White
        Write-Host "4. Try restoring packages from Visual Studio: Tools > NuGet Package Manager > Package Manager Console" -ForegroundColor White
        Write-Host "   Run: Update-Package -reinstall" -ForegroundColor Gray
    }
}
catch {
    Write-Host "? Error during package restoration: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Checking restored packages..." -ForegroundColor Cyan

$packagesDir = Join-Path $solutionDir "packages"
if (Test-Path $packagesDir) {
    $packageCount = (Get-ChildItem $packagesDir -Directory).Count
    Write-Host "? Found $packageCount package folders in $packagesDir" -ForegroundColor Green
}
else {
    Write-Host "? Warning: packages folder not found at $packagesDir" -ForegroundColor Yellow
}

Write-Host ""
Read-Host "Press Enter to exit"
