using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ExecutiveReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    public async Task Dashboard_LoadsAndRendersHeader()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        await header.WaitForAsync();
        (await header.IsVisibleAsync()).Should().BeTrue();

        var title = page.Locator(".hdr h1");
        (await title.TextContentAsync()).Should().NotBeNullOrEmpty();

        var subtitle = page.Locator(".sub");
        (await subtitle.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_RendersTimelineSection()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var timeline = page.Locator(".tl-area");
        await timeline.WaitForAsync();
        (await timeline.IsVisibleAsync()).Should().BeTrue();

        var svg = page.Locator(".tl-svg-box svg");
        (await svg.IsVisibleAsync()).Should().BeTrue();
        (await svg.GetAttributeAsync("width")).Should().Be("1560");
        (await svg.GetAttributeAsync("height")).Should().Be("185");
    }

    [Fact]
    public async Task Dashboard_RendersHeatmapGrid()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heatmap = page.Locator(".hm-wrap");
        await heatmap.WaitForAsync();
        (await heatmap.IsVisibleAsync()).Should().BeTrue();

        var grid = page.Locator(".hm-grid");
        (await grid.IsVisibleAsync()).Should().BeTrue();

        var corner = page.Locator(".hm-corner");
        (await corner.TextContentAsync()).Should().Contain("Status");
    }

    [Fact]
    public async Task Dashboard_HasNoScrollbars_At1920x1080()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var overflowHidden = await page.EvaluateAsync<bool>(
            "() => window.getComputedStyle(document.body).overflow === 'hidden'");
        overflowHidden.Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_HeaderBacklogLink_OpensInNewTab()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var link = page.Locator(".hdr h1 a");
        var target = await link.GetAttributeAsync("target");
        target.Should().Be("_blank");
    }
}