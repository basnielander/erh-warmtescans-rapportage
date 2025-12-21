@echo off
SETLOCAL

echo =========================================
echo NuGet Package Restore Utility
echo =========================================
echo.

REM Check if packages.config exists
if not exist "packages.config" (
    echo [ERROR] packages.config not found!
    pause
    exit /b 1
)

echo [OK] packages.config found
echo.

REM Check if nuget.exe is in PATH
where nuget >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [WARNING] nuget.exe not found in PATH
    echo.
    echo Please install NuGet CLI from:
    echo https://www.nuget.org/downloads
    echo.
    echo Or use Visual Studio Package Manager Console:
    echo Tools ^> NuGet Package Manager ^> Package Manager Console
    echo Run: Update-Package -ProjectName ERH.HeatScans.Reporting.Server.Framework -Reinstall
    echo.
    pause
    exit /b 1
)

echo [OK] nuget.exe found
echo.
echo Starting package restoration...
echo.

REM Restore packages
nuget restore packages.config -SolutionDirectory .. -Verbosity detailed

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =========================================
    echo [SUCCESS] Package restoration completed!
    echo =========================================
    echo.
    echo Next steps:
    echo 1. Close and reopen Visual Studio
    echo 2. Clean and rebuild the solution
    echo 3. If issues persist, run CleanAndRestore.bat
) else (
    echo.
    echo [ERROR] Package restoration failed!
    echo.
    echo Troubleshooting:
    echo 1. Check internet connection
    echo 2. Run: nuget locals all -clear
    echo 3. See NUGET_TROUBLESHOOTING.md for more help
)

echo.
pause
