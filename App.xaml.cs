using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace RssVisualizerScreensaver
{
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.Init();
            Logger.Log("App", "Application starting");
            Logger.Log("App", $"Arguments: {string.Join(" ", e.Args)}");

            var args = e.Args.Select(a => a.Trim().ToLowerInvariant()).ToArray();
            if (args.Length == 0 || args[0] == "/s" || args[0] == "-s")
            {
                Logger.Log("App", "Starting full-screen mode");
                // Full-screen run
                var win = new MainWindow();
                win.Show();
            }
            else if ((args[0] == "/c" || args[0] == "-c"))
            {
                // Config/options
                var dlg = new OptionsWindow();
                dlg.Owner = null;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dlg.ShowDialog();
                Shutdown();
            }
            else if ((args[0] == "/p" || args[0] == "-p") && args.Length >= 2)
            {
                // Preview (hosted inside tiny window from Screen Saver dialog)
                if (long.TryParse(args[1], out long handle))
                {
                    var preview = new MainWindow(isPreview: true);
                    preview.WindowStyle = WindowStyle.None;
                    preview.ResizeMode = ResizeMode.NoResize;
                    preview.ShowInTaskbar = false;
                    preview.ShowActivated = false;
                    preview.Show();
                    var interop = new System.Windows.Interop.WindowInteropHelper(preview);
                    SetParent(interop.Handle, new IntPtr(handle));
                }
                else
                {
                    Shutdown();
                }
            }
            else
            {
                // Unknown arg -> open config
                var dlg = new OptionsWindow();
                dlg.ShowDialog();
                Shutdown();
            }
        }
    }
}