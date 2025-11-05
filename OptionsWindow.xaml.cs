using System;
using System.Linq;
using System.Windows;

namespace RssVisualizerScreensaver
{
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
            LoadUi();
        }

        private void LoadUi()
        {
            var cfg = AppConfig.Current;
            FeedsBox.Text = string.Join(Environment.NewLine, cfg.Feeds);
            FgFontSizeBox.Text = cfg.ForegroundFontSize.ToString();
            BgFontSizeBox.Text = cfg.BackgroundFontSize.ToString();
            ItemsPerFeedBox.Text = cfg.MaxItemsPerFeed.ToString();
            RefreshMinutesBox.Text = cfg.RefreshMinutes.ToString();
            ArticleDisplayTimeBox.Text = cfg.ArticleDisplaySeconds.ToString();
            MaxArticleAgeBox.Text = cfg.MaxArticleAgeHours.ToString();
            BgGradientBox.Text = cfg.BackgroundGradientCss;
            FgColorBox.Text = cfg.ForegroundColorCss;
            BgColumnColorBox.Text = cfg.BackgroundColumnColorCss;
            SpeedFactorBox.Text = cfg.SpeedFactor.ToString("0.##");
            CustomStopWordsBox.Text = string.Join(", ", cfg.CustomStopWords);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var cfg = AppConfig.Current;

            cfg.Feeds = FeedsBox.Text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (int.TryParse(FgFontSizeBox.Text, out var fgSize)) cfg.ForegroundFontSize = fgSize; else cfg.ForegroundFontSize = 42;
            if (int.TryParse(BgFontSizeBox.Text, out var bgSize)) cfg.BackgroundFontSize = bgSize; else cfg.BackgroundFontSize = 20;
            if (int.TryParse(ItemsPerFeedBox.Text, out var maxItems)) cfg.MaxItemsPerFeed = maxItems; else cfg.MaxItemsPerFeed = 5;
            if (int.TryParse(RefreshMinutesBox.Text, out var refreshMin)) cfg.RefreshMinutes = refreshMin; else cfg.RefreshMinutes = 15;
            if (int.TryParse(ArticleDisplayTimeBox.Text, out var displaySec)) cfg.ArticleDisplaySeconds = displaySec; else cfg.ArticleDisplaySeconds = 12;
            if (int.TryParse(MaxArticleAgeBox.Text, out var maxAge)) cfg.MaxArticleAgeHours = maxAge; else cfg.MaxArticleAgeHours = 48;

            cfg.BackgroundGradientCss = string.IsNullOrWhiteSpace(BgGradientBox.Text)
                ? cfg.BackgroundGradientCss : BgGradientBox.Text.Trim();
            cfg.ForegroundColorCss = string.IsNullOrWhiteSpace(FgColorBox.Text)
                ? cfg.ForegroundColorCss : FgColorBox.Text.Trim();
            cfg.BackgroundColumnColorCss = string.IsNullOrWhiteSpace(BgColumnColorBox.Text)
                ? cfg.BackgroundColumnColorCss : BgColumnColorBox.Text.Trim();

            if (double.TryParse(SpeedFactorBox.Text, out var speedFactor)) cfg.SpeedFactor = speedFactor; else cfg.SpeedFactor = 1.0;

            // Parse custom stop words
            cfg.CustomStopWords = CustomStopWordsBox.Text
                .Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            AppConfig.Save(cfg);
            DialogResult = true;
            Close();
        }

        private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Restore all settings to default values?", "Restore Defaults", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var defaults = new AppConfig();
                AppConfig.Save(defaults);
                LoadUi();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}