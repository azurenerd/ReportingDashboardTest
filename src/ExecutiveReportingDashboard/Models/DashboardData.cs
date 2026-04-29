using System.Text.Json.Serialization;

namespace ExecutiveReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    [JsonPropertyName("backlogUrl")]
    public string BacklogUrl { get; set; } = string.Empty;

    [JsonPropertyName("currentMonth")]
    public string CurrentMonth { get; set; } = string.Empty;

    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<Category> Categories { get; set; } = new();

    [JsonPropertyName("nowLinePosition")]
    public double NowLinePosition { get; set; }
}

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<MilestoneEvent> Events { get; set; } = new();
}

public class MilestoneEvent
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public double Position { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

public class Category
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("colorClass")]
    public string ColorClass { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; set; } = new();
}