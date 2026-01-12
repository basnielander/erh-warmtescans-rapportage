# Setup Script for .NET Framework 4.8.1 Project

Write-Host "ERH Heat Scans Reporting - .NET Framework 4.8.1 Setup" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host ""

# Copy appSettings.template.xml to appSettings.xml
$appSettingsTemplate = ".\appSettings.template.xml"
$appSettingsFile = ".\appSettings.xml"

if (Test-Path $appSettingsTemplate) {
    if (!(Test-Path $appSettingsFile)) {
        Write-Host "Copying appSettings.template.xml to appSettings.xml..." -ForegroundColor Yellow
        Copy-Item $appSettingsTemplate $appSettingsFile
        Write-Host "? appSettings.xml created successfully" -ForegroundColor Green
        Write-Host "  Please update the values in appSettings.xml with your configuration" -ForegroundColor Yellow
    } else {
        Write-Host "? appSettings.xml already exists" -ForegroundColor Green
    }
} else {
    Write-Host "? Warning: appSettings.template.xml not found" -ForegroundColor Red
}

Write-Host ""

# Check if google-credentials.json exists in parent directory
$parentCredPath = "..\ERH.HeatScans.Reporting.Server\google-credentials.json"
$localCredPath = ".\google-credentials.json"

if (Test-Path $parentCredPath) {
    Write-Host "Found google-credentials.json in original project" -ForegroundColor Green
    
    if (!(Test-Path $localCredPath)) {
        Write-Host "Copying google-credentials.json to Framework project..." -ForegroundColor Yellow
        Copy-Item $parentCredPath $localCredPath
        Write-Host "? Credentials copied successfully" -ForegroundColor Green
    } else {
        Write-Host "? google-credentials.json already exists in Framework project" -ForegroundColor Green
    }
} else {
    Write-Host "? Warning: google-credentials.json not found" -ForegroundColor Yellow
    Write-Host "  Please place your Google service account credentials file" -ForegroundColor Yellow
    Write-Host "  at: $localCredPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Setup Steps:" -ForegroundColor Cyan
Write-Host "1. Ensure .NET Framework 4.8.1 Developer Pack is installed" -ForegroundColor White
Write-Host "2. Open solution in Visual Studio" -ForegroundColor White
Write-Host "3. Right-click solution ? Restore NuGet Packages" -ForegroundColor White
Write-Host "4. Build the project (Ctrl+Shift+B)" -ForegroundColor White
Write-Host "5. Run the project (F5)" -ForegroundColor White
Write-Host ""
Write-Host "The server will be available at: https://localhost:44300/" -ForegroundColor Green
Write-Host ""
Write-Host "To update the Angular client:" -ForegroundColor Cyan
Write-Host "  Update baseUrl in google-drive.service.ts to:" -ForegroundColor White
Write-Host "  https://localhost:44300/api/user/googledrive" -ForegroundColor Yellow
Write-Host ""

# Check for .NET Framework 4.8.1
$frameworkPath = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1"
if (Test-Path $frameworkPath) {
    Write-Host "? .NET Framework 4.8.1 is installed" -ForegroundColor Green
} else {
    Write-Host "? .NET Framework 4.8.1 not found" -ForegroundColor Yellow
    Write-Host "  Download from: https://dotnet.microsoft.com/download/dotnet-framework/net481" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Setup complete!" -ForegroundColor Green
Write-Host "See README.md and MIGRATION_GUIDE.md for more information" -ForegroundColor White
Write-Host ""

# Pause to read output
Read-Host "Press Enter to continue"
