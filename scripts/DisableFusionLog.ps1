# Script to disable Fusion Log Viewer
# Run this as Administrator

Write-Host "Disabling Assembly Binding Failure Logging..." -ForegroundColor Cyan

$registryPath = "HKLM:\Software\Microsoft\Fusion"

if (Test-Path $registryPath) {
    Remove-ItemProperty -Path $registryPath -Name "EnableLog" -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $registryPath -Name "ForceLog" -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $registryPath -Name "LogFailures" -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $registryPath -Name "LogResourceBinds" -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $registryPath -Name "LogPath" -ErrorAction SilentlyContinue
}

Write-Host "Fusion Log disabled!" -ForegroundColor Green
