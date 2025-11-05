using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace RssVisualizerScreensaver
{
    public static class SceneBuilder
    {
        public static string BuildHtml(ScenePayload payload)
        {
            // Serialize data with camelCase for JavaScript compatibility
            var jsonOptions = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };
            string itemsJson = JsonSerializer.Serialize(payload.Items, jsonOptions);
            SceneOptions o = payload.Options;

            // Raw CSS with token placeholders
            string css = """
html, body {
  margin:0; padding:0; height:100%; overflow:hidden; cursor:none;
  background: __BG_GRADIENT__;
  font-family: system-ui, Segoe UI, Roboto, Helvetica, Arial, sans-serif;
}
#stage {
  position:relative; width:100vw; height:100vh; perspective: 1200px;
}
/* Background columns of headlines */
.columns {
  position:absolute; inset:0; display:grid; grid-template-columns: 1fr;
  gap: 4vw; padding: 8vh 6vw; opacity:0.7;
}
.col {
  position:relative; overflow:hidden;
}
.col-roller {
  position:absolute; width:100%; top:100%; animation: scroll var(--scrollDur) linear infinite;
}
.col-item {
  color: __BG_COLUMN_COLOR__;
  font-size: __BG_FONT_PX__px; line-height:1.35; margin-bottom: 2.2vh;
  text-shadow: 0 1px 2px rgba(0,0,0,0.25);
  white-space:nowrap; overflow:hidden; text-overflow:ellipsis;
}
@keyframes scroll {
  from { transform: translateY(0%); }
  to   { transform: translateY(-200%); }
}

/* Word Cloud */
.word-cloud {
  position: absolute;
  top: 2vh;
  right: 2vw;
  width: 600px;
  height: 450px;
  z-index: 5;
  pointer-events: none;
}
.word-cloud-word {
  position: absolute;
  color: __BG_COLUMN_COLOR__;
  font-weight: 600;
  text-shadow: 0 1px 4px rgba(0,0,0,0.5);
  white-space: nowrap;
  opacity: 0.8;
  transition: opacity 0.3s ease;
}
.word-cloud-word:hover {
  opacity: 1;
}

/* Foreground card */
.card {
  position:absolute; 
  width:min(960px, 80vw); padding: 28px 36px; border-radius:16px;
  background:rgba(10, 14, 28, 0.35); backdrop-filter: blur(8px);
  color: __FG_COLOR__; box-shadow: 0 10px 40px rgba(0,0,0,0.35);
  transform-style: preserve-3d;
  z-index: 10;
}
.card-title {
  font-size: __FG_FONT_PX__px; font-weight:700; margin:0 0 10px 0;
  text-shadow: 0 2px 8px rgba(0,0,0,0.4);
}
.card-image {
  max-width: 100%; max-height: 300px; object-fit: contain; border-radius: 8px; margin-bottom: 14px;
}
.card-summary {
  font-size: __FG_SUMMARY_PX__px; opacity:0.9; margin:0 0 10px 0;
}
.card-meta {
  font-size:14px; opacity:0.8;
}
.card-enter {
  animation: cardIn calc(1800ms / var(--speed)) cubic-bezier(.2,.7,.1,1) forwards;
}
.card-leave {
  animation: cardOut calc(1300ms / var(--speed)) ease-in forwards;
}
@keyframes cardIn {
  from { opacity:0; transform: translateY(40px) rotateX(9deg); }
  to   { opacity:1; transform: translateY(0) rotateX(0deg); }
}
@keyframes cardOut {
  from { opacity:1; transform: translateY(0) rotateX(0deg); }
  to   { opacity:0; transform: translateY(-30px) rotateX(-6deg); }
}
""";

            // Raw JS with token placeholders
            string js = """
const SPEED = __SPEED__;
const ITEMS = __ITEMS__;
const DISPLAY_MS = __DISPLAY_MS__;
const CUSTOM_STOP_WORDS = __CUSTOM_STOP_WORDS__;

let idx = 0;
let cardEl = null;
let shownItems = [];
let availableIndices = []; // Track which articles haven't been shown yet
document.documentElement.style.setProperty('--speed', SPEED.toString());
document.documentElement.style.setProperty('--scrollDur', (80 / SPEED) + 's');

function createColumns(items) {
  const cols = document.querySelector('.columns');
  cols.innerHTML = '';
  
  // Create 1 column
  const col = document.createElement('div');
  col.className = 'col';
  const roller = document.createElement('div');
  roller.className = 'col-roller';
  
  if (items.length > 0) {
    const build = (arr) => arr.map(i => `<div class='col-item' title='${escapeHtml(i.title)}'>• ${escapeHtml(i.title)}</div>`).join('');
    roller.innerHTML = build(items) + build(items); // duplicate for seamless scroll
  }
  
  col.appendChild(roller);
  cols.appendChild(col);
}

function addItemToColumns(item) {
  const cols = document.querySelectorAll('.col');
  if (cols.length === 0) {
    // Create column if it doesn't exist
    createColumns([]);
  }
  const targetCol = document.querySelector('.col');
  const roller = targetCol.querySelector('.col-roller');
  const itemHtml = `<div class='col-item' title='${escapeHtml(item.title)}'>• ${escapeHtml(item.title)}</div>`;
  
  // Add to the end of roller
  roller.insertAdjacentHTML('beforeend', itemHtml);
}

function generateWordCloud(items) {
  console.log('Word cloud: Processing', items.length, 'items');
  
  // Common words to exclude
  const stopWords = new Set([
    'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for', 'of', 'with', 'by', 'from', 
    'as', 'is', 'was', 'are', 'be', 'has', 'have', 'had', 'do', 'does', 'did', 'will', 'would', 'could',
    'should', 'may', 'might', 'can', 'this', 'that', 'these', 'those', 'it', 'its', 'they', 'their',
    'them', 'he', 'she', 'his', 'her', 'who', 'what', 'when', 'where', 'why', 'how', 'which', 'all',
    'each', 'every', 'both', 'few', 'more', 'most', 'other', 'some', 'such', 'no', 'not', 'only', 
    'own', 'same', 'so', 'than', 'too', 'very', 'just', 'said', 'says', 'after', 'also', 'been',
    'about', 'over', 'into', 'through', 'during', 'before', 'between', 'under', 'again', 'further',
    'then', 'once', 'here', 'there', 'where', 'when', 'make', 'gets', 'made', 'make', 'being', 'have'
  ]);
  
  // Add custom stop words from configuration
  CUSTOM_STOP_WORDS.forEach(word => stopWords.add(word.toLowerCase()));
  
  console.log('Word cloud: Using', stopWords.size, 'total stop words');
  
  // Group items by source and extract words per source
  const sourceWords = {};
  
  items.forEach(item => {
    const source = item.source || 'Unknown';
    if (!sourceWords[source]) {
      sourceWords[source] = {};
    }
    
    const text = (item.title || '').toLowerCase();
    const words = text.match(/\b[a-z]{4,}\b/g) || [];
    
    words.forEach(word => {
      if (!stopWords.has(word)) {
        sourceWords[source][word] = (sourceWords[source][word] || 0) + 1;
      }
    });
  });
  
  const sources = Object.keys(sourceWords);
  console.log('Word cloud: Found', sources.length, 'sources');
  
  // Collect top words from each source (balanced sampling)
  const wordsPerSource = Math.ceil(800 / sources.length);
  const candidateWords = new Set();
  
  sources.forEach(source => {
    const sourceTop = Object.entries(sourceWords[source])
      .sort((a, b) => b[1] - a[1])
      .slice(0, wordsPerSource)
      .map(([word]) => word);
    
    sourceTop.forEach(word => candidateWords.add(word));
  });
  
  console.log('Word cloud: Collected', candidateWords.size, 'unique candidate words from all sources');
  
  // Now calculate GLOBAL frequency for these candidate words across ALL items
  const globalWordFreq = {};
  
  items.forEach(item => {
    const text = (item.title || '').toLowerCase();
    const words = text.match(/\b[a-z]{4,}\b/g) || [];
    
    words.forEach(word => {
      if (candidateWords.has(word)) {
        globalWordFreq[word] = (globalWordFreq[word] || 0) + 1;
      }
    });
  });
  
  // Sort by GLOBAL frequency for proper sizing
  const finalWords = Object.entries(globalWordFreq)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 600);
  
  console.log('Word cloud: Attempting to place', finalWords.length, 'words with global frequencies');
  
  if (finalWords.length === 0) return;
  
  const maxFreq = finalWords[0][1];
  const minFreq = finalWords[finalWords.length - 1][1];
  
  // Create temporary canvas for text measurement
  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');
  
  const cloudEl = document.getElementById('wordCloud');
  cloudEl.innerHTML = '';
  
  const width = 600;  // Increased from 420
  const height = 450; // Increased from 320
  const centerX = width / 2;
  const centerY = height / 2;
  
  const placed = [];
  
  finalWords.forEach(([word, freq]) => {
    // Map frequency to font size (14px to 48px for bigger range)
    const normalizedFreq = (freq - minFreq) / (maxFreq - minFreq || 1);
    const fontSize = 14 + (normalizedFreq * 34);
    
    // Measure text dimensions
    ctx.font = `600 ${fontSize}px system-ui, -apple-system, sans-serif`;
    const metrics = ctx.measureText(word);
    const textWidth = metrics.width;
    const textHeight = fontSize * 1.2;
    
    // Try to place word using Archimedean spiral
    let placed_word = false;
    const maxAttempts = 800; // Increased attempts for better packing
    
    for (let attempt = 0; attempt < maxAttempts && !placed_word; attempt++) {
      const angle = attempt * 0.15;
      const radius = 2.5 * angle;
      
      const x = centerX + radius * Math.cos(angle);
      const y = centerY + radius * Math.sin(angle);
      
      if (x - textWidth/2 < 0 || x + textWidth/2 > width ||
          y - textHeight/2 < 0 || y + textHeight/2 > height) {
        continue;
      }
      
      let collision = false;
      const padding = 3; // Slightly tighter packing
      
      for (const p of placed) {
        if (!(x + textWidth/2 + padding < p.x - p.w/2 ||
              x - textWidth/2 - padding > p.x + p.w/2 ||
              y + textHeight/2 + padding < p.y - p.h/2 ||
              y - textHeight/2 - padding > p.y + p.h/2)) {
          collision = true;
          break;
        }
      }
      
      if (!collision) {
        const span = document.createElement('span');
        span.className = 'word-cloud-word';
        span.textContent = word;
        span.style.fontSize = fontSize + 'px';
        span.style.left = x + 'px';
        span.style.top = y + 'px';
        span.style.transform = 'translate(-50%, -50%)';
        cloudEl.appendChild(span);
        
        placed.push({
          x: x,
          y: y,
          w: textWidth,
          h: textHeight
        });
        
        placed_word = true;
      }
    }
  });
  
  console.log('Word cloud: Successfully placed', placed.length, 'words out of', finalWords.length, 'candidates');
}

function mountCard(item, entering=true) {
  const stage = document.getElementById('stage');
  const el = document.createElement('div');
  el.className = 'card ' + (entering ? 'card-enter' : '');
  
  // Random position within safe bounds
  const maxWidth = 960;
  const cardWidth = Math.min(maxWidth, window.innerWidth * 0.8);
  const cardHeight = 400; // Approximate height
  
  const minX = cardWidth / 2 + 40;
  const maxX = window.innerWidth - cardWidth / 2 - 40;
  const minY = cardHeight / 2 + 40;
  const maxY = window.innerHeight - cardHeight / 2 - 40;
  
  const randomX = Math.random() * (maxX - minX) + minX;
  const randomY = Math.random() * (maxY - minY) + minY;
  
  el.style.left = randomX + 'px';
  el.style.top = randomY + 'px';
  el.style.translate = '-50% -50%';
  
  const imageHtml = item.imageUrl ? 
    `<img class='card-image' src='${escapeHtml(item.imageUrl)}' alt='' onerror='this.style.display="none"'>` : '';
  
  el.innerHTML = `
    ${imageHtml}
    <div class='card-title'>${escapeHtml(item.title)}</div>
    <div class='card-summary'>${escapeHtml(item.summary || '')}</div>
    <div class='card-meta'>${escapeHtml(item.source)} · ${new Date(item.published).toLocaleString()}</div>
  `;
  stage.appendChild(el);
  return el;
}

function nextCard() {
  if (!ITEMS.length) {
    console.log('nextCard: No items available');
    return;
  }
  
  console.log('nextCard: Available indices:', availableIndices.length, 'Total items:', ITEMS.length);
  
  // Reset available indices if all articles have been shown
  if (availableIndices.length === 0) {
    availableIndices = ITEMS.map((_, i) => i);
    console.log('nextCard: Reset available indices');
  }
  
  // Pick a random article from available ones
  const randomIdx = Math.floor(Math.random() * availableIndices.length);
  const itemIdx = availableIndices[randomIdx];
  availableIndices.splice(randomIdx, 1); // Remove from available list
  
  const item = ITEMS[itemIdx];
  console.log('nextCard: Showing item:', item.title);
  
  // Start exit animation for old card
  if (cardEl) {
    const oldCard = cardEl;
    oldCard.classList.remove('card-enter');
    oldCard.classList.add('card-leave');
    
    // Remove old card after exit animation completes
    setTimeout(() => oldCard.remove(), 1300);
    
    // Wait 1 second after exit completes before showing new card
    cardEl = null;
    setTimeout(() => {
      const newCard = mountCard(item, true);
      cardEl = newCard;
      
      // Add to background columns after showing in focus
      shownItems.push(item);
      addItemToColumns(item);
    }, 1300 + 1000);
  } else {
    // First card, show immediately
    const newCard = mountCard(item, true);
    cardEl = newCard;
    
    // Add to background columns after showing in focus
    shownItems.push(item);
    addItemToColumns(item);
  }
}

function start() {
  const sorted = ITEMS.slice().sort((a,b)=> (new Date(b.published)) - (new Date(a.published)));
  ITEMS.length = 0;
  ITEMS.push(...sorted);
  
  // Initialize available indices with all article indices
  availableIndices = ITEMS.map((_, i) => i);
  
  // Start with empty columns
  createColumns([]);
  
  // Generate word cloud from all items
  generateWordCloud(ITEMS);
  
  nextCard();
  setInterval(nextCard, DISPLAY_MS);
}

window.__rssUpdate = function(items) {
  ITEMS.length = 0;
  ITEMS.push(...items);
  shownItems = [];
  availableIndices = ITEMS.map((_, i) => i); // Reset available indices on data refresh
  document.querySelectorAll('.card').forEach(n => n.remove());
  generateWordCloud(ITEMS); // Update word cloud with new data
  start();
}

function escapeHtml(s) {
  return (s || '').replace(/[&<>"']/g, m => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m]));
}

// Mouse and click tracking for screensaver exit
let lastMouseX = -1;
let lastMouseY = -1;
let mouseInitialized = false;

document.addEventListener('mousemove', (e) => {
  if (!mouseInitialized) {
    lastMouseX = e.clientX;
    lastMouseY = e.clientY;
    mouseInitialized = true;
    return;
  }
  
  const deltaX = Math.abs(e.clientX - lastMouseX);
  const deltaY = Math.abs(e.clientY - lastMouseY);
  
  if (deltaX > 5 || deltaY > 5) {
    console.log('Mouse movement detected in JS:', deltaX, deltaY);
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.postMessage('EXIT');
    }
  }
});

document.addEventListener('click', (e) => {
  console.log('Click detected in JS');
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage('EXIT');
  }
});

document.addEventListener('mousedown', (e) => {
  console.log('Mouse down detected in JS');
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage('EXIT');
  }
});

document.addEventListener('DOMContentLoaded', start);
""";

            // Apply tokens
            css = css
                .Replace("__BG_GRADIENT__", o.BackgroundGradientCss)
                .Replace("__BG_COLUMN_COLOR__", o.BackgroundColumnColorCss)
                .Replace("__FG_COLOR__", o.ForegroundColorCss)
                .Replace("__BG_FONT_PX__", o.BackgroundFontSize.ToString(CultureInfo.InvariantCulture))
                .Replace("__FG_FONT_PX__", o.ForegroundFontSize.ToString(CultureInfo.InvariantCulture))
                .Replace("__FG_SUMMARY_PX__", Math.Max(16, o.ForegroundFontSize - 18).ToString(CultureInfo.InvariantCulture));

            js = js
                .Replace("__SPEED__", o.SpeedFactor.ToString(CultureInfo.InvariantCulture))
                .Replace("__ITEMS__", itemsJson)
                .Replace("__DISPLAY_MS__", (o.ArticleDisplaySeconds * 1000).ToString(CultureInfo.InvariantCulture))
                .Replace("__CUSTOM_STOP_WORDS__", JsonSerializer.Serialize(o.CustomStopWords, jsonOptions));

            // Build minimal HTML
            var sb = new StringBuilder(64 * 1024);
            sb.Append("<!doctype html><html><head><meta charset='utf-8'>");
            sb.Append("<meta http-equiv='X-UA-Compatible' content='IE=edge'>");
            sb.Append("<meta name='viewport' content='width=device-width,initial-scale=1'>");
            sb.Append("<style>");
            sb.Append(css);
            sb.Append("</style></head><body>");
            sb.Append("<div id='stage'><div class='columns'></div><div class='word-cloud' id='wordCloud'></div></div>");
            sb.Append("<script>");
            sb.Append(js);
            sb.Append("</script>");
            sb.Append("</body></html>");

            return sb.ToString();
        }
    }
}