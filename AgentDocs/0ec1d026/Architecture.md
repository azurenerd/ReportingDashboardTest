# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application (.NET 8) that renders a pixel-perfect 1920×1080 project status visualization optimized for PowerPoint screenshot capture. The system reads all display data from a local JSON configuration file, requires zero authentication, zero cloud dependencies, and zero external NuGet packages beyond the default Blazor Server template.

**Architectural Goals:**

1. **Zero-friction operation** — Clone, `dotnet run`, screenshot. No setup, no credentials, no database.
2. **Pixel-perfect rendering** — Match `OriginalDesignConcept.html` exactly at 1920×1080 using CSS Grid, Flexbox, and inline SVG.
3. **Data-driven display** — Every visual element is driven by `dashboard-data.json`; zero hardcoded values in components.
4. **Minimal surface area** — One project, one page, one data file, three visual components. No over-engineering.
5. **Instant feedback loop** — Edit JSON, refresh browser, see changes. No restart required.

**Architecture Pattern:** Service-injected component tree with file-based data source. The application follows a simple read-render pattern with no write operations, no state management, and no user interaction beyond hyperlink navigation.

---

## System Components

### 1. Application Host (`Program.cs`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Configure and start the Kestrel web server with Blazor Server middleware |
| **Interfaces** | None exposed; entry point only |
| **Dependencies** | .NET 8 runtime, `DashboardDataService` (registered as singleton) |
| **Data** | None directly; delegates to service layer |

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

**Key decisions:**
- No authentication middleware
- No HTTPS redirection (local-only)
- No CORS policy (single-origin)
- `DashboardDataService` registered as singleton for file read caching within a request

---

### 2. Data Service (`Services/DashboardDataService.cs`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Load, deserialize, and validate the JSON configuration file on each request |
| **Interfaces** | `Task<DashboardData?> GetDashboardDataAsync()`, `string? GetErrorMessage()` |
| **Dependencies** | `IWebHostEnvironment` (for resolving `wwwroot` path), `System.Text.Json` |
| **Data** | Reads `wwwroot/data/dashboard-data.json` from disk |

**Behavior:**
- Reads the JSON file fresh on each call (no persistent cache — ensures edits are reflected on refresh)
- Returns `null` with a human-readable error message if file is missing or malformed
- Uses `JsonSerializerOptions` with `PropertyNameCaseInsensitive = true` for flexible authoring
- Does NOT watch the file system (out of scope for Phase 1)

```csharp
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
            return (data, null);
        }
        catch (JsonException)
        {
            return (null, "Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json.");
        }
    }
}
```

---

### 3. Root Component (`Components/App.razor`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | HTML document shell with fixed viewport meta, CSS references, Blazor script tag |
| **Interfaces** | Blazor root component |
| **Dependencies** | `_framework/blazor.web.js` (auto-included by template) |
| **Data** | None |

Sets `<meta name="viewport" content="width=1920">` and links to `css/dashboard.css`.

---

### 4. Empty Layout (`Components/Layout/EmptyLayout.razor`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render page body with zero chrome — no navigation sidebar, no header bar from template |
| **Interfaces** | Implements `LayoutComponentBase` |
| **Dependencies** | None |
| **Data** | `@Body` render fragment |

```razor
@inherits LayoutComponentBase
@Body
```

---

### 5. Dashboard Page (`Components/Pages/Dashboard.razor`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Orchestrate data loading and render the three visual sections; handle error state |
| **Interfaces** | Route: `/` |
| **Dependencies** | `DashboardDataService`, child components (`HeaderBar`, `TimelineSection`, `HeatmapGrid`) |
| **Data** | `DashboardData` model instance |

**Behavior:**
- Calls `DashboardDataService.LoadAsync()` in `OnInitializedAsync`
- If error, renders centered error message (no stack trace)
- If success, passes model properties to child components as `[Parameter]` values

---

### 6. Header Bar Component (`Components/Shared/HeaderBar.razor`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render project title, ADO backlog link, subtitle, and legend icons |
| **Interfaces** | Parameters: `Title`, `Subtitle`, `BacklogUrl`, `CurrentMonth` |
| **Dependencies** | None |
| **Data** | String parameters from parent |

