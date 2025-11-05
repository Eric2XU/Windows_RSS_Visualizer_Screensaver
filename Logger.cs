using System;
using System.IO;

namespace RssVisualizerScreensaver
{
    public static class Logger
    {
        private static readonly string LogPath = @"C:\Windows\Temp\rss_viz.log";
        private static readonly object LockObj = new object();

        public static void Init()
        {
            try
            {
                lock (LockObj)
                {
                    // Overwrite log file each time
                    File.WriteAllText(LogPath, $"=== RSS Visualizer Log Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
                }
            }
            catch { }
        }

        public static void Log(string subsystem, string message)
        {
            try
            {
                lock (LockObj)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logLine = $"[{timestamp}] [{subsystem}] {message}\n";
                    File.AppendAllText(LogPath, logLine);
                }
            }
            catch { }
        }

        public static void LogException(string subsystem, Exception ex)
        {
            Log(subsystem, $"EXCEPTION: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
