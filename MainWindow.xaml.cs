using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RssVisualizerScreensaver
{
    public partial class MainWindow : Window
    {
        private readonly bool _isPreview;
        private Point _lastMousePos;
        private DateTime _lastMove = DateTime.UtcNow;
        private bool _mouseInitialized = false;
        private Timer? _mouseCheckTimer;
        private Timer? _refreshTimer;

        public MainWindow(bool isPreview = false)
        {
            Logger.Log("MainWindow", $"Constructor called, isPreview={isPreview}");
            InitializeComponent();
            _isPreview = isPreview;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.Log("MainWindow", "Window_Loaded started");
            
            if (!_isPreview)
            {
                // Make it full screen on whichever monitor we're on
                WindowState = WindowState.Maximized;
                Logger.Log("MainWindow", "Window maximized");
            }

            try
            {
                Logger.Log("MainWindow", "Initializing WebView2");
                
                // Try multiple locations for WebView2 user data folder
                // Screensavers may run under restricted contexts (SYSTEM account, locked sessions)
                string? userDataFolder = null;
                string[] candidatePaths = new[]
                {
                    // Option 1: Current user's temp (may fail under SYSTEM)
                    System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RssVisualizerScreensaver_WebView2"),
                    
                    // Option 2: Windows temp root (more permissive)
                    System.IO.Path.Combine(Environment.GetEnvironmentVariable("TEMP") ?? @"C:\Windows\Temp", "RssVisualizerScreensaver_WebView2"),
                    
                    // Option 3: ProgramData (system-wide, accessible to SYSTEM)
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RssVisualizerScreensaver", "WebView2"),
                    
                    // Option 4: Local AppData (user-specific but more reliable)
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RssVisualizerScreensaver", "WebView2")
                };
                
                // Find first writable location
                foreach (var candidatePath in candidatePaths)
                {
                    try
                    {
                        Logger.Log("MainWindow", $"Trying WebView2 user data folder: {candidatePath}");
                        
                        // Create directory if it doesn't exist
                        var dirInfo = System.IO.Directory.CreateDirectory(candidatePath);
                        
                        // Test write permissions by creating and deleting a test file
                        var testFile = System.IO.Path.Combine(candidatePath, $"_test_{Guid.NewGuid()}.tmp");
                        System.IO.File.WriteAllText(testFile, "test");
                        System.IO.File.Delete(testFile);
                        
                        userDataFolder = candidatePath;
                        Logger.Log("MainWindow", $"WebView2 user data folder selected: {userDataFolder}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("MainWindow", $"Cannot use {candidatePath}: {ex.Message}");
                    }
                }
                
                if (userDataFolder == null)
                {
                    throw new InvalidOperationException("No writable location found for WebView2 user data. Tried: " + string.Join(", ", candidatePaths));
                }
                
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await Web.EnsureCoreWebView2Async(env);
                
                Logger.Log("MainWindow", "WebView2 initialized");
                
                // Set background color to match our blue gradient
                Web.DefaultBackgroundColor = System.Drawing.Color.FromArgb(255, 11, 61, 145); // #0b3d91

                // Add navigation event handlers for debugging
                Web.CoreWebView2.NavigationStarting += (s, e) =>
                {
                    Logger.Log("WebView2", $"NavigationStarting: {e.Uri}");
                };
                
                Web.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    Logger.Log("WebView2", $"NavigationCompleted: Success={e.IsSuccess}, HttpStatusCode={e.HttpStatusCode}, WebErrorStatus={e.WebErrorStatus}");
                };
                
                Web.CoreWebView2.DOMContentLoaded += (s, e) =>
                {
                    Logger.Log("WebView2", "DOMContentLoaded");
                };

                // Disable context menus and hotkeys
                Web.CoreWebView2.Settings.AreDevToolsEnabled = false;
                Web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                Web.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                
                Logger.Log("MainWindow", "WebView2 settings configured");

                // Add event handlers to WebView2 to ensure they're captured
                if (!_isPreview)
                {
                    Web.MouseMove += Window_MouseMove;
                    Web.MouseDown += Window_MouseDown;
                    Web.PreviewMouseDown += Window_MouseDown;
                    Web.PreviewMouseMove += Window_MouseMove;
                    
                    // Use WebMessageReceived to get messages from JavaScript
                    Web.CoreWebView2.WebMessageReceived += (s, e) =>
                    {
                        var message = e.TryGetWebMessageAsString();
                        Logger.Log("WebView2", $"Message from JavaScript: {message}");
                        if (message == "EXIT")
                        {
                            Logger.Log("MainWindow", "Exit requested from JavaScript");
                            Application.Current.Shutdown();
                        }
                    };
                    
                    Logger.Log("MainWindow", "WebView2 input handlers attached");
                }

                Logger.Log("MainWindow", "Loading scene");
                await LoadSceneAsync();
                Logger.Log("MainWindow", "Scene loaded");

                // Initialize mouse position after a brief delay
                if (!_isPreview)
                {
                    await Task.Delay(500);
                    _lastMousePos = Mouse.GetPosition(this);
                    _mouseInitialized = false;
                    Logger.Log("MainWindow", $"Mouse tracking initialized at: {_lastMousePos.X}, {_lastMousePos.Y}");
                    
                    // Start polling timer to check mouse position (WebView2 can block events)
                    _mouseCheckTimer = new Timer(_ => CheckMousePosition(), null, 
                        TimeSpan.FromMilliseconds(100), 
                        TimeSpan.FromMilliseconds(100));
                    Logger.Log("MainWindow", "Mouse polling timer started");
                }

                // Periodic refresh of feeds
                _refreshTimer = new Timer(async _ => await RefreshDataAsync(), null,
                    TimeSpan.FromMinutes(AppConfig.Current.RefreshMinutes),
                    TimeSpan.FromMinutes(AppConfig.Current.RefreshMinutes));
                Logger.Log("MainWindow", "Refresh timer started");
            }
            catch (Exception ex)
            {
                Logger.LogException("MainWindow", ex);
                throw;
            }
        }

        private void CheckMousePosition()
        {
            if (_isPreview) return;
            
            try
            {
                Dispatcher.Invoke(() =>
                {
                    var pos = Mouse.GetPosition(this);
                    
                    // First check just initializes position
                    if (!_mouseInitialized)
                    {
                        _lastMousePos = pos;
                        _mouseInitialized = true;
                        return;
                    }
                    
                    // Check for movement
                    var deltaX = Math.Abs(pos.X - _lastMousePos.X);
                    var deltaY = Math.Abs(pos.Y - _lastMousePos.Y);
                    
                    if (deltaX > 5 || deltaY > 5)
                    {
                        Logger.Log("MainWindow", $"Mouse movement detected in poll: deltaX={deltaX}, deltaY={deltaY}, exiting");
                        Application.Current.Shutdown();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogException("CheckMousePosition", ex);
            }
        }

        private async Task LoadSceneAsync()
        {
            Logger.Log("LoadScene", "Starting RSS aggregation");
            var items = await RssService.LoadAggregatedAsync(AppConfig.Current.Feeds, AppConfig.Current.MaxItemsPerFeed);
            Logger.Log("LoadScene", $"Loaded {items.Count} RSS items");
            
            // Log source distribution for debugging word cloud
            var sourceGroups = items.GroupBy(x => x.Source).OrderByDescending(g => g.Count());
            Logger.Log("LoadScene", "Article distribution by source:");
            foreach (var group in sourceGroups)
            {
                Logger.Log("LoadScene", $"  {group.Key}: {group.Count()} articles");
            }
            
            var payload = new ScenePayload
            {
                Items = items,
                Options = AppConfig.Current.ToSceneOptions()
            };

            Logger.Log("LoadScene", "Building HTML");
            string html = SceneBuilder.BuildHtml(payload);
            Logger.Log("LoadScene", $"HTML length: {html.Length} characters");
            Logger.Log("LoadScene", $"First 200 chars: {(html.Length > 200 ? html.Substring(0, 200) : html)}");
            
            Logger.Log("LoadScene", "Navigating WebView to HTML");
            Web.NavigateToString(html);
        }

        private async Task RefreshDataAsync()
        {
            try
            {
                var items = await RssService.LoadAggregatedAsync(AppConfig.Current.Feeds, AppConfig.Current.MaxItemsPerFeed);
                var jsonOptions = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                };
                var json = JsonSerializer.Serialize(items, jsonOptions);
                await Dispatcher.InvokeAsync(async () =>
                {
                    if (Web?.CoreWebView2 != null)
                    {
                        string script = $"window.__rssUpdate({json});";
                        await Web.ExecuteScriptAsync(script);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_isPreview) return;
            Logger.Log("MainWindow", $"Key pressed: {e.Key}, exiting");
            Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isPreview) return;
            Logger.Log("MainWindow", $"Mouse button pressed: {e.ChangedButton}, exiting");
            Application.Current.Shutdown();
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isPreview) return;
            
            var pos = e.GetPosition(this);
            
            // First movement just initializes position
            if (!_mouseInitialized)
            {
                _lastMousePos = pos;
                _mouseInitialized = true;
                _lastMove = DateTime.UtcNow;
                Logger.Log("MainWindow", $"Mouse position initialized: {pos.X}, {pos.Y}");
                return;
            }
            
            // Any significant mouse movement should exit
            var deltaX = Math.Abs(pos.X - _lastMousePos.X);
            var deltaY = Math.Abs(pos.Y - _lastMousePos.Y);
            
            if (deltaX > 5 || deltaY > 5)
            {
                Logger.Log("MainWindow", $"Mouse movement detected: deltaX={deltaX}, deltaY={deltaY}, exiting");
                Application.Current.Shutdown();
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            _mouseCheckTimer?.Dispose();
            _refreshTimer?.Dispose();
            
            if (!_isPreview)
            {
                Application.Current.Shutdown();
            }
        }
    }

    public sealed class ScenePayload
    {
        public List<RssItem> Items { get; set; } = new();
        public SceneOptions Options { get; set; } = new();
    }
}