**Visual mapping:** `.hdr` section of the reference design.

---

### 7. Timeline Section Component (`Components/Shared/TimelineSection.razor`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render SVG timeline with milestone tracks, event markers, and NOW indicator |
| **Interfaces** | Parameters: `Milestones` (list), `Months` (list), `NowLinePosition` (double) |
| **Dependencies** | None |
| **Data** | Milestone and event model objects |

**Visual mapping:** `.tl-area` section of the reference design. Renders a 1560×185 SVG.

---

### 8. Heatmap Grid Component (`Components/Shared/HeatmapGrid.razor`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render CSS Grid with month columns, category rows, and work item bullets |
| **Interfaces** | Parameters: `Categories` (list), `Months` (list), `CurrentMonth` (string) |
| **Dependencies** | None |
| **Data** | Category model objects with per-month item arrays |

**Visual mapping:** `.hm-wrap` and `.hm-grid` sections of the reference design.

---

### 9. Data Models (`Models/DashboardData.cs`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Strongly-typed POCOs representing the JSON schema |
| **Interfaces** | Plain C# classes with `[JsonPropertyName]` attributes |
| **Dependencies** | `System.Text.Json.Serialization` |
| **Data** | Deserialized from JSON file |

---

### 10. Global Stylesheet (`wwwroot/css/dashboard.css`)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Define the fixed 1920×1080 viewport, font stack, color variables, and all layout CSS |
| **Interfaces** | Referenced in `App.razor` `<head>` |
| **Dependencies** | Segoe UI font (Windows-native) |
| **Data** | None |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│  wwwroot/data/dashboard-data.json  (static file on disk)    │
└─────────────────────┬───────────────────────────────────────┘
                      │ File.ReadAllTextAsync()
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  DashboardDataService                                        │
│  - Reads file per request                                    │
│  - Deserializes to DashboardData                             │
│  - Returns (Data, Error) tuple                               │
└─────────────────────┬───────────────────────────────────────┘
                      │ Injected via @inject
                      ▼
┌─────────────────────────────────────────────────────────────┐
│  Dashboard.razor (Page)                                      │
│  - OnInitializedAsync: calls LoadAsync()                     │
│  - Error path: renders error message                         │
│  - Success path: passes data to child components             │
└───┬──────────────────┬──────────────────┬───────────────────┘
    │                  │                  │
    ▼                  ▼                  ▼
┌────────┐     ┌─────────────┐     ┌───────────┐
│HeaderBar│     │TimelineSection│    │HeatmapGrid│
│ .razor  │     │   .razor     │    │  .razor   │
└────────┘     └─────────────┘     └───────────┘
    │                  │                  │
    ▼                  ▼                  ▼
┌─────────────────────────────────────────────────────────────┐
│  HTML + CSS + SVG  (rendered to browser via SignalR)          │
└─────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| Pattern | Description |
|---------|-------------|
| **Service Injection** | `DashboardDataService` is injected into `Dashboard.razor` via `@inject` |
| **Parameter Passing** | Parent page passes data down to child components via `[Parameter]` properties |
| **No Events Up** | Child components do not emit events — the dashboard is read-only |
| **No State Management** | No cascading values, no state containers, no inter-component messaging |
| **SignalR (implicit)** | Blazor Server uses SignalR for DOM diffing; irrelevant to application logic |

### Request Lifecycle

1. Browser navigates to `http://localhost:5000`
2. Kestrel receives HTTP request, routes to Blazor Server middleware
3. `Dashboard.razor` component initializes
4. `OnInitializedAsync` calls `DashboardDataService.LoadAsync()`
5. Service reads JSON file from disk, deserializes to `DashboardData`
6. Component renders HTML/SVG tree using data model
7. Blazor Server sends rendered HTML to browser via SignalR
8. Browser displays pixel-perfect 1920×1080 dashboard
9. No further interaction unless user refreshes (which repeats from step 1)

---

## Data Model

### Entity: `DashboardData` (Root)

```csharp
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
```

