using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RssVisualizerScreensaver
{
    public sealed class AppConfig
    {
        public List<string> Feeds { get; set; } = new()
        {
            "https://apnews.com/apf-topnews?format=xml",
            "https://feeds.bbci.co.uk/news/rss.xml",
            "https://feeds.washingtonpost.com/rss/world",
            "https://feeds.npr.org/1001/rss.xml",
            "https://www.espn.com/espn/rss/news",
            "https://www.cbssports.com/rss/headlines/"
        };

        public int MaxItemsPerFeed { get; set; } = 24;
        public int RefreshMinutes { get; set; } = 15;
        public int ArticleDisplaySeconds { get; set; } = 12;
        public int MaxArticleAgeHours { get; set; } = 48;

        public int ForegroundFontSize { get; set; } = 42;
        public int BackgroundFontSize { get; set; } = 20;

        public string BackgroundGradientCss { get; set; } =
            "linear-gradient(180deg, #0b3d91 0%, #0a2a6b 50%, #061a44 100%)";

        public string ForegroundColorCss { get; set; } = "rgba(255,255,255,0.95)";
        public string BackgroundColumnColorCss { get; set; } = "rgba(255,255,255,0.30)";

        public double SpeedFactor { get; set; } = 1.0;

        public List<string> CustomStopWords { get; set; } = new()
        {
            "football", "college", "best", "state", "rankings", 
            "predictions", "basketball", "bets", "real", "odds",
            "picks", "week", "city", "team"
        };

        public static string ConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RssVisualizerScreensaver", "config.json");

        public static AppConfig Current { get; private set; } = Load();

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var cfg = JsonSerializer.Deserialize<AppConfig>(json);
                    if (cfg != null) return cfg;
                }
            }
            catch { /* ignore */ }

            var def = new AppConfig();
            Save(def);
            return def;
        }

        public static void Save(AppConfig cfg)
        {
            var dir = Path.GetDirectoryName(ConfigPath)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
            Current = cfg;
        }

        public SceneOptions ToSceneOptions() => new()
        {
            BackgroundGradientCss = BackgroundGradientCss,
            ForegroundColorCss = ForegroundColorCss,
            BackgroundColumnColorCss = BackgroundColumnColorCss,
            ForegroundFontSize = ForegroundFontSize,
            BackgroundFontSize = BackgroundFontSize,
            SpeedFactor = SpeedFactor,
            ArticleDisplaySeconds = ArticleDisplaySeconds,
            CustomStopWords = CustomStopWords
        };
    }

    public sealed class SceneOptions
    {
        public string BackgroundGradientCss { get; set; } = "";
        public string ForegroundColorCss { get; set; } = "";
        public string BackgroundColumnColorCss { get; set; } = "";
        public int ForegroundFontSize { get; set; }
        public int BackgroundFontSize { get; set; }
        public double SpeedFactor { get; set; } = 1.0;
        public int ArticleDisplaySeconds { get; set; } = 12;
        public List<string> CustomStopWords { get; set; } = new();
    }
}