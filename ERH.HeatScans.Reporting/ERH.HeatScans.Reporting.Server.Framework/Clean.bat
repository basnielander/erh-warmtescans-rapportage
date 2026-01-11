@echo off
SETLOCAL

echo =========================================
echo Clean Utility
echo =========================================
echo.
echo This will delete bin, obj, and .vs folders
echo.
set /p confirm="Continue? (Y/N): "
if /i not "%confirm%"=="Y" (
    echo Cancelled.
    pause
    exit /b 0
)

echo.
echo Step 1: Cleaning bin folder...
if exist "bin" (
    rmdir /s /q "bin"
    echo [OK] bin folder deleted
) else (
    echo [INFO] bin folder not found
)

echo.
echo Step 2: Cleaning obj folder...
if exist "obj" (
    rmdir /s /q "obj"
    echo [OK] obj folder deleted
) else (
    echo [INFO] obj folder not found
)

echo.
echo Step 3: Cleaning .vs folder...
if exist ".vs" (
    rmdir /s /q ".vs"
    echo [OK] .vs folder deleted
) else (
    echo [INFO] .vs folder not found
)

echo.
echo Step 4: Clearing NuGet cache...
nuget locals all -clear
if %ERRORLEVEL% EQU 0 (
    echo [OK] NuGet cache cleared
) else (
    echo [WARNING] Could not clear NuGet cache
)

echo.
pause
