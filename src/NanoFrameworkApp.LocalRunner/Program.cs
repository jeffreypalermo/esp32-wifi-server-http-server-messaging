using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Microsoft.Playwright;

// ═══════════════════════════════════════════════════════════════════════════════
// Local Runner: Mirrors the nanoFramework ESP32-S3 app for integration testing.
// Runs the same architecture (MessageBus, Workers) using standard .NET APIs.
// ═══════════════════════════════════════════════════════════════════════════════

const int Port = 5080;
string BaseUrl = $"http://localhost:{Port}";
var logs = new ConcurrentQueue<string>();

void Log(string message)
{
    var entry = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
    logs.Enqueue(entry);
    Console.WriteLine(entry);
}

// ── MessageBus (standard .NET port) ─────────────────────────────────────────

var subscribers = new ConcurrentDictionary<string, List<Action<string>>>();

void Subscribe(string topic, Action<string> handler)
{
    subscribers.AddOrUpdate(topic,
        _ => new List<Action<string>> { handler },
        (_, list) => { lock (list) { list.Add(handler); } return list; });
}

void Publish(string topic, string payload)
{
    Log($"MessageBus: Publishing to '{topic}' payload='{payload}'");
    if (subscribers.TryGetValue(topic, out var handlers))
    {
        List<Action<string>> snapshot;
        lock (handlers) { snapshot = new List<Action<string>>(handlers); }
        foreach (var h in snapshot) h(payload);
    }
}

// ── FakeLedController ───────────────────────────────────────────────────────

int flashCount = 0;

void FlashLed(int times)
{
    for (int i = 0; i < times; i++)
    {
        flashCount++;
        Log($"LED: ON  (flash #{flashCount})");
        Thread.Sleep(100);
        Log($"LED: OFF (flash #{flashCount})");
        Thread.Sleep(100);
    }
}

// ── LedWorker ───────────────────────────────────────────────────────────────

Subscribe("led/flash", payload =>
{
    if (int.TryParse(payload, out int count))
    {
        Log($"LedWorker: Received flash command, count={count}");
        Task.Run(() => FlashLed(count));
    }
});
Log("LedWorker: Started, subscribed to 'led/flash'");

// ── WebServerWorker (HttpListener) ──────────────────────────────────────────

var listener = new HttpListener();
listener.Prefixes.Add($"{BaseUrl}/");
listener.Start();
Log($"WebServerWorker: Listening on {BaseUrl}");

var serverCts = new CancellationTokenSource();

_ = Task.Run(async () =>
{
    while (!serverCts.Token.IsCancellationRequested)
    {
        try
        {
            var ctx = await listener.GetContextAsync();
            var url = ctx.Request.Url?.AbsolutePath ?? "/";
            var method = ctx.Request.HttpMethod;

            Log($"WebServer: {method} {url}");

            string responseBody;
            string contentType;

            if (url == "/api/led/flash" && method == "POST")
            {
                Publish("led/flash", "3");
                responseBody = "LED flash triggered";
                contentType = "text/plain";
            }
            else if (url == "/api/status" && method == "GET")
            {
                responseBody = "OK";
                contentType = "text/plain";
            }
            else
            {
                responseBody = GetHtmlPage();
                contentType = "text/html";
            }

            var buffer = Encoding.UTF8.GetBytes(responseBody);
            ctx.Response.ContentType = contentType;
            ctx.Response.ContentLength64 = buffer.Length;
            await ctx.Response.OutputStream.WriteAsync(buffer);
            ctx.Response.Close();
        }
        catch (Exception ex) when (!serverCts.Token.IsCancellationRequested)
        {
            Log($"WebServer error: {ex.Message}");
        }
    }
});

// ── Run Playwright Tests ────────────────────────────────────────────────────

Log("═══════════════════════════════════════════════════");
Log("Starting Playwright browser automation...");
Log("═══════════════════════════════════════════════════");

// Give the server a moment to be ready
await Task.Delay(500);

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
var page = await browser.NewPageAsync();

// Test 1: Load homepage
Log("TEST 1: Loading homepage...");
var response = await page.GotoAsync(BaseUrl);
Log($"  Response status: {response?.Status}");
var title = await page.TitleAsync();
Log($"  Page title: '{title}'");
var bodyHtml = await page.ContentAsync();
bool hasButton = bodyHtml.Contains("Flash LED");
Log($"  Has 'Flash LED' button: {hasButton}");

