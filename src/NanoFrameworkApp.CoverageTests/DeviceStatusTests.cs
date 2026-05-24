using NanoFrameworkApp;
using NanoFrameworkApp.Hardware;

namespace NanoFrameworkApp.CoverageTests;

public class DeviceStatusTests
{
    // ── GetStatusJson ────────────────────────────────────────────────────────

    [Fact]
    public void GetStatusJson_InitialState_CountIsZero()
    {
        var status = new DeviceStatus();

        string json = status.GetStatusJson();

        Assert.Contains("\"count\":0", json);
    }

    [Fact]
    public void GetStatusJson_AfterOneEvent_ReturnsCorrectCount()
    {
        var status = new DeviceStatus();
        status.RecordFlashEvent(5);

        string json = status.GetStatusJson();

        Assert.Contains("\"count\":5", json);
    }

    [Fact]
    public void GetStatusJson_AfterMultipleEvents_AccumulatesCount()
    {
        var status = new DeviceStatus();
        status.RecordFlashEvent(3);
        status.RecordFlashEvent(7);

        string json = status.GetStatusJson();

        Assert.Contains("\"count\":10", json);
    }

    [Fact]
    public void GetStatusJson_ContainsCorrectBoardName()
    {
        var status = new DeviceStatus();

        string json = status.GetStatusJson();

        Assert.Contains("\"board\":\"" + BoardConfig.SocName + "\"", json);
    }

    [Fact]
    public void GetStatusJson_HasValidJsonStructure()
    {
        var status = new DeviceStatus();

        string json = status.GetStatusJson();

        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
        Assert.Contains("\"board\":", json);
        Assert.Contains("\"count\":", json);
        Assert.Contains("\"uptime\":", json);
    }

    [Fact]
    public void GetStatusJson_UptimeFieldIsNonNegativeInteger()
    {
        var status = new DeviceStatus();

        string json = status.GetStatusJson();

        // JSON shape: {"board":"...","count":0,"uptime":NNN}
        const string marker = "\"uptime\":";
        int idx = json.IndexOf(marker) + marker.Length;
        string uptimeStr = json.Substring(idx).TrimEnd('}');
        long uptime = long.Parse(uptimeStr);

        Assert.True(uptime >= 0, $"Uptime must be non-negative, got {uptime}");
    }

    [Fact]
    public void GetStatusJson_IsThreadSafe()
    {
        var status = new DeviceStatus();
        var exceptions = new System.Collections.Generic.List<Exception>();
        var lockObj = new object();

        var threads = new Thread[8];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < 50; j++)
                    {
                        status.RecordFlashEvent(1);
                        string _ = status.GetStatusJson();
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObj) { exceptions.Add(ex); }
                }
            });
            threads[i].Start();
        }
        foreach (var t in threads) t.Join();

        Assert.Empty(exceptions);
        Assert.Contains("\"count\":400", status.GetStatusJson());
    }

    // ── GetHistoryJson ───────────────────────────────────────────────────────

    [Fact]
    public void GetHistoryJson_WhenEmpty_ReturnsEmptyArray()
    {
        var status = new DeviceStatus();

        string json = status.GetHistoryJson();

        Assert.Equal("[]", json);
    }

    [Fact]
    public void GetHistoryJson_WithOneEvent_ReturnsSingleElement()
    {
        var status = new DeviceStatus();
        status.RecordFlashEvent(3);

        string json = status.GetHistoryJson();

        Assert.Equal("[3]", json);
    }

    [Fact]
    public void GetHistoryJson_WithMultipleEvents_ReturnsAllInOrder()
    {
        var status = new DeviceStatus();
        status.RecordFlashEvent(3);
        status.RecordFlashEvent(5);
        status.RecordFlashEvent(1);

        string json = status.GetHistoryJson();

        Assert.Equal("[3,5,1]", json);
    }

    [Fact]
    public void GetHistoryJson_RecordsZeroFlashes()
    {
        var status = new DeviceStatus();
        status.RecordFlashEvent(0);

        string json = status.GetHistoryJson();

        Assert.Equal("[0]", json);
    }

    [Fact]
    public void GetHistoryJson_StartsAndEndsWithBrackets()
    {
        var status = new DeviceStatus();
        status.RecordFlashEvent(2);

        string json = status.GetHistoryJson();

        Assert.StartsWith("[", json);
        Assert.EndsWith("]", json);
    }

    // ── History overflow (triggers RemoveAt on line 28) ──────────────────────

    [Fact]
    public void RecordFlashEvent_After21Events_OldestEntryIsEvicted()
    {
        var status = new DeviceStatus();
        // Record 21 events with values 1..21; oldest (1) should be evicted
        for (int i = 1; i <= 21; i++)
            status.RecordFlashEvent(i);

        string json = status.GetHistoryJson();

        // First element must be 2 (value 1 was evicted)
        Assert.StartsWith("[2,", json);
        // Last element must be 21
        Assert.EndsWith(",21]", json);
    }

    [Fact]
    public void RecordFlashEvent_HistoryNeverExceeds20Entries()
    {
        var status = new DeviceStatus();
        for (int i = 1; i <= 25; i++)
            status.RecordFlashEvent(i);

        string json = status.GetHistoryJson();

        // Count commas: 19 commas = 20 elements
        int commas = 0;
        foreach (char c in json)
            if (c == ',') commas++;

        Assert.Equal(19, commas);
    }

    [Fact]
    public void RecordFlashEvent_After25Events_RetainsLatest20()
    {
        var status = new DeviceStatus();
        // Values 1–25; only 6–25 should remain
        for (int i = 1; i <= 25; i++)
            status.RecordFlashEvent(i);

        string json = status.GetHistoryJson();

        // Oldest retained is 6, newest is 25
        Assert.StartsWith("[6,", json);
        Assert.EndsWith(",25]", json);
    }

    [Fact]
    public void RecordFlashEvent_ExactlyAtCapacity_AllEntriesRetained()
    {
        var status = new DeviceStatus();
        // Exactly 20 entries — no overflow should occur
        for (int i = 1; i <= 20; i++)
            status.RecordFlashEvent(i);

        string json = status.GetHistoryJson();

        Assert.StartsWith("[1,", json);
        Assert.EndsWith(",20]", json);
        int commas = 0;
        foreach (char c in json)
            if (c == ',') commas++;
        Assert.Equal(19, commas);
    }
}
