# copilot-instructions.md

**Scope**\
This file defines how to implement a faithful clone of Apple's legacy
"RSS Visualizer" screen saver for Windows. It captures the visual
language, motion, layout, and content rules that the original used, and
maps them to this project's architecture.

Use this as the single source of truth. Do not invent new behaviors
unless flagged here as an optional enhancement.

## 1. Product intent

Render live RSS headlines in a calm, data-forward ambient scene. The
original had a signature look: cool blue background, translucent glass
card for the active story, and columns of drifting headlines in the
background. The feeling should be quiet, readable, and slightly three
dimensional.

## 2. Visual model

-   **Background**
    -   A cool blue gradient backdrop. Example stop values: `#0b3d91` to
        `#061a44` vertically.\
    -   No texture, no vignette by default. Subtle is the point.
-   **Background columns**
    -   3 to 4 columns of small headline text.\
    -   Columns fill the viewport with comfortable gutters.\
    -   Headlines in columns are single line, ellipsized, and low
        opacity.\
    -   Columns scroll continuously upward at slightly different speeds
        to create depth.
-   **Foreground article card**
    -   A centered, translucent card that features one story at a time.\
    -   Card includes title, a short summary or excerpt, and metadata
        line with timestamp and link.\
    -   The card eases in with a small Z or X rotation and settles
        flat.\
    -   The previous card fades out and slightly translates up while the
        next fades in.
-   **Color and contrast**
    -   Background text is light on dark: white with \~0.3 opacity and a
        faint shadow for legibility.\
    -   Foreground card text is near-white with higher opacity.\
    -   Card surface uses a subtle blur and translucency.

## 3. Layout details

-   **Safe areas**
    -   Card width target: min(960 px, 80% of viewport).\
    -   Card vertical anchor: around 60% to 70% of viewport height.\
    -   Page padding for columns: about 6% of viewport width and 8% of
        viewport height.
-   **Typography**
    -   System UI family on Windows.\
    -   Background columns font size: \~18--22 px.\
    -   Foreground title font size: \~38--46 px.\
    -   Foreground summary font size: title size minus \~18 px, but not
        less than 16 px.\
    -   Headline letter spacing normal. No fancy ligatures.\
    -   Text shadows are subtle to avoid glow.
-   **Ellipsizing and truncation**
    -   Background column items are a single line with ellipsis.\
    -   Foreground summary is 2--4 lines max. Clamp and add ellipsis.\
    -   Do not hard wrap long words. Let the browser break them.

## 4. Motion and timing

-   **Background column scroll**
    -   Infinite upward loop per column.\
    -   Slightly different durations per column to avoid
        synchronization.\
    -   Example baseline: 40 seconds per loop. Multiply by per-column
        multipliers 1.0, 1.08, 0.94, 1.15.\
    -   Motion is linear to minimize visual "pumping."
-   **Card transitions**
    -   Show a new item every 6 seconds by default.\
    -   In animation: 0.9 s with ease-out-like curve. Translate Y from
        +40 px to 0. Rotate X from 9 degrees to 0. Fade 0 to 1.\
    -   Out animation: 0.65 s ease-in. Translate Y 0 to −30 px. Rotate X
        0 to −6 degrees. Fade 1 to 0.\
    -   Never animate scale. It feels cheap.
-   **Global speed factor**
    -   A numeric multiplier 0.5 to 2.0 applied to both the column
        scroll and the card timer.

## 5. Content rules

-   **Feed inputs**
    -   Accept any RSS or Atom. Normalize into
        `{ title, summary, link, published }`.\
    -   Default feeds in config should be well known and public.
        Suggested defaults:
        -   BBC Top Stories: `https://feeds.bbci.co.uk/news/rss.xml`
        -   CNN Top Stories: `http://rss.cnn.com/rss/edition.rss`
        -   Washington Post World:
            `https://feeds.washingtonpost.com/rss/world`
-   **Selection and ordering**
    -   Aggregate up to 10 items per feed by default.\
    -   De-duplicate by `(normalized title, link)`.\
    -   Sort by published date descending. If missing, treat as "now" to
        avoid starving such items.\
    -   Keep a working set of about 50--60 items so the scene feels
        rich.
-   **Summary text**
    -   Strip tags. Decode HTML entities.\
    -   Trim whitespace. Clamp to \~220 characters for the foreground
        summary.
-   **Attribution and links**
    -   The foreground card should include a clickable link labeled
        "open."\
    -   Do not display publisher logos unless provided in feed metadata
        and an explicit config flag enables it.

## 6. Interaction

-   Any keyboard press or mouse move beyond a minimal jitter threshold
    should exit the saver.\
-   No click actions on background column items. Only the foreground
    card exposes a link and only in environments where clicks are
    permitted.\
