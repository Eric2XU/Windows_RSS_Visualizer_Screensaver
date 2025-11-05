using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace RssVisualizerScreensaver
{
    public static class RssService
    {
        private static readonly HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10) // Increased back to 10 seconds for slower feeds
        };

        public static async Task<List<RssItem>> LoadAggregatedAsync(List<string> feeds, int maxPerSource)
        {
            Logger.Log("RssService", $"Loading {feeds.Count} feeds, max {maxPerSource} items per source");
            var tasks = feeds.Select(f => LoadFeedAsync(f, 50)); // Fetch more initially, will limit per source later
            var results = await Task.WhenAll(tasks);
            var all = results.SelectMany(r => r).ToList();
            Logger.Log("RssService", $"Retrieved {all.Count} total items from all feeds");

            var maxAge = AppConfig.Current.MaxArticleAgeHours;
            var cutoffDate = DateTime.UtcNow.AddHours(-maxAge);
            
            // Filter by age, deduplicate, then limit per source for balanced mixing
            var filtered = all
                .Where(i => i.Published >= cutoffDate)
                .GroupBy(i => (Title: i.Title.Trim(), Link: i.Link))
                .Select(g => g.First())
                .OrderByDescending(i => i.Published)
                .GroupBy(i => i.Source)
                .SelectMany(sourceGroup => sourceGroup.Take(maxPerSource)) // Limit items per source
                .OrderBy(x => Guid.NewGuid()) // Randomize for good mixing
                .ToList();

            Logger.Log("RssService", $"After age filter ({maxAge}h), dedup, and per-source limit ({maxPerSource}): {filtered.Count} items");
            return filtered;
        }

        private static async Task<List<RssItem>> LoadFeedAsync(string url, int perFeed)
        {
            Logger.Log("RssService", $"Loading feed: {url}");
            try
            {
                using var stream = await Client.GetStreamAsync(url);
                
                // Configure XmlReader to handle DTDs (required for some feeds like AP News)
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Parse,
                    MaxCharactersFromEntities = 1024,
                    XmlResolver = null // Don't fetch external resources
                };
                
                using var reader = XmlReader.Create(stream, settings);
                var feed = SyndicationFeed.Load(reader);
                if (feed == null)
                {
                    Logger.Log("RssService", $"Feed returned null: {url}");
                    return new List<RssItem>();
                }

                var items = feed.Items
                    .Select(x => new RssItem
                    {
                        Title = x.Title?.Text ?? "",
                        Summary = SanitizedSummary(x.Summary?.Text ?? ""),
                        Link = x.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                        Published = x.PublishDate.UtcDateTime != DateTime.MinValue
                            ? x.PublishDate.UtcDateTime
                            : DateTime.UtcNow,
                        ImageUrl = ExtractImageUrl(x),
                        Source = feed.Title?.Text ?? ExtractSourceFromUrl(url)
                    })
                    .Where(i => !string.IsNullOrWhiteSpace(i.Title))
                    .OrderByDescending(i => i.Published) // Sort by pubDate descending first
                    .Take(perFeed) // Then take top items
                    .ToList();
                
                Logger.Log("RssService", $"Loaded {items.Count} items from {url}");
                return items;
            }
            catch (Exception ex)
            {
                Logger.LogException("RssService", ex);
                Logger.Log("RssService", $"Failed to load feed: {url}");
                return new List<RssItem>();
            }
        }

        private static string SanitizedSummary(string html)
        {
            // Simple strip of tags; we only need a short blurb
            var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
            text = System.Net.WebUtility.HtmlDecode(text);
            text = text.Replace("\n", " ").Replace("\r", " ");
            return text.Length > 220 ? text[..220] + "…" : text;
        }

        private static string ExtractImageUrl(SyndicationItem item)
        {
            // Try to get image from media:thumbnail or media:content
            try
            {
                // Check element extensions for media namespace
                foreach (var ext in item.ElementExtensions)
                {
                    if (ext.OuterName == "thumbnail" || ext.OuterName == "content")
                    {
                        var reader = ext.GetReader();
                        var url = reader.GetAttribute("url");
                        if (!string.IsNullOrEmpty(url))
                            return url;
                    }
                }
                
                // Check for enclosures (like podcast images or article images)
                var imageEnclosure = item.Links.FirstOrDefault(l => 
                    l.RelationshipType == "enclosure" && 
                    l.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true);
                
                if (imageEnclosure != null)
                    return imageEnclosure.Uri.ToString();

                // Try to find image in content
                var content = item.Content as TextSyndicationContent;
                if (content != null)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        content.Text, 
                        @"<img[^>]+src=[""']([^""']+)[""']",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                        return match.Groups[1].Value;
                }

                // Try summary for image
                if (item.Summary != null)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        item.Summary.Text, 
                        @"<img[^>]+src=[""']([^""']+)[""']",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                        return match.Groups[1].Value;
                }
            }
            catch { }
            
            return "";
        }
        
        private static string ExtractSourceFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var host = uri.Host;
                // Remove www. prefix if present
                if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                    host = host.Substring(4);
                return host;
            }
            catch
            {
                return "Unknown Source";
            }
        }
    }

    public sealed class RssItem
    {
        public string Title { get; set; } = "";
        public string Summary { get; set; } = "";
        public string Link { get; set; } = "";
        public DateTime Published { get; set; }
        public string ImageUrl { get; set; } = "";
        public string Source { get; set; } = "";
    }
}