### Entity: `Milestone`

```csharp
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
```

### Entity: `MilestoneEvent`

```csharp
public class MilestoneEvent
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public double Position { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;  // "checkpoint", "poc", "production"

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
```

### Entity: `Category`

```csharp
public class Category
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("colorClass")]
    public string ColorClass { get; set; } = string.Empty;  // "ship", "prog", "carry", "block"

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
```

### Entity Relationships

```
DashboardData (1)
  ├── Milestone (1..5)
  │     └── MilestoneEvent (1..N)
  └── Category (exactly 4)
        └── Items: Dictionary<month, List<workItem>> (N months × M items)
```

### Storage

| Aspect | Decision |
|--------|----------|
| Storage engine | Flat JSON file on local filesystem |
| Location | `wwwroot/data/dashboard-data.json` |
| Max size | 100KB (supports 12 months × 4 categories × 20 items) |
| Persistence | Checked into Git repository |
| Backup | Git version history |
| Migration | Manual JSON editing; no schema migration tooling needed |

---

## API Contracts

This application has **no REST API**. It is a server-rendered Blazor application with a single page route.

### Internal Service Contract

```csharp
// DashboardDataService — the only "API" in the system
public interface IDashboardDataService
{
    /// <summary>
    /// Loads dashboard data from the JSON configuration file.
    /// Returns the deserialized data or an error message string.
    /// Called once per page load (no caching across requests).
    /// </summary>
    Task<(DashboardData? Data, string? Error)> LoadAsync();
}
```

### Page Route

| Route | Method | Component | Response |
|-------|--------|-----------|----------|
| `/` | GET (implicit via Blazor) | `Dashboard.razor` | Full HTML page with dashboard |

### Error Handling Contract

| Condition | Behavior |
|-----------|----------|
| JSON file missing | Render centered error: "Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json." |
| JSON file malformed | Same error message (no stack trace, no technical details) |
| JSON file empty | Same error message |
| JSON valid but category has empty items array for a month | Render gray dash (`—`) in that cell |
| JSON valid but milestones array is empty | Timeline section renders with month grid only, no tracks |

### Static File Serving

| Path | Content |
|------|---------|
| `/_framework/blazor.web.js` | Blazor SignalR client (auto-served) |
| `/css/dashboard.css` | Global stylesheet |
| `/data/dashboard-data.json` | Configuration file (also readable directly via HTTP) |
| `/_content/*/` | CSS isolation bundles (auto-generated) |

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|---------------|
| Runtime | .NET 8.0 SDK (or self-contained publish) |
| Web server | Kestrel (built into .NET 8, no IIS/nginx) |
| Port | `http://localhost:5000` (configurable in `launchSettings.json`) |
| Protocol | HTTP only (no TLS certificate management) |
| OS | Windows 10/11 (primary), macOS/Linux (compatible) |

### Networking

| Requirement | Specification |
|-------------|---------------|
| Inbound | `localhost:5000` — browser to Kestrel |
| Outbound | **None** — zero network calls at runtime |
| DNS | Not required (localhost only) |
| Firewall | No ports need opening beyond loopback |

### Storage

| Requirement | Specification |
|-------------|---------------|
| Disk space | < 10MB total (application + dependencies) |
| File I/O | Single read of `dashboard-data.json` per page load (~1-100KB) |
| Temp files | None created |
| Database | None |

### CI/CD

| Aspect | Specification |
|--------|---------------|
| Build | `dotnet build` (no additional tooling) |
| Test | `dotnet test` (optional xUnit + bUnit project) |
| Deploy | Not applicable — local-only tool |
| Artifact | Not applicable — run from source |

### Development Environment

| Tool | Version | Required |
|------|---------|----------|
| .NET 8 SDK | 8.0.x | Yes |
| Chrome or Edge | 120+ | Yes (for viewing/screenshot) |
| Visual Studio / VS Code | Any recent | Recommended |
| Git | Any | Yes (for cloning) |

---

## Technology Stack Decisions

