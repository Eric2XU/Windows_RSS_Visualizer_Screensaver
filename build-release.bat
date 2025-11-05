@echo off
setlocal enabledelayedexpansion

echo ================================================================
echo RSS Visualizer Screensaver - Release Builder
echo ================================================================
echo.

REM Check if we're in the right directory
if not exist "RssVisualizerScreensaver.csproj" (
    echo ERROR: RssVisualizerScreensaver.csproj not found!
    echo Please run this script from the project root directory.
    pause
    exit /b 1
)

echo [1/5] Cleaning previous builds...
dotnet clean RssVisualizerScreensaver.csproj -c Release > nul 2>&1

echo [2/5] Building self-contained version (includes .NET runtime)...
dotnet publish RssVisualizerScreensaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true

if !errorlevel! neq 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo [3/5] Preparing Release folder...
if not exist "Release" mkdir Release

REM Clean Release folder
del /q "Release\*.*" 2>nul

echo [4/5] Copying files to Release folder...

REM Copy main executable
copy "bin\Release\net8.0-windows\win-x64\publish\RssVisualizerScreensaver.exe" "Release\" > nul
if !errorlevel! neq 0 (
    echo ERROR: Failed to copy main executable!
    pause
    exit /b 1
)

REM Copy documentation
copy "README.md" "Release\" > nul 2>&1
copy "INSTALL.txt" "Release\" > nul 2>&1
copy "RELEASE_NOTES.txt" "Release\" > nul 2>&1
copy "DEPLOYMENT_GUIDE.md" "Release\" > nul 2>&1

REM Copy WebView2 documentation if present
copy "bin\Release\net8.0-windows\win-x64\publish\Microsoft.Web.WebView2.*.xml" "Release\" > nul 2>&1

REM Create VERSION.txt
echo RSS Visualizer Screensaver v1.1.1 > "Release\VERSION.txt"
echo Built: %date% %time% >> "Release\VERSION.txt"
echo Target: Windows 10/11 (64-bit) >> "Release\VERSION.txt"
echo Runtime: Self-contained (.NET 8.0 included) >> "Release\VERSION.txt"

REM Create installation batch file
echo @echo off > "Release\Install.bat"
echo echo Installing RSS Visualizer Screensaver... >> "Release\Install.bat"
echo echo. >> "Release\Install.bat"
echo echo This will copy the screensaver to C:\Windows\System32\ >> "Release\Install.bat"
echo echo You need administrator privileges for this to work. >> "Release\Install.bat"
echo echo. >> "Release\Install.bat"
echo pause >> "Release\Install.bat"
echo echo. >> "Release\Install.bat"
echo copy "RssVisualizerScreensaver.exe" "C:\Windows\System32\RssVisualizerScreensaver.scr" >> "Release\Install.bat"
echo if %%errorlevel%% equ 0 ( >> "Release\Install.bat"
echo     echo. >> "Release\Install.bat"
echo     echo ✓ Installation successful! >> "Release\Install.bat"
echo     echo. >> "Release\Install.bat"
echo     echo Next steps: >> "Release\Install.bat"
echo     echo 1. Open Settings ^> Personalization ^> Lock screen ^> Screen saver >> "Release\Install.bat"
echo     echo 2. Select "RssVisualizerScreensaver" from the dropdown >> "Release\Install.bat"
echo     echo 3. Click "Settings" to configure feeds ^(optional^) >> "Release\Install.bat"
echo     echo 4. Click "Preview" to test >> "Release\Install.bat"
echo     echo 5. Set wait time and click "Apply" >> "Release\Install.bat"
echo ^) else ( >> "Release\Install.bat"
echo     echo. >> "Release\Install.bat"
echo     echo ✗ Installation failed! >> "Release\Install.bat"
echo     echo Make sure you're running as Administrator. >> "Release\Install.bat"
echo ^) >> "Release\Install.bat"
echo echo. >> "Release\Install.bat"
echo pause >> "Release\Install.bat"

echo [5/5] Checking file sizes...

for %%f in ("Release\RssVisualizerScreensaver.exe") do (
    set size=%%~zf
    set /a sizeMB=!size! / 1024 / 1024
    echo Main executable: !sizeMB! MB
    
    if !sizeMB! LSS 50 (
        echo WARNING: File size is suspiciously small ^(!sizeMB! MB^)
        echo This might indicate the .NET runtime is not properly embedded.
        echo Expected size: 90-120 MB for self-contained deployment.
        echo.
    )
)

echo.
echo ================================================================
echo BUILD COMPLETE!
echo ================================================================
echo.
echo Files created in Release folder:
dir /b "Release"
echo.
echo ✓ RssVisualizerScreensaver.exe - Main executable (self-contained)
echo ✓ Install.bat - Automated installer (run as administrator)
echo ✓ Documentation files
echo.
echo To deploy to another Windows system:
echo 1. Copy the entire Release folder
echo 2. Right-click Install.bat and "Run as administrator"
echo 3. OR manually copy RssVisualizerScreensaver.exe to C:\Windows\System32\RssVisualizerScreensaver.scr
echo.
echo The executable includes the .NET runtime and should work on any Windows 10/11 system
echo without requiring .NET to be separately installed.
echo.
pause