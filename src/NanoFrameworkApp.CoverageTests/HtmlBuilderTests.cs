using NanoFrameworkApp.Hardware;
using NanoFrameworkApp.Workers;

namespace NanoFrameworkApp.CoverageTests;

public class HtmlBuilderTests
{
    // ── GetHtmlPage ──────────────────────────────────────────────────────────

    [Fact]
    public void GetHtmlPage_ContainsDocType()
    {
        string html = HtmlBuilder.GetHtmlPage();

        Assert.StartsWith("<!DOCTYPE html>", html);
    }

    [Fact]
    public void GetHtmlPage_ContainsBoardNameInTitle()
    {
        string html = HtmlBuilder.GetHtmlPage();

        Assert.Contains("<title>nanoFramework " + BoardConfig.SocName + "</title>", html);
    }

    [Fact]
    public void GetHtmlPage_ContainsOpenAndCloseHtmlTags()
    {
        string html = HtmlBuilder.GetHtmlPage();

        Assert.Contains("<html", html);
        Assert.EndsWith("</html>", html);
    }

    [Fact]
    public void GetHtmlPage_ContainsStyleBlock()
    {
        string html = HtmlBuilder.GetHtmlPage();

        Assert.Contains("<style>", html);
        Assert.Contains("</style>", html);
    }

    [Fact]
    public void GetHtmlPage_ContainsScriptBlock()
    {
        string html = HtmlBuilder.GetHtmlPage();

        Assert.Contains("<script>", html);
        Assert.Contains("</script>", html);
    }

    // ── GetCssBlock ──────────────────────────────────────────────────────────

    [Fact]
    public void GetCssBlock_ContainsLedColorClass()
    {
        string css = HtmlBuilder.GetCssBlock();

        // .chip color uses the board's LED color
        Assert.Contains(".chip{color:" + BoardConfig.LedColor + "}", css);
    }

    [Fact]
    public void GetCssBlock_ContainsKeyframeAnimation()
    {
        string css = HtmlBuilder.GetCssBlock();

        Assert.Contains("@keyframes lf", css);
        Assert.Contains("@keyframes dp", css);
    }

    [Fact]
    public void GetCssBlock_WrappedInStyleTag()
    {
        string css = HtmlBuilder.GetCssBlock();

        Assert.StartsWith("<style>", css);
        Assert.EndsWith("</style>", css);
    }

    [Fact]
    public void GetCssBlock_ContainsFlashButtonClass()
    {
        string css = HtmlBuilder.GetCssBlock();

        Assert.Contains(".fb{", css);
        Assert.Contains(".fb:disabled{", css);
    }

    // ── GetBodyHtml ──────────────────────────────────────────────────────────

    [Fact]
    public void GetBodyHtml_ContainsBoardName()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains(BoardConfig.SocName, body);
    }

    [Fact]
    public void GetBodyHtml_ContainsLedPin()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains("GPIO " + BoardConfig.LedPin.ToString(), body);
    }

    [Fact]
    public void GetBodyHtml_ContainsActiveHighOrActiveLow()
    {
        string body = HtmlBuilder.GetBodyHtml();

        string expected = BoardConfig.LedActiveHigh ? "Active-High" : "Active-Low";
        Assert.Contains(expected, body);
    }

    [Fact]
    public void GetBodyHtml_ContainsSvgLedElement()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains("<svg id=\"led\"", body);
        Assert.Contains("<radialGradient", body);
    }

    [Fact]
    public void GetBodyHtml_ContainsFlashButton()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains("id=\"btn\"", body);
        Assert.Contains("onclick=\"doFlash()\"", body);
    }

    [Fact]
    public void GetBodyHtml_ContainsFlashHistoryChart()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains("id=\"ch\"", body);
        Assert.Contains("Flash History", body);
    }

    [Fact]
    public void GetBodyHtml_ContainsLedColors()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains(BoardConfig.LedColor, body);
        Assert.Contains(BoardConfig.LedColorLight, body);
        Assert.Contains(BoardConfig.LedColorDark, body);
    }

    [Fact]
    public void GetBodyHtml_ContainsOnlineIndicator()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains("Online", body);
        Assert.Contains("class=\"dot\"", body);
    }

    [Fact]
    public void GetBodyHtml_ContainsUptimeElement()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains("id=\"upt\"", body);
    }

    [Fact]
    public void GetBodyHtml_ContainsTotalFlashesCounter()
    {
        string body = HtmlBuilder.GetBodyHtml();

        Assert.Contains("id=\"cnt\"", body);
        Assert.Contains("Total Flashes", body);
    }

    // ── GetScript ────────────────────────────────────────────────────────────

    [Fact]
    public void GetScript_ContainsApiEndpoints()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains("/api/led/flash", script);
        Assert.Contains("/api/status", script);
        Assert.Contains("/api/history", script);
    }

    [Fact]
    public void GetScript_ContainsPollingIntervals()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains("setInterval(fetchS", script);
        Assert.Contains("setInterval(fetchH", script);
    }

    [Fact]
    public void GetScript_ContainsDoFlashFunction()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains("function doFlash()", script);
    }

    [Fact]
    public void GetScript_ContainsFetchStatusFunction()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains("function fetchS()", script);
    }

    [Fact]
    public void GetScript_ContainsFetchHistoryFunction()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains("function fetchH()", script);
    }

    [Fact]
    public void GetScript_ContainsDrawChartFunction()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains("function drawC()", script);
    }

    [Fact]
    public void GetScript_ContainsUptimeFormatter()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains("function fmtU(ms)", script);
    }

    [Fact]
    public void GetScript_UsesLedColorForChart()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains(BoardConfig.LedColor, script);
    }

    [Fact]
    public void GetScript_WrappedInScriptTag()
    {
        string script = HtmlBuilder.GetScript();

        Assert.StartsWith("<script>", script);
        Assert.EndsWith("</script>", script);
    }

    [Fact]
    public void GetScript_PostMethodForFlash()
    {
        string script = HtmlBuilder.GetScript();

        Assert.Contains("method:'POST'", script);
    }
}