| Layer | Choice | Alternatives Considered | Justification |
|-------|--------|------------------------|---------------|
| **Framework** | Blazor Server (.NET 8) | Blazor WASM, plain Razor Pages, static HTML | Server provides hot-reload, simpler hosting, no separate static file deployment. SignalR overhead irrelevant for local single-user. |
| **Rendering** | Server-side Razor + inline SVG | Client-side JS charting (Chart.js, D3) | Zero JavaScript constraint. Inline SVG gives pixel-perfect control over timeline shapes. |
| **CSS Strategy** | Global `dashboard.css` + CSS isolation | Tailwind, CSS-in-JS, component libraries | Reference design CSS can be ported verbatim. No build step. No conflicting resets. |
| **Layout Engine** | CSS Grid + Flexbox | Tables, absolute positioning | CSS Grid maps 1:1 to the heatmap design. Flexbox handles header and timeline sidebar. |
| **Data Format** | JSON | YAML, TOML, XML, SQLite | Native `System.Text.Json` support, human-readable, Git-diffable, no extra packages. |
| **Data Access** | Direct file read (`File.ReadAllTextAsync`) | `IConfiguration`, EF Core, Dapper | Simplest possible approach. No DI complexity. File is the entire "database". |
| **Component Library** | None (pure HTML/CSS) | MudBlazor, Radzen, FluentUI | Component libraries add CSS resets and opinionated themes that conflict with pixel-perfect custom design. |
| **Charting Library** | None (hand-coded SVG) | Radzen.Blazor Charts, ApexCharts | Libraries add complexity, bundle size, and fight the custom design. SVG primitives are simpler for this use case. |
| **Testing** | xUnit + bUnit (optional) | NUnit, MSTest | Industry standard for .NET; bUnit is purpose-built for Blazor component testing. |
| **Package Management** | Zero additional NuGet packages | N/A | Default Blazor Server template includes everything needed: `System.Text.Json`, Kestrel, Razor, static files. |

---

## Security Considerations

### Threat Model

This application has an **extremely minimal threat surface** because it runs exclusively on localhost with no authentication, no user input processing, and no network communication.

| Threat | Applicability | Mitigation |
|--------|---------------|------------|
| Unauthorized access | N/A | Local-only; no network exposure |
| Data exfiltration | N/A | No sensitive data (project status only) |
| Injection attacks | N/A | No user input forms, no database queries |
| Cross-site scripting (XSS) | Minimal | Blazor's Razor engine auto-encodes output; JSON data rendered as text content |
| Cross-site request forgery (CSRF) | Minimal | Antiforgery middleware included by default; no state-changing operations |
| Denial of service | N/A | Local single-user; no external access |
| Supply chain attacks | Low | Zero external NuGet packages beyond the .NET SDK |
| File path traversal | Low | File path is hardcoded to `wwwroot/data/dashboard-data.json`; not user-controllable |

### Authentication & Authorization

**None required.** The application has:
- No login page
- No auth middleware
- No role-based access
- No tokens or credentials
- No user identity concept

### Data Protection

- JSON file contains **no PII** — only project names, milestone dates, and status labels
- File is checked into Git — versioned and auditable
- No encryption at rest or in transit (HTTP on localhost is acceptable)
- No secrets management needed (no API keys, connection strings, or certificates)

### Input Validation

The only "input" is the JSON configuration file:
- Validated via `System.Text.Json` deserialization (rejects malformed JSON)
- Strongly-typed POCOs prevent unexpected property injection
- Empty/missing fields handled gracefully with default values
- No user-editable forms or query parameters processed

---

## Scaling Strategy

### Current Design: Single-User Local Tool

This application is intentionally **not designed to scale** beyond a single user on localhost. Scaling considerations are limited to:

| Dimension | Current Capacity | Notes |
|-----------|-----------------|-------|
| Concurrent users | 1 | Single developer/PM on their machine |
| Data volume | 100KB JSON | 12 months × 4 categories × 20 items per cell |
| Milestone tracks | 1–5 | Layout accommodates up to 5 vertical tracks in 185px SVG height |
| Month columns | 1–6 | CSS Grid `repeat(N, 1fr)` adapts; 6 months is the visual maximum for readability |
| Page load time | < 2 seconds | Single file read + Razor render; no optimization needed |
| Memory | < 100MB RSS | .NET 8 Blazor Server baseline is ~50MB; JSON data adds negligible overhead |

### Future Scaling Paths (Phase 2+, if needed)

| Scenario | Approach |
|----------|----------|
| Multiple projects | Query parameter routing (`/?project=privacy`) with multiple JSON files |
| Team-wide access | Deploy as a container or self-contained executable on a shared server; add basic auth |
| Larger datasets | Paginate months (show 6 at a time with navigation); truncate items per cell at 10 |
| Auto-refresh | `FileSystemWatcher` + SignalR push to re-render without manual browser refresh |
| Self-contained distribution | `dotnet publish -c Release --self-contained -r win-x64` for single-file deployment |

### Performance Budget

| Metric | Budget | Achieved By |
|--------|--------|-------------|
| Time to first render | < 2s | Single file I/O + synchronous Razor rendering |
| JSON parse time | < 10ms | `System.Text.Json` is allocation-efficient for < 100KB |
| Memory per request | < 1MB | Small POCO graph, no buffering |
| Cold start (`dotnet run`) | < 5s | Minimal middleware pipeline, no EF Core migrations |

---

## Risks & Mitigations

| # | Risk | Severity | Likelihood | Impact | Mitigation |
|---|------|----------|------------|--------|------------|
| 1 | **SVG rendering inconsistencies** between Chrome and Edge | Low | Low | Minor visual differences in drop shadows or text kerning | Both browsers use Chromium renderer. Test in both. Use `font-family: 'Segoe UI', Arial` for consistency. |
| 2 | **JSON schema drift** as requirements evolve | Medium | Medium | Deserialization failures or missing data on dashboard | Define strict C# POCOs with `[JsonPropertyName]` attributes. Ship a `dashboard-data.sample.json` as documentation. Consider adding a JSON Schema file for editor validation. |
| 3 | **Fixed 1920×1080 layout clips content** on smaller monitors | Low | High | Developer cannot see full dashboard during development | Accepted by design — this is a screenshot tool. Use browser zoom or DevTools device emulation during development. |
| 4 | **Blazor Server requires .NET 8 SDK** installed | Low | Low | New team member cannot run without SDK | Document in README. Alternatively, publish as self-contained single-file executable. |
| 5 | **SignalR circuit disconnects** after idle timeout | Low | Medium | Page shows "reconnecting" overlay | Irrelevant for screenshot workflow (user refreshes before screenshotting). Optionally increase `CircuitOptions.DisconnectedCircuitRetentionPeriod`. |
| 6 | **Heatmap cell overflow** with too many items | Medium | Low | Text clips or layout breaks | CSS `overflow: hidden` on cells (matches reference). Document max 10 items per cell in JSON schema comments. |
| 7 | **Hot-reload breaks layout** during development | Trivial | Medium | CSS changes don't apply | Stateless page — full browser refresh always works. `dotnet watch` handles most cases. |
| 8 | **JSON file accidentally deleted or corrupted** | Low | Low | Dashboard shows error instead of data | Clear error message directs user to check the file. Git history provides recovery. Ship sample file alongside. |
| 9 | **Segoe UI font unavailable** on non-Windows OS | Low | Low | Fallback to Arial changes visual appearance slightly | Font stack includes Arial as fallback. Primary target is Windows. Difference is minimal. |
| 10 | **Future scope creep** adds complexity | Medium | Medium | Simple architecture becomes over-engineered | Document Phase 2 items clearly as out-of-scope. Maintain zero-dependency principle. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component with its CSS strategy, data bindings, and interactions.

### Component Map

| Visual Section | Blazor Component | CSS Strategy | Data Bindings | Interactions |
|----------------|-----------------|--------------|---------------|--------------|
| Full page body | `Dashboard.razor` | Fixed `width: 1920px; height: 1080px; overflow: hidden; display: flex; flex-direction: column` | `DashboardData` model (loaded via service) | None (read-only) |
| Header bar (`.hdr`) | `HeaderBar.razor` | Flexbox: `justify-content: space-between; align-items: center; padding: 12px 44px 10px` | `Title`, `Subtitle`, `BacklogUrl`, `CurrentMonth` | ADO link opens in new tab (`target="_blank"`) |
| Legend (right side of header) | Inline in `HeaderBar.razor` | Flex row with `gap: 22px`. Shape indicators via inline `<span>` with CSS transforms | `CurrentMonth` (for "Now" label text) | None |
| Timeline area (`.tl-area`) | `TimelineSection.razor` | Flexbox horizontal, fixed `height: 196px; background: #FAFAFA` | `Milestones`, `Months`, `NowLinePosition` | None |
| Milestone sidebar (left of timeline) | Inline in `TimelineSection.razor` | Fixed `width: 230px; flex-direction: column; justify-content: space-around` | `Milestones[].Id`, `Milestones[].Label`, `Milestones[].Color` | None |
| SVG timeline canvas | Inline SVG in `TimelineSection.razor` | SVG `width="1560" height="185"` with computed positions | `Months` (grid lines), `Milestones[].Events` (markers), `NowLinePosition` | None |
| Heatmap wrapper (`.hm-wrap`) | `HeatmapGrid.razor` | Flex column: `flex: 1; min-height: 0; padding: 10px 44px 10px` | `Categories`, `Months`, `CurrentMonth` | None |
| Heatmap title | Inline in `HeatmapGrid.razor` | `font-size: 14px; font-weight: 700; color: #888; text-transform: uppercase` | Static text: "MONTHLY EXECUTION STATUS" | None |
| Grid (`.hm-grid`) | Inline in `HeatmapGrid.razor` | CSS Grid: `grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)` | Dynamic column count from `Months.Count` | None |
| Grid corner cell | `<div class="hm-corner">` | `background: #F5F5F5; font-size: 11px; text-transform: uppercase` | Static text or empty | None |
| Grid column headers | `@foreach (var month in Months)` | `hm-col-hdr` class + conditional `apr-hdr` class when `month == CurrentMonth` | `Months[]`, `CurrentMonth` | None |
| Grid row headers | `@foreach (var cat in Categories)` | `{colorClass}-hdr` class (e.g., `ship-hdr`, `prog-hdr`) | `Categories[].Name`, `Categories[].ColorClass` | None |
| Grid data cells | Nested loop: categories × months | `{colorClass}-cell` class + conditional `apr` class for current month | `Categories[].Items[month]` (list of strings) | None |
| Cell items (bullets) | `@foreach (var item in items)` | `<div class="it">` with `::before` pseudo-element for colored dot | Item text strings | None |
| Empty cell state | Conditional in cell loop | Gray dash character when `items.Count == 0` | Empty array check | None |
| Error state | Conditional in `Dashboard.razor` | Centered `<div>` with `display: flex; justify-content: center; align-items: center; height: 100%` | Error message string from service | None |

### CSS Class-to-Component Mapping

```
dashboard.css (global)
├── body styles (fixed viewport)
├── .hdr, .hdr h1, .sub           → HeaderBar.razor
├── .tl-area, .tl-svg-box         → TimelineSection.razor
├── .hm-wrap, .hm-title, .hm-grid → HeatmapGrid.razor
├── .hm-corner, .hm-col-hdr       → HeatmapGrid.razor (header row)
├── .hm-row-hdr                    → HeatmapGrid.razor (row labels)
├── .hm-cell, .it                  → HeatmapGrid.razor (data cells)
├── .ship-*, .prog-*, .carry-*, .block-*  → HeatmapGrid.razor (category colors)
└── .apr-hdr, .apr                 → HeatmapGrid.razor (current month highlight)
```

### SVG Rendering Details (TimelineSection)

