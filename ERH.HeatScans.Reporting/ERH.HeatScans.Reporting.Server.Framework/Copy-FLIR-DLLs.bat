@echo off
REM Script to copy FLIR native DLLs to the output directory

echo Copying FLIR Native DLLs...

set NUGET_PATH=C:\Projects\Nuget\packages
set FLIR_PACKAGE_PATH=%NUGET_PATH%\Flir.Atlas.Cronos.7.6.0\build\net452

REM Determine platform (default to x64)
if "%Platform%"=="x86" (
    set PLATFORM_TARGET=x86
) else if "%Platform%"=="Win32" (
    set PLATFORM_TARGET=x86
) else (
    set PLATFORM_TARGET=x64
)

echo Platform: %PLATFORM_TARGET%

set SOURCE_LIB=%FLIR_PACKAGE_PATH%\win-%PLATFORM_TARGET%\lib
set SOURCE_NATIVE=%FLIR_PACKAGE_PATH%\win-%PLATFORM_TARGET%\native
set TARGET_DIR=%~dp0bin

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

REM Copy lib DLLs
if exist "%SOURCE_LIB%" (
    echo Copying from: %SOURCE_LIB%
    xcopy "%SOURCE_LIB%\*.dll" "%TARGET_DIR%" /Y /Q
)

REM Copy native DLLs
if exist "%SOURCE_NATIVE%" (
    echo Copying from: %SOURCE_NATIVE%
    xcopy "%SOURCE_NATIVE%\*.dll" "%TARGET_DIR%" /Y /Q
)

echo FLIR DLLs copied successfully!
