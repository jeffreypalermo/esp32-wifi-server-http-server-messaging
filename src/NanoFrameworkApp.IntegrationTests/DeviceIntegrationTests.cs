// These tests require the ESP32-S3 to be running and connected to the 'NanoFramework-ESP32S3' WiFi network.
// Connect your development machine to the 'NanoFramework-ESP32S3' WiFi network before running these tests.
// Run with: dotnet test --filter TestCategory=Integration

namespace NanoFrameworkApp.IntegrationTests;

[TestClass]
[TestCategory("Integration")]
public class DeviceIntegrationTests
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("http://192.168.4.1"),
        Timeout = TimeSpan.FromSeconds(5)
    };

    [TestMethod]
    [TestCategory("Integration")]
    public async Task TestDeviceIsReachable()
    {
        var response = await _client.GetAsync("/api/status");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.AreEqual("OK", content);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task TestHomepageReturnsHtml()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Flash LED"), "Homepage should contain 'Flash LED'");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task TestLedFlashEndpoint()
    {
        var response = await _client.PostAsync("/api/led/flash", null);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("triggered"), "Response should contain 'triggered'");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task TestLedFlashMultipleTimes()
    {
        for (int i = 0; i < 3; i++)
        {
            var response = await _client.PostAsync("/api/led/flash", null);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("triggered"),
                $"Flash request {i + 1} should contain 'triggered'");
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task TestNotFoundReturns404()
    {
        var response = await _client.GetAsync("/nonexistent");
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