| Visual Element | SVG Markup | Blazor Rendering Logic |
|----------------|-----------|----------------------|
| Month grid lines | `<line x1="@x" y1="0" x2="@x" y2="185" stroke="#bbb" stroke-opacity="0.4"/>` | `@for (int i = 0; i < Months.Count; i++)` with `x = i * (1560.0 / Months.Count)` |
| Month labels | `<text x="@(x+5)" y="14" fill="#666" font-size="11" font-weight="600">@month</text>` | Same loop as grid lines |
| NOW line | `<line x1="@nowX" y1="0" x2="@nowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>` | `nowX = NowLinePosition * 1560` |
| Milestone track | `<line x1="0" y1="@y" x2="1560" y2="@y" stroke="@color" stroke-width="3"/>` | `y = 42 + (index * 56)` for even vertical distribution |
| Checkpoint circle | `<circle cx="@cx" cy="@y" r="7" fill="white" stroke="@color" stroke-width="2.5"/>` | `cx = event.Position * 1560` |
| PoC diamond | `<polygon points="@pts" fill="#F4B400" filter="url(#sh)"/>` | Computed diamond points: `cx-11,cy cx,cy-11 cx+11,cy cx,cy+11` |
| Production diamond | `<polygon points="@pts" fill="#34A853" filter="url(#sh)"/>` | Same geometry as PoC, different fill color |
| Date labels | `<text x="@cx" y="@(y-16)" text-anchor="middle" fill="#666" font-size="10">@label</text>` | Positioned above or below the marker |
| Drop shadow filter | `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>` | Static, rendered once in SVG `<defs>` |

### Dynamic CSS Grid Configuration

The heatmap grid template adapts to the number of months:

```csharp
// In HeatmapGrid.razor
private string GridTemplateColumns => $"160px repeat({Months.Count}, 1fr)";
private string GridTemplateRows => $"36px repeat({Categories.Count}, 1fr)";
```

```razor
<div class="hm-grid" style="grid-template-columns: @GridTemplateColumns; grid-template-rows: @GridTemplateRows;">
```

### Current Month Highlighting Logic

```razor
@foreach (var month in Months)
{
    var isCurrentMonth = month == CurrentMonth;
    var headerClass = isCurrentMonth ? "hm-col-hdr apr-hdr" : "hm-col-hdr";
    <div class="@headerClass">@month</div>
}

@* In data cells: *@
@{
    var cellClass = $"{category.ColorClass}-cell{(month == CurrentMonth ? " apr" : "")}";
}
<div class="hm-cell @cellClass">
    @if (items.Count == 0)
    {
        <span style="color:#999;">—</span>
    }
    else
    {
        @foreach (var item in items)
        {
            <div class="it">@item</div>
        }
    }
</div>
```

---

## Solution Structure

```
ExecutiveReportingDashboard.sln
│
├── src/
│   └── ExecutiveReportingDashboard/
│       ├── ExecutiveReportingDashboard.csproj    # net8.0, no extra packages
│       ├── Program.cs                            # 10 lines, minimal host
│       ├── Components/
│       │   ├── App.razor                         # HTML shell, CSS link, viewport meta
│       │   ├── _Imports.razor                    # Global usings
│       │   ├── Pages/
│       │   │   └── Dashboard.razor              # Route "/", orchestrates layout
│       │   ├── Layout/
│       │   │   └── EmptyLayout.razor            # @Body only, no chrome
│       │   └── Shared/
│       │       ├── HeaderBar.razor              # Title, subtitle, legend
│       │       ├── TimelineSection.razor        # SVG milestone timeline
│       │       └── HeatmapGrid.razor            # CSS Grid status heatmap
│       ├── Models/
│       │   └── DashboardData.cs                 # All POCOs in one file
│       ├── Services/
│       │   └── DashboardDataService.cs          # JSON file reader
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css                # All styles (ported from reference)
│       │   └── data/
│       │       ├── dashboard-data.json          # Active configuration
│       │       └── dashboard-data.sample.json   # Example with fictional data
│       └── Properties/
│           └── launchSettings.json              # Port 5000 config
│
└── tests/
    └── ExecutiveReportingDashboard.Tests/
        ├── ExecutiveReportingDashboard.Tests.csproj  # xUnit + bUnit
        └── DashboardDataServiceTests.cs             # JSON loading tests
```

### Project File (`ExecutiveReportingDashboard.csproj`)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Zero additional `<PackageReference>` entries.** The Web SDK provides everything needed.