// Test 2: Check /api/status endpoint
Log("TEST 2: Checking /api/status...");
var statusResponse = await page.GotoAsync($"{BaseUrl}/api/status");
var statusText = await page.InnerTextAsync("body");
Log($"  Status response: '{statusText.Trim()}'");

// Test 3: Navigate back and click the Flash LED button
Log("TEST 3: Clicking Flash LED button...");
await page.GotoAsync(BaseUrl);
int flashCountBefore = flashCount;
Log($"  Flash count before click: {flashCountBefore}");

// Click the button and wait for the response
await page.ClickAsync("button");
await Task.Delay(1500); // Wait for LED flashes (3 * 200ms = 600ms + margin)

int flashCountAfter = flashCount;
Log($"  Flash count after click: {flashCountAfter}");
Log($"  LED flashed {flashCountAfter - flashCountBefore} times");

// Check status text on page
var statusElement = await page.TextContentAsync("#status");
Log($"  Page status text: '{statusElement}'");

// Test 4: Click multiple times
Log("TEST 4: Clicking button 3 more times...");
int countBefore = flashCount;
await page.ClickAsync("button");
await Task.Delay(200);
await page.ClickAsync("button");
await Task.Delay(200);
await page.ClickAsync("button");
await Task.Delay(2500); // Wait for all flashes

int countAfter = flashCount;
Log($"  Total new flashes: {countAfter - countBefore} (expected ~9)");

// ── Results ─────────────────────────────────────────────────────────────────

Log("");
Log("═══════════════════════════════════════════════════");
Log("TEST RESULTS");
Log("═══════════════════════════════════════════════════");

bool test1Pass = response?.Status == 200 && hasButton && title.Contains("nanoFramework");
bool test2Pass = statusText.Trim() == "OK";
bool test3Pass = flashCountAfter - flashCountBefore == 3;
bool test4Pass = countAfter - countBefore >= 7; // At least 7 of 9 expected (async timing)

Log($"  Test 1 - Homepage loads with button:   {(test1Pass ? "PASS ✓" : "FAIL ✗")}");
Log($"  Test 2 - /api/status returns OK:       {(test2Pass ? "PASS ✓" : "FAIL ✗")}");
Log($"  Test 3 - Button click flashes LED:     {(test3Pass ? "PASS ✓" : "FAIL ✗")}");
Log($"  Test 4 - Multiple clicks queue flashes:{(test4Pass ? "PASS ✓" : "FAIL ✗")}");
Log($"  Total LED flashes recorded: {flashCount}");
Log("═══════════════════════════════════════════════════");

bool allPass = test1Pass && test2Pass && test3Pass && test4Pass;
Log(allPass ? "ALL TESTS PASSED ✓" : "SOME TESTS FAILED ✗");

// Cleanup
serverCts.Cancel();
listener.Stop();
await browser.CloseAsync();

// Print full telemetry log
Log("");
Log("── Full Telemetry Log ──────────────────────────────");
foreach (var entry in logs)
    Console.WriteLine($"  {entry}");

return allPass ? 0 : 1;

// ── HTML Page (same as ESP32 version) ───────────────────────────────────────

static string GetHtmlPage() =>
    """
    <!DOCTYPE html>
    <html><head>
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>nanoFramework ESP32-S3</title>
    <style>
    body { font-family: Arial, sans-serif; text-align: center; margin-top: 50px; background: #f0f0f0; }
    h1 { color: #333; }
    button { padding: 15px 30px; font-size: 18px; cursor: pointer; background: #0078d4; color: white; border: none; border-radius: 5px; }
    button:hover { background: #005a9e; }
    #status { margin-top: 20px; font-size: 16px; color: #555; }
    </style>
    </head><body>
    <h1>nanoFramework ESP32-S3</h1>
    <button onclick="flashLed()">Flash LED</button>
    <p id="status"></p>
    <script>
    function flashLed() {
      document.getElementById('status').textContent = 'Sending...';
      fetch('/api/led/flash', { method: 'POST' })
        .then(function(r) { return r.text(); })
        .then(function(t) { document.getElementById('status').textContent = t; })
        .catch(function(e) { document.getElementById('status').textContent = 'Error: ' + e; });
    }
    </script>
    </body></html>
    """;
