# RSS Visualizer Screensaver for Windows# RSS Visualizer Screensaver v1.0.0



[![Version](https://img.shields.io/badge/version-1.1.0-blue.svg)](https://github.com/Eric2XU/Windows_RSS_Visualizer_Screensaver/releases)A modern Windows screensaver that displays live RSS news headlines in an elegant, ambient visualization inspired by Apple's legacy RSS Visualizer.

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)

[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-lightgrey.svg)](https://www.microsoft.com/windows)## Features



A modern Windows screensaver that displays live RSS news headlines in an elegant, ambient visualization inspired by Apple's legacy RSS Visualizer.- 📰 **Live RSS Feed Display** - Shows headlines from multiple RSS/Atom feeds

- 🎨 **Beautiful Design** - Cool blue gradient with translucent glass cards

## ✨ Features- ☁️ **Word Cloud** - Dynamic word cloud showing trending topics

- 🎯 **Smart Content** - Source-balanced article selection and age filtering

- 📰 **Live RSS/Atom Feeds** - Display headlines from multiple news sources- ⚙️ **Fully Configurable** - Customize feeds, timing, colors, and appearance

- 🎨 **Beautiful Design** - Cool blue gradient with translucent glass cards- 🚫 **Custom Word Filters** - Exclude specific words from the word cloud

- ☁️ **Dynamic Word Cloud** - Trending topics visualization with smart word filtering

- 🎯 **Smart Content** - Source-balanced article selection and age filtering## Installation

- ⚙️ **Fully Configurable** - Customize feeds, timing, colors, and appearance

- 🔒 **Secure** - Works under all Windows security contexts (SYSTEM account, locked sessions)### Quick Install (Recommended)

- 🚫 **Custom Filters** - Exclude specific words from word cloud

1. **Copy the screensaver file**:

## 🎬 What's New in v1.1.0   - Copy `RssVisualizerScreensaver.exe` to `C:\Windows\System32\` (requires admin)

   - Or keep it anywhere and right-click → "Install"

🐛 **Critical Fix**: Resolved WebView2 initialization issues when running under restricted security contexts

- Implements multi-location fallback for WebView2 user data folder2. **Set as screensaver**:

- Tests write permissions before selecting location   - Right-click Desktop → Personalize → Lock screen → Screen saver settings

- Fixes "Access is denied (0x80070005)" error   - Select "RSS Visualizer Screensaver" from the dropdown

- Now works reliably when launched from OS screensaver settings   - Click "Settings" to configure feeds and options

   - Click "Preview" to test

[View Full Changelog](RELEASE_NOTES.txt)

### Manual Installation

## 📦 Installation

1. Copy `RssVisualizerScreensaver.exe` to your desired location

### Quick Install (Recommended)2. Rename it to `RssVisualizerScreensaver.scr` (optional, for screensaver mode)

3. Right-click the file and select "Install"

1. **Download the latest release**

   - Download `RSS_Visualizer_v1.1.0.zip` from [Releases](https://github.com/Eric2XU/Windows_RSS_Visualizer_Screensaver/releases)## Usage

   - Extract to a folder

### Command Line Options

2. **Run the installer**

   - Right-click `Install.bat`- `/s` - Run screensaver (full-screen mode)

   - Select "Run as administrator"- `/p [hwnd]` - Preview mode (in settings dialog)

   - Follow the prompts- `/c` - Configuration dialog



3. **Configure**### Configuration

   - Open Windows Settings → Personalization → Lock screen → Screen saver

   - Select "RssVisualizerScreensaver" from dropdownClick "Settings" in the Windows screensaver settings to configure:

   - Click "Settings" to customize feeds (optional)

   - Click "Preview" to test**RSS Feeds**:

- Add/remove RSS or Atom feed URLs

### Manual Installation- Default feeds: AP News, BBC, Washington Post, NPR, ESPN, CBS Sports



1. Copy `RssVisualizerScreensaver.exe` to `C:\Windows\System32\`**Timing**:

2. Rename to `RssVisualizerScreensaver.scr`- Article display time (default: 12 seconds)

3. Configure via Windows screensaver settings- Feed refresh interval (default: 15 minutes)

- Maximum article age (default: 48 hours)

**Note**: The `.scr` extension is required for Windows to recognize it as a screensaver.

**Content**:

[Detailed Installation Guide](INSTALL.txt)- Items per feed (default: 24)

- Animation speed multiplier (default: 1.0)

## 🚀 Usage

**Word Cloud**:

### Command Line Options- Custom excluded words (comma-separated)

- Default exclusions: sports terms, generic words

```powershell

RssVisualizerScreensaver.exe /s    # Run screensaver (full-screen)**Appearance**:

RssVisualizerScreensaver.exe /p    # Preview mode (in settings dialog)- Font sizes for main card and background

RssVisualizerScreensaver.exe /c    # Configuration dialog- Background gradient CSS

```- Text colors (CSS format)



### Default Configuration## Default Settings



The screensaver comes pre-configured with:The screensaver comes pre-configured with:



**Feeds** (6 sources):- 6 news feeds (3 general news + 3 sports)

- AP News Top Stories- 24 articles per feed for balanced content mix

- BBC News- 48-hour article freshness window

- Washington Post World- Word cloud with 14 custom excluded words

- NPR News- Cool blue gradient background

- ESPN Sports- 12-second article display time

- CBS Sports Headlines

## Configuration File

**Settings**:

- 24 articles per feedSettings are stored in:

- 12 second display per article```

- 15 minute refresh interval%APPDATA%\RssVisualizerScreensaver\config.json

- 48 hour article age limit```



## 🛠️ Building from SourceYou can edit this file directly or use the Settings dialog.



### Prerequisites## Technical Details



- Visual Studio 2022 or later- **Framework**: .NET 8.0 Windows

- .NET 8.0 SDK- **Rendering**: WebView2 (HTML/CSS/JavaScript)

- Windows 10/11 SDK- **RSS Parsing**: System.ServiceModel.Syndication with DTD support

- **Self-Contained**: No .NET runtime installation required

### Build Steps

## Troubleshooting

```powershell

# Clone the repository**Screensaver doesn't start**:

git clone https://github.com/Eric2XU/Windows_RSS_Visualizer_Screensaver.git- Ensure the file is named `.scr` or use the `/s` flag

cd Windows_RSS_Visualizer_Screensaver- Check Windows Event Viewer for errors

- Review logs at `C:\Windows\Temp\rss_viz.log`

# Build Release version

dotnet build -c Release**Feeds not loading**:

- Check your internet connection

# Output will be in: bin\Release\net8.0-windows\win-x64\- Verify feed URLs are accessible

```- Some feeds may require specific user agents or have rate limits

- Check the log file for feed-specific errors

### Project Structure

**Word cloud shows unwanted words**:

```- Open Settings → Word Cloud

Windows_RSS_Visualizer_Screensaver/- Add words to exclude (comma-separated)

├── App.xaml                      # WPF application definition- Words are case-insensitive

├── App.xaml.cs                   # Application logic

├── MainWindow.xaml               # Main screensaver window**Performance issues**:

├── MainWindow.xaml.cs            # Screensaver logic & WebView2 init- Reduce Items per feed (try 10-15 instead of 24)

├── OptionsWindow.xaml            # Configuration dialog- Increase refresh interval

├── OptionsWindow.xaml.cs         # Configuration logic- Reduce animation speed to 0.5 or 0.75

├── AppConfig.cs                  # Configuration management

├── RssService.cs                 # RSS/Atom feed parsing## Exit Screensaver

├── SceneBuilder.cs               # HTML/CSS/JS scene generation

├── Logger.cs                     # Logging utility- Move mouse

├── RssVisualizerScreensaver.csproj  # Project file- Click anywhere

├── Release/                      # Distribution files- Press any key

│   ├── RssVisualizerScreensaver.exe- Touch screen (if touchscreen enabled)

│   ├── Install.bat

│   └── Documentation## Requirements

└── .github/

    └── copilot-instructions.md   # Development guidelines- Windows 10/11 (64-bit)

```- WebView2 Runtime (usually pre-installed on Windows 11)

- Internet connection for RSS feeds

## 🔧 Technical Details

## Version History

**Framework**: .NET 8.0 Windows (WPF)  

**Rendering**: WebView2 (HTML/CSS/JavaScript)  ### 1.0.0 (November 2025)

**RSS Parsing**: System.ServiceModel.Syndication  - Initial release

**Deployment**: Self-contained, single-file  - RSS/Atom feed support with DTD parsing

**Size**: ~9.2 MB executable- Dynamic word cloud with source balancing

- Configurable settings UI

## 🐛 Known Issues- Custom stop words for word cloud

- Per-source article limiting

- ESPN feed may fail due to DateTime parsing issues (non-critical)- Random card positioning

- AP News feed may fail due to format incompatibilities (non-critical)- Mouse/keyboard exit detection

- Other feeds continue to work normally- Article age filtering

- Background scrolling headlines

## 📝 Configuration Files

## License

- **Settings**: `%APPDATA%\RssVisualizerScreensaver\config.json`

- **Logs**: `C:\Windows\Temp\rss_viz.log`Created as a faithful recreation of Apple's legacy RSS Visualizer for Windows.

- **WebView2 Data**: Multiple possible locations (logged in rss_viz.log)

## Credits

## 📄 License

Design and motion inspired by the original Apple RSS Visualizer screensaver.

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Inspired by Apple's legacy RSS Visualizer screensaver
- Built with [WebView2](https://developer.microsoft.com/microsoft-edge/webview2/)
- Uses [System.ServiceModel.Syndication](https://www.nuget.org/packages/System.ServiceModel.Syndication/)

## 📊 System Requirements

- Windows 10 or Windows 11 (64-bit)
- WebView2 Runtime (pre-installed on modern Windows)
- Internet connection
- ~50MB disk space

---

**Version**: 1.1.0  
**Release Date**: November 5, 2025  
**Copyright**: © 2025