-   No right-click menus. No dev tools. No drag.

## 7. Configuration model

Backed by `AppConfig` in
`%APPDATA%\RssVisualizerScreensaver\config.json`.

``` json
{
  "feeds": [
    "https://feeds.bbci.co.uk/news/rss.xml",
    "http://rss.cnn.com/rss/edition.rss",
    "https://feeds.washingtonpost.com/rss/world"
  ],
  "maxItemsPerFeed": 10,
  "refreshMinutes": 15,
  "foregroundFontSize": 42,
  "backgroundFontSize": 20,
  "backgroundGradientCss": "linear-gradient(180deg, #0b3d91 0%, #0a2a6b 50%, #061a44 100%)",
  "foregroundColorCss": "rgba(255,255,255,0.95)",
  "backgroundColumnColorCss": "rgba(255,255,255,0.30)",
  "speedFactor": 1.0
}
```

Options dialog must allow editing every field above and validate numeric
ranges.

## 8. Architecture mapping

-   **Rendering layer**
    -   WebView scene with inline HTML, CSS and JS for animation and
        layout.\
    -   All CSS and JS are embedded via raw string literals and token
        replacement to avoid C# escaping bugs.
-   **Data layer**
    -   `System.ServiceModel.Syndication` to parse both RSS and Atom.\
    -   Normalize dates to UTC.\
    -   Simple in-memory cache. Refresh the list on a timer and push
        updates to the scene via `window.__rssUpdate(items)`.
-   **Performance**
    -   Cap refresh to every 15 minutes by default to be a good
        citizen.\
    -   Keep FPS reasonable. CSS animations are GPU accelerated by
        WebView2.\
    -   Avoid image fetching by default to prevent heavy network usage.
-   **Power behavior**
    -   Optional: throttle animation timer when on battery.\
    -   Optional: pause background column scroll when minimized or not
        visible.

## 9. Accessibility and legibility

-   Minimum text sizes are enforced.\
-   Color contrast for foreground card meets readable contrast against
    the blue gradient.\
-   Avoid rapid flashing or heavy motion. The motion here is continuous
    and gentle.

## 10. Edge cases and fallbacks

-   If a feed is unreachable, keep previous items and log the failure
    silently.\
-   If a feed provides no summary, show only the title on the card and
    still rotate normally.\
-   If there are fewer than 8 total items, repeat items but do not show
    the same item twice in a row.\
-   If all feeds are empty, show a static "No items available" card and
    keep background columns running with placeholder bullets.

## 11. Fidelity checklist vs original

-   Blue gradient background\
-   White-on-blue typography\
-   3--4 columns of small headlines drifting upward at different speeds\
-   Foreground card that fades and eases with a slight perspective tilt\
-   Clickable link on the active story only\
-   Simple feed URL input via options\
-   No images required\
-   No sound\
-   No interactive overlays

## 12. Testing scenarios

-   **Functional**
    -   Add a new feed and verify items appear and rotate.\
    -   Corrupt one feed URL and confirm others still display.\
    -   Verify mouse movement exits saver after threshold.
-   **Layout**
    -   Resize preview window. Check columns and card reflow.\
    -   Very long titles show ellipsis in columns and wrap gracefully on
        the card.
-   **Motion**
    -   Verify card interval obeys `speedFactor`.\
    -   Confirm columns have different scroll durations.
-   **Performance**
    -   CPU use stays low while idle.\
    -   Memory does not grow unchecked after repeated refreshes.

## 13. Non-goals

-   No image thumbnails.\
-   No filtering by keyword.\
-   No offline read.\
-   No transitions that scale the card.\
-   No heavy particle effects or extra ornaments.

## 14. Implementation notes

-   WebView2 must have dev tools and context menus disabled.\
-   Use raw strings in C# for the HTML, CSS and JS, then replace tokens
    at runtime.\
-   De-duplicate items by normalized title and link to avoid jitter.\
-   Decode HTML entities with `System.Net.WebUtility.HtmlDecode`.\
-   When dates are missing, set `published = DateTime.UtcNow` to keep
    the queue moving.

## 15. Future optional enhancements

-   Configurable theme presets that stay faithful to the original
    palette.\
-   Optional publisher label on the card.\
-   Smooth pause on power saver mode.\
-   Multi-monitor support with synchronized but offset scenes.

------------------------------------------------------------------------

**Copilot usage hints**

-   When asked to "match the original," prefer the values and behaviors
    listed above.\
-   When asked to "make it pop," keep within the aesthetic: subtle
    glass, clean type, calm motion.\
-   Do not switch to WinUI or DirectX unless explicitly requested. The
    current WebView2 approach is the reference path.\
-   Respect the configuration schema. Add new fields only after a code
    comment updates this document.

End of file.
