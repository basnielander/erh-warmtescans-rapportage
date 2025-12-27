# Script to enable Fusion Log Viewer for assembly binding failures
# Run this as Administrator

Write-Host "Enabling Assembly Binding Failure Logging..." -ForegroundColor Cyan

# Create registry keys for Fusion Log
$registryPath = "HKLM:\Software\Microsoft\Fusion"

if (!(Test-Path $registryPath)) {
    New-Item -Path $registryPath -Force | Out-Null
}

# Enable logging
Set-ItemProperty -Path $registryPath -Name "EnableLog" -Value 1 -Type DWord
Set-ItemProperty -Path $registryPath -Name "ForceLog" -Value 1 -Type DWord
Set-ItemProperty -Path $registryPath -Name "LogFailures" -Value 1 -Type DWord
Set-ItemProperty -Path $registryPath -Name "LogResourceBinds" -Value 1 -Type DWord

# Set log path
$logPath = "C:\FusionLogs"
if (!(Test-Path $logPath)) {
    New-Item -Path $logPath -ItemType Directory | Out-Null
}
Set-ItemProperty -Path $registryPath -Name "LogPath" -Value $logPath -Type String

Write-Host "Fusion Log enabled!" -ForegroundColor Green
Write-Host "Log files will be written to: $logPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "To view logs:" -ForegroundColor Cyan
Write-Host "1. Run your application" -ForegroundColor White
Write-Host "2. Check the logs in: $logPath" -ForegroundColor White
Write-Host "3. Open FUSLOGVW.exe (Fusion Log Viewer) from Visual Studio Developer Command Prompt" -ForegroundColor White
Write-Host ""
Write-Host "To disable logging later, run DisableFusionLog.ps1" -ForegroundColor Yellow
