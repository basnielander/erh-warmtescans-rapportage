@echo off
echo ========================================
echo IIS Setup for ERH.HeatScans.Reporting
echo ========================================
echo.
echo This script will:
echo - Create self-signed certificate
echo - Configure IIS website and app pool
echo - Set up HTTPS binding
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

echo Running IIS setup script...
echo.
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Setup-IIS.ps1"

if %errorLevel% == 0 (
    echo.
    echo ========================================
    echo Setup completed successfully!
    echo ========================================
    echo.
    echo Next steps:
    echo 1. Publish the application in Visual Studio
    echo 2. Copy google-credentials.json to C:\inetpub\wwwroot\ERH.HeatScans.Reporting\
    echo 3. Browse to https://localhost/
    echo.
) else (
    echo.
    echo ========================================
    echo Setup failed with error code %errorLevel%
    echo ========================================
    echo.
    echo Please check the error messages above.
    echo.
)

pause
