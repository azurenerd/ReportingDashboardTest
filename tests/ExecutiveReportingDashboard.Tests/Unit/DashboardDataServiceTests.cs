using System.Text.Json;
using ExecutiveReportingDashboard.Models;
using ExecutiveReportingDashboard.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace ExecutiveReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_tests_{Guid.NewGuid():N}");
        _dataDir = Path.Combine(_tempDir, "data");
        Directory.CreateDirectory(_dataDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ReturnsError()
    {
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = await service.LoadAsync();

        data.Should().BeNull();
        error.Should().Be("Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json.");
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_ReturnsError()
    {
        await File.WriteAllTextAsync(Path.Combine(_dataDir, "dashboard-data.json"), "{ invalid json !@#");
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = await service.LoadAsync();

        data.Should().BeNull();
        error.Should().Be("Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json.");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_ReturnsData()
    {
        var dashboardData = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com",
            CurrentMonth = "Apr",
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
            NowLinePosition = 0.55,
            Milestones = new List<Milestone>
            {
                new() { Id = "M1", Label = "Phase 1", Color = "#0078D4", Events = new List<MilestoneEvent>() }
            },
            Categories = new List<Category>
            {
                new() { Name = "SHIPPED", ColorClass = "ship", Items = new Dictionary<string, List<string>> { ["Apr"] = new() { "Item 1" } } }
            }
        };

        var json = JsonSerializer.Serialize(dashboardData);
        await File.WriteAllTextAsync(Path.Combine(_dataDir, "dashboard-data.json"), json);
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = await service.LoadAsync();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Title.Should().Be("Test Project");
        data.Months.Should().HaveCount(4);
        data.Milestones.Should().HaveCount(1);
        data.Categories.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadAsync_NullDeserialization_ReturnsError()
    {
        await File.WriteAllTextAsync(Path.Combine(_dataDir, "dashboard-data.json"), "null");
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = await service.LoadAsync();

        data.Should().BeNull();
        error.Should().Be("Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json.");
    }

    [Fact]
    public async Task LoadAsync_CaseInsensitiveProperties_Deserializes()
    {
        var json = """{"Title":"Upper Case","Subtitle":"sub","BacklogUrl":"http://x","CurrentMonth":"Mar","Months":["Mar"],"Milestones":[],"Categories":[],"NowLinePosition":0.5}""";
        await File.WriteAllTextAsync(Path.Combine(_dataDir, "dashboard-data.json"), json);
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = await service.LoadAsync();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Title.Should().Be("Upper Case");
    }
}