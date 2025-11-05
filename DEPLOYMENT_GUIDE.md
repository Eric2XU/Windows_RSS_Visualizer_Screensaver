# RSS Visualizer Screensaver - Deployment Guide

## The "CoreCLR Path" Error Fix

If you encounter this error on target systems:
```
Could not resolve CoreCLR path. For more details, enable tracing by setting COREHOST_TRACE environment variable to 1
```

This means the .NET runtime is missing or the application wasn't properly published as self-contained.

## Solution 1: Use the Properly Published Version

1. **Build the self-contained version** (if you're the developer):
   ```bash
   dotnet publish RssVisualizerScreensaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true
   ```

2. **Use the executable from**: `bin\Release\net8.0-windows\win-x64\publish\RssVisualizerScreensaver.exe`

3. **This version should be ~90-120MB** (includes .NET runtime)

## Solution 2: Install .NET Runtime on Target System

If you prefer a smaller deployment:

1. Install .NET 8.0 Runtime on the target system:
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Install "Desktop Runtime" (includes Windows Forms/WPF support)

2. Use the framework-dependent version (much smaller, ~5-10MB)

## Solution 3: For System Administrators

For deployment across multiple systems:

### Option A: Self-Contained (Recommended)
- Pros: No .NET installation required on target systems
- Cons: Larger file size (~100MB)
- Best for: Mixed environments, systems without admin access

### Option B: Framework-Dependent
- Pros: Much smaller deployment
- Cons: Requires .NET 8.0 Runtime on all target systems
- Best for: Controlled environments where you can ensure .NET is installed

## Deployment Steps

### For Self-Contained Deployment:

1. **Publish the application**:
   ```bash
   dotnet publish RssVisualizerScreensaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true
   ```

2. **Copy the published executable**:
   - Source: `bin\Release\net8.0-windows\win-x64\publish\RssVisualizerScreensaver.exe`
   - Destination: `C:\Windows\System32\RssVisualizerScreensaver.scr`

3. **Use the provided Install.bat** (recommended):
   ```batch
   @echo off
   echo Installing RSS Visualizer Screensaver...
   copy "RssVisualizerScreensaver.exe" "C:\Windows\System32\RssVisualizerScreensaver.scr"
   echo Installation complete!
   echo Go to Settings → Personalization → Lock screen → Screen saver to configure.
   pause
   ```

### For Framework-Dependent Deployment:

1. **Ensure .NET 8.0 Runtime is installed** on target systems
2. **Publish without self-contained**:
   ```bash
   dotnet publish RssVisualizerScreensaver.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
   ```
3. **Deploy the smaller executable**

## Verification

After installation, verify the screensaver works:

1. **Test the executable directly**:
   ```cmd
   "C:\Windows\System32\RssVisualizerScreensaver.scr" /s
   ```

2. **Check Windows screensaver settings**:
   - Should appear in the dropdown as "RssVisualizerScreensaver"

3. **Test configuration**:
   ```cmd
   "C:\Windows\System32\RssVisualizerScreensaver.scr" /c
   ```

## Troubleshooting

### Still getting CoreCLR errors?

1. **Enable detailed tracing**:
   ```cmd
   set COREHOST_TRACE=1
   "C:\Windows\System32\RssVisualizerScreensaver.scr" /s
   ```

2. **Check the executable properties**:
   - Self-contained version should be ~90-120MB
   - Framework-dependent version should be ~5-10MB

3. **Verify .NET installation** (for framework-dependent):
   ```cmd
   dotnet --list-runtimes
   ```
   Should show: `Microsoft.WindowsDesktop.App 8.0.x`

### WebView2 Issues

If you get WebView2 errors:
1. Install WebView2 Runtime from Microsoft
2. Or use the embedded WebView2 version (already handled in our build)

## Build Script for Developers

Save this as `build-release.bat`:

```batch
@echo off
echo Building RSS Visualizer Screensaver for deployment...

REM Clean previous builds
dotnet clean RssVisualizerScreensaver.csproj -c Release

REM Build self-contained version
echo Building self-contained version...
dotnet publish RssVisualizerScreensaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true

REM Copy to Release folder
if not exist "Release" mkdir Release
copy "bin\Release\net8.0-windows\win-x64\publish\RssVisualizerScreensaver.exe" "Release\"
copy "README.md" "Release\"
copy "INSTALL.txt" "Release\"
copy "RELEASE_NOTES.txt" "Release\"

echo.
echo Build complete! Files are in the Release folder.
echo The RssVisualizerScreensaver.exe is self-contained and includes the .NET runtime.
echo File size should be approximately 90-120MB.
pause
```

## System Requirements

- **Windows 10 or 11** (64-bit)
- **WebView2 Runtime** (usually pre-installed)
- **No .NET installation required** (when using self-contained deployment)
- **~100MB disk space** for the screensaver
- **Internet connection** for RSS feeds

## Security Notes

- The screensaver runs in the Windows screensaver security context
- WebView2 is configured with restricted permissions
- No external executables are launched
- Configuration is stored in user's AppData folder
- Network access is limited to RSS feed URLs only