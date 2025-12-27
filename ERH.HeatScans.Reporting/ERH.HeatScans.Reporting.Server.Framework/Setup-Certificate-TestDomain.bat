@echo off
echo ========================================
echo Create Certificate for test.nielander.nl
echo ========================================
echo.
echo This script will:
echo - Create a self-signed certificate for test.nielander.nl
echo - Trust the certificate on this machine
echo - Optionally bind it to IIS
echo - Optionally update hosts file
echo.
echo Press Ctrl+C to cancel, or
pause

echo.
echo Checking for Administrator privileges...
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Administrator privileges confirmed.
    echo.
) else (
    echo ERROR: This script requires Administrator privileges.
    echo Please right-click and select "Run as Administrator"
    echo.
    pause
    exit /b 1
)

echo Running certificate creation script...
echo.
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Setup-Certificate-TestDomain.ps1" -DnsName "test.nielander.nl" -FriendlyName "ERH.HeatScans.TestDomain"

if %errorLevel% == 0 (
    echo.
    echo ========================================
    echo Certificate created successfully!
    echo ========================================
    echo.
) else (
    echo.
    echo ========================================
    echo Certificate creation failed!
    echo ========================================
    echo.
)

pause
