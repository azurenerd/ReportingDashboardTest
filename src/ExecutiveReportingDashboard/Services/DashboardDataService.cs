using System.Text.Json;
using ExecutiveReportingDashboard.Models;

namespace ExecutiveReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _filePath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.WebRootPath, "data", "dashboard-data.json");
    }

    public async Task<(DashboardData? Data, string? Error)> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return (null, "Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json.");

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            if (data == null)
                return (null, "Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json.");
            return (data, null);
        }
        catch (JsonException)
        {
            return (null, "Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json.");
        }
    }
}