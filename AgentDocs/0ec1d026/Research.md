# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-29 22:03 UTC_

### Summary

Build a single-page Blazor Server application (.NET 8) that renders a pixel-perfect executive reporting dashboard matching the provided HTML design concept. The app reads project data from a checked-in JSON configuration file (`dashboard-data.json`), requires zero authentication, zero cloud dependencies, and runs entirely on localhost. The architecture is intentionally minimal: one Blazor component renders a timeline with SVG milestones and a CSS Grid heatmap of project status. Total implementation effort is approximately 2–3 days for an engineer familiar with Blazor. **Primary Recommendation:** Use a single Blazor Server project with inline SVG rendering (no charting library), CSS isolation for styling, and `System.Text.Json` for configuration deserialization. This keeps dependencies near zero and maximizes screenshot fidelity for PowerPoint decks. ---

### Key Findings

- The design is a fixed-layout (1920×1080) dashboard optimized for screenshots, not responsive use — Blazor Server with static CSS is ideal for this.
- SVG timeline rendering is simple enough to do inline without a charting library; the reference design uses hand-coded SVG with basic shapes (lines, circles, diamonds).
- The heatmap grid maps directly to CSS Grid with 5 columns (`160px repeat(4, 1fr)`) and 5 rows — pure CSS, no JavaScript needed.
- JSON is the best configuration format: human-readable, editable by non-developers, natively supported by .NET 8 with `System.Text.Json`, and diffable in source control.
- Blazor Server's SignalR circuit is irrelevant for this use case (single user, local), but it provides instant hot-reload during development which accelerates iteration on visual fidelity.
- No database is needed — the JSON file is the entire data layer.
- The color palette and typography (Segoe UI) from the reference design are Windows-native and will render identically on the developer's machine.
- CSS isolation (`.razor.css` files) keeps component styles scoped and avoids global stylesheet conflicts.
- The `IConfiguration` / `IOptions<T>` pattern in .NET 8 can load the JSON file with zero custom code, but for this use case a simple `JsonSerializer.Deserialize<T>()` on startup is cleaner and more explicit. ---
- `dotnet new blazor -n ExecutiveReportingDashboard --interactivity Server`
- Create `DashboardData.cs` model classes matching the JSON schema above
- Create `DashboardDataService.cs` that reads and deserializes the JSON file
- Build `Dashboard.razor` with three sections: Header, Timeline (SVG), Heatmap (CSS Grid)
- Port the CSS from the HTML reference directly into `dashboard.css` or component-scoped `.razor.css` files
- Create `dashboard-data.json` with sample data for a fictional project
- Verify at 1920×1080 in browser — compare side-by-side with reference design
- Add file-watching to auto-reload dashboard when JSON changes
- Add subtle CSS transitions on page load (fade-in)
- Add a "Last Updated" timestamp in the footer
- Support multiple project files via query string routing
- Add a print-friendly CSS media query
- **Immediate visual fidelity**: Copy the CSS from the HTML reference verbatim into the Blazor app — it will work as-is since Blazor renders standard HTML.
- **Hot-reload workflow**: Use `dotnet watch` during development to iterate on layout without manual restarts.
- **Sample data**: Ship a `dashboard-data.sample.json` alongside the real file so new users can see the expected format.
```
wwwroot/data/dashboard-data.json          ← Active data file (gitignored or checked in)
wwwroot/data/dashboard-data.sample.json   ← Example with fictional project (always checked in)
``` | Package | Version | Required? | Purpose | |---------|---------|-----------|---------| | (none beyond default template) | — | — | The default Blazor Server template includes everything needed | **Zero additional NuGet packages are required.** The .NET 8 Blazor Server template includes `System.Text.Json`, Kestrel, static file serving, and Razor component support out of the box. This is the simplest possible dependency footprint.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | Framework | Blazor Server | .NET 8.0 (LTS) | Server-side rendering with SignalR | | CSS Layout | CSS Grid + Flexbox | Native | Heatmap grid and header layout | | SVG Rendering | Inline SVG in Razor | Native | Timeline milestones, diamonds, circles | | CSS Scoping | Blazor CSS Isolation | Built-in | Per-component `.razor.css` files | | Icons | None needed | — | Design uses geometric CSS/SVG shapes | | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | Runtime | .NET 8.0 | 8.0.x LTS | Application host | | JSON Parsing | System.Text.Json | Built-in (8.0) | Deserialize dashboard config | | Configuration | Manual file load via `File.ReadAllText` + `JsonSerializer` | Built-in | Simple, explicit, no DI overhead | | File Watching | `IHostEnvironment.ContentRootFileProvider` | Built-in | Optional: auto-reload on file change | | Aspect | Decision | Rationale | |--------|----------|-----------| | Format | JSON | Native .NET support, human-readable, Git-diffable | | File Name | `wwwroot/data/dashboard-data.json` | Convention: static data in wwwroot for easy access | | Schema | Strongly-typed C# POCOs | Compile-time safety, IntelliSense | | Alternative Considered | YAML | Rejected: requires `YamlDotNet` dependency, less .NET-native | | Alternative Considered | TOML | Rejected: poor .NET ecosystem support | | Tool | Version | Purpose | |------|---------|---------| | xUnit | 2.7+ | Unit testing framework | | bUnit | 1.25+ | Blazor component testing | | FluentAssertions | 6.12+ | Readable test assertions | | Tool | Purpose | |------|---------| | `dotnet watch` | Hot-reload during development | | Visual Studio 2022 / VS Code + C# Dev Kit | IDE | | Browser DevTools | Verify pixel-perfect layout at 1920×1080 | ---
```
Solution: ExecutiveReportingDashboard.sln
│
├── ExecutiveReportingDashboard/
│   ├── Program.cs                          # Minimal hosting
│   ├── Components/
│   │   ├── App.razor                       # Root
│   │   ├── Pages/
│   │   │   └── Dashboard.razor             # The single page
│   │   ├── Layout/
│   │   │   └── EmptyLayout.razor           # No nav, no chrome
│   │   └── Shared/
│   │       ├── TimelineSection.razor       # SVG timeline
│   │       ├── HeatmapGrid.razor           # CSS Grid status view
│   │       └── HeaderBar.razor             # Title + legend
│   ├── Models/
│   │   └── DashboardData.cs               # POCOs for JSON shape
│   ├── Services/
│   │   └── DashboardDataService.cs        # Loads & caches JSON
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css              # Global styles (fonts, reset)
│   │   └── data/
│   │       └── dashboard-data.json        # THE configuration file
│   └── Properties/
│       └── launchSettings.json
│
└── ExecutiveReportingDashboard.Tests/
    └── ... (bUnit + xUnit tests)
```
```
dashboard-data.json → DashboardDataService (reads on startup / on-demand)
    → Dashboard.razor (receives model)
        → HeaderBar.razor (title, subtitle, legend)
        → TimelineSection.razor (SVG milestones)
        → HeatmapGrid.razor (CSS Grid cells)
```
- **No database** — JSON file is the single source of truth. Edit it, refresh the page.
- **No routing** — Single page at `/`. No navigation needed.
- **No authentication** — Local use only. `Program.cs` has zero auth middleware.
- **No JavaScript** — Everything is CSS + SVG + Blazor server-side rendering.
- **Fixed viewport** — Design targets 1920×1080 for screenshot fidelity. Use `width: 1920px; height: 1080px; overflow: hidden` on `body`.
- **Component decomposition** — Three sub-components (Header, Timeline, Heatmap) for maintainability, but could be a single file for maximum simplicity.
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentMonth": "Apr",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "Jan 12", "position": 0.067, "type": "checkpoint" },
        { "date": "Mar 26", "position": 0.478, "type": "poc", "label": "PoC" },
        { "date": "May 1", "position": 0.667, "type": "production", "label": "Prod (TBD)" }
      ]
    }
  ],
  "categories": [
    {
      "name": "Shipped",
      "colorClass": "ship",
      "items": {
        "Jan": ["Agent Framework v1", "CI Pipeline"],
        "Feb": ["Data Connector", "Auth Module"],
        "Mar": ["Dashboard MVP"],
        "Apr": ["Reporting API", "Export Feature"]
      }
    },
    {
      "name": "In Progress",
      "colorClass": "prog",
      "items": { "Jan": [], "Feb": [], "Mar": ["Review Flow"], "Apr": ["Auto-DFD", "Policy Engine"] }
    },
    {
      "name": "Carryover",
      "colorClass": "carry",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Legacy Migration"] }
    },
    {
      "name": "Blockers",
      "colorClass": "block",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Vendor API Delay"] }
    }
  ],
  "nowLinePosition": 0.528
}
``` The timeline uses basic SVG primitives that map directly to Razor markup: | Visual Element | SVG Element | Blazor Approach | |----------------|-------------|-----------------| | Month grid lines | `<line>` | `@foreach` over months | | "NOW" indicator | `<line stroke-dasharray>` + `<text>` | Conditional render | | Milestone track | `<line>` per milestone | Loop over milestones | | Checkpoint | `<circle>` | Event type switch | | PoC diamond | `<polygon>` (rotated square) | Computed points | | Production diamond | `<polygon>` (green) | Computed points | All positions are computed as percentages of the SVG width (e.g., `position * svgWidth`), driven by the `position` field in the JSON. ---

### Considerations & Risks

- **None.** This is explicitly a local-only tool with no auth requirements. `Program.cs` should be:
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
- No sensitive data in the JSON config (it's project status, not PII).
- JSON file is checked into source control — treat it as documentation.
- No encryption needed. | Aspect | Recommendation | |--------|---------------| | Host | Local Kestrel (built into .NET 8) | | Port | Default `https://localhost:5001` or `http://localhost:5000` | | Deployment | `dotnet run` from the project directory | | Distribution | Clone the repo, `dotnet run`, open browser | | Containerization | Not needed for local use; optional `Dockerfile` if sharing | **$0.** Everything runs locally. No cloud services, no subscriptions, no licenses beyond what's already available with .NET 8 SDK (free, open source). --- | Risk | Severity | Mitigation | |------|----------|------------| | SVG rendering differences across browsers | Low | Target Chrome/Edge for screenshots; test in both | | JSON schema drift as features are added | Low | Define C# POCOs with `[JsonPropertyName]` attributes; add a JSON schema file for validation | | Fixed 1920×1080 layout breaks on smaller screens | Accepted | This is by design — the page is for screenshots, not general browsing | | Blazor Server requires .NET 8 SDK installed | Low | Document in README; could pre-build as self-contained if needed | | Hot-reload occasionally loses state | Trivial | Stateless page — just refresh | | SignalR circuit timeout on idle | Low | Irrelevant for screenshot use; set `CircuitOptions.DisconnectedCircuitRetentionPeriod` if it annoys during development |
- **Blazor Server over Blazor WASM** — Server is simpler (no separate hosting of static files), faster startup, and hot-reload works better. The SignalR overhead is irrelevant for local single-user.
- **Inline SVG over charting library** — Libraries like `Radzen.Blazor` or `ApexCharts.Blazor` add complexity and opinionated styling that would fight the pixel-perfect design. Hand-coded SVG in Razor gives total control.
- **JSON over database** — A SQLite database would be over-engineering. The data changes monthly and is edited by hand.
- **No component library (MudBlazor, Radzen)** — These add CSS resets and themes that would conflict with the custom design. Pure HTML/CSS is simpler and matches the reference exactly. ---
- **How often does the data change?** If monthly, manual JSON editing is fine. If weekly, consider adding a simple edit form (Phase 2).
- **Should the page auto-refresh?** If the JSON is updated while the page is open, should it reflect immediately? (Easy to add with `FileSystemWatcher` + SignalR push.)
- **Multiple projects?** Is this always one dashboard per project, or should it support switching between projects? (Could be handled with multiple JSON files and a query parameter: `/?project=privacy-automation`.)
- **Print/PDF export?** If PowerPoint embedding is the goal, browser "Print to PDF" at 1920×1080 might be sufficient. Or add a "Download PNG" button using browser screenshot APIs.
- **Who edits the JSON?** If non-technical PMs need to update it, consider adding a minimal JSON editor page (Phase 2) or providing a well-documented schema with examples. ---

### Detailed Analysis

# Research.md — Executive Reporting Dashboard

## Executive Summary

Build a single-page Blazor Server application (.NET 8) that renders a pixel-perfect executive reporting dashboard matching the provided HTML design concept. The app reads project data from a checked-in JSON configuration file (`dashboard-data.json`), requires zero authentication, zero cloud dependencies, and runs entirely on localhost. The architecture is intentionally minimal: one Blazor component renders a timeline with SVG milestones and a CSS Grid heatmap of project status. Total implementation effort is approximately 2–3 days for an engineer familiar with Blazor.

**Primary Recommendation:** Use a single Blazor Server project with inline SVG rendering (no charting library), CSS isolation for styling, and `System.Text.Json` for configuration deserialization. This keeps dependencies near zero and maximizes screenshot fidelity for PowerPoint decks.

---

## Key Findings

- The design is a fixed-layout (1920×1080) dashboard optimized for screenshots, not responsive use — Blazor Server with static CSS is ideal for this.
- SVG timeline rendering is simple enough to do inline without a charting library; the reference design uses hand-coded SVG with basic shapes (lines, circles, diamonds).
- The heatmap grid maps directly to CSS Grid with 5 columns (`160px repeat(4, 1fr)`) and 5 rows — pure CSS, no JavaScript needed.
- JSON is the best configuration format: human-readable, editable by non-developers, natively supported by .NET 8 with `System.Text.Json`, and diffable in source control.
- Blazor Server's SignalR circuit is irrelevant for this use case (single user, local), but it provides instant hot-reload during development which accelerates iteration on visual fidelity.
- No database is needed — the JSON file is the entire data layer.
- The color palette and typography (Segoe UI) from the reference design are Windows-native and will render identically on the developer's machine.
- CSS isolation (`.razor.css` files) keeps component styles scoped and avoids global stylesheet conflicts.
- The `IConfiguration` / `IOptions<T>` pattern in .NET 8 can load the JSON file with zero custom code, but for this use case a simple `JsonSerializer.Deserialize<T>()` on startup is cleaner and more explicit.

---

## Recommended Technology Stack

### Frontend (Blazor Server)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| Framework | Blazor Server | .NET 8.0 (LTS) | Server-side rendering with SignalR |
| CSS Layout | CSS Grid + Flexbox | Native | Heatmap grid and header layout |
| SVG Rendering | Inline SVG in Razor | Native | Timeline milestones, diamonds, circles |
| CSS Scoping | Blazor CSS Isolation | Built-in | Per-component `.razor.css` files |
| Icons | None needed | — | Design uses geometric CSS/SVG shapes |

### Backend / Data

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| Runtime | .NET 8.0 | 8.0.x LTS | Application host |
| JSON Parsing | System.Text.Json | Built-in (8.0) | Deserialize dashboard config |
| Configuration | Manual file load via `File.ReadAllText` + `JsonSerializer` | Built-in | Simple, explicit, no DI overhead |
| File Watching | `IHostEnvironment.ContentRootFileProvider` | Built-in | Optional: auto-reload on file change |

### Data Format

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| Format | JSON | Native .NET support, human-readable, Git-diffable |
| File Name | `wwwroot/data/dashboard-data.json` | Convention: static data in wwwroot for easy access |
| Schema | Strongly-typed C# POCOs | Compile-time safety, IntelliSense |
| Alternative Considered | YAML | Rejected: requires `YamlDotNet` dependency, less .NET-native |
| Alternative Considered | TOML | Rejected: poor .NET ecosystem support |

### Testing

| Tool | Version | Purpose |
|------|---------|---------|
| xUnit | 2.7+ | Unit testing framework |
| bUnit | 1.25+ | Blazor component testing |
| FluentAssertions | 6.12+ | Readable test assertions |

### Development Tools

| Tool | Purpose |
|------|---------|
| `dotnet watch` | Hot-reload during development |
| Visual Studio 2022 / VS Code + C# Dev Kit | IDE |
| Browser DevTools | Verify pixel-perfect layout at 1920×1080 |

---

## Architecture Recommendations

### Pattern: Single-Component Page with Service

```
Solution: ExecutiveReportingDashboard.sln
│
├── ExecutiveReportingDashboard/
│   ├── Program.cs                          # Minimal hosting
│   ├── Components/
│   │   ├── App.razor                       # Root
│   │   ├── Pages/
│   │   │   └── Dashboard.razor             # The single page
│   │   ├── Layout/
│   │   │   └── EmptyLayout.razor           # No nav, no chrome
│   │   └── Shared/
│   │       ├── TimelineSection.razor       # SVG timeline
│   │       ├── HeatmapGrid.razor           # CSS Grid status view
│   │       └── HeaderBar.razor             # Title + legend
│   ├── Models/
│   │   └── DashboardData.cs               # POCOs for JSON shape
│   ├── Services/
│   │   └── DashboardDataService.cs        # Loads & caches JSON
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css              # Global styles (fonts, reset)
│   │   └── data/
│   │       └── dashboard-data.json        # THE configuration file
│   └── Properties/
│       └── launchSettings.json
│
└── ExecutiveReportingDashboard.Tests/
    └── ... (bUnit + xUnit tests)
```

### Data Flow

```
dashboard-data.json → DashboardDataService (reads on startup / on-demand)
    → Dashboard.razor (receives model)
        → HeaderBar.razor (title, subtitle, legend)
        → TimelineSection.razor (SVG milestones)
        → HeatmapGrid.razor (CSS Grid cells)
```

### Key Design Decisions

1. **No database** — JSON file is the single source of truth. Edit it, refresh the page.
2. **No routing** — Single page at `/`. No navigation needed.
3. **No authentication** — Local use only. `Program.cs` has zero auth middleware.
4. **No JavaScript** — Everything is CSS + SVG + Blazor server-side rendering.
5. **Fixed viewport** — Design targets 1920×1080 for screenshot fidelity. Use `width: 1920px; height: 1080px; overflow: hidden` on `body`.
6. **Component decomposition** — Three sub-components (Header, Timeline, Heatmap) for maintainability, but could be a single file for maximum simplicity.

### JSON Configuration Schema (Recommended)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentMonth": "Apr",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "Jan 12", "position": 0.067, "type": "checkpoint" },
        { "date": "Mar 26", "position": 0.478, "type": "poc", "label": "PoC" },
        { "date": "May 1", "position": 0.667, "type": "production", "label": "Prod (TBD)" }
      ]
    }
  ],
  "categories": [
    {
      "name": "Shipped",
      "colorClass": "ship",
      "items": {
        "Jan": ["Agent Framework v1", "CI Pipeline"],
        "Feb": ["Data Connector", "Auth Module"],
        "Mar": ["Dashboard MVP"],
        "Apr": ["Reporting API", "Export Feature"]
      }
    },
    {
      "name": "In Progress",
      "colorClass": "prog",
      "items": { "Jan": [], "Feb": [], "Mar": ["Review Flow"], "Apr": ["Auto-DFD", "Policy Engine"] }
    },
    {
      "name": "Carryover",
      "colorClass": "carry",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Legacy Migration"] }
    },
    {
      "name": "Blockers",
      "colorClass": "block",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Vendor API Delay"] }
    }
  ],
  "nowLinePosition": 0.528
}
```

### SVG Timeline Rendering Strategy

The timeline uses basic SVG primitives that map directly to Razor markup:

| Visual Element | SVG Element | Blazor Approach |
|----------------|-------------|-----------------|
| Month grid lines | `<line>` | `@foreach` over months |
| "NOW" indicator | `<line stroke-dasharray>` + `<text>` | Conditional render |
| Milestone track | `<line>` per milestone | Loop over milestones |
| Checkpoint | `<circle>` | Event type switch |
| PoC diamond | `<polygon>` (rotated square) | Computed points |
| Production diamond | `<polygon>` (green) | Computed points |

All positions are computed as percentages of the SVG width (e.g., `position * svgWidth`), driven by the `position` field in the JSON.

---

## Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a local-only tool with no auth requirements. `Program.cs` should be:

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

### Data Protection

- No sensitive data in the JSON config (it's project status, not PII).
- JSON file is checked into source control — treat it as documentation.
- No encryption needed.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| Host | Local Kestrel (built into .NET 8) |
| Port | Default `https://localhost:5001` or `http://localhost:5000` |
| Deployment | `dotnet run` from the project directory |
| Distribution | Clone the repo, `dotnet run`, open browser |
| Containerization | Not needed for local use; optional `Dockerfile` if sharing |

### Infrastructure Costs

**$0.** Everything runs locally. No cloud services, no subscriptions, no licenses beyond what's already available with .NET 8 SDK (free, open source).

---

## Risks & Trade-offs

| Risk | Severity | Mitigation |
|------|----------|------------|
| SVG rendering differences across browsers | Low | Target Chrome/Edge for screenshots; test in both |
| JSON schema drift as features are added | Low | Define C# POCOs with `[JsonPropertyName]` attributes; add a JSON schema file for validation |
| Fixed 1920×1080 layout breaks on smaller screens | Accepted | This is by design — the page is for screenshots, not general browsing |
| Blazor Server requires .NET 8 SDK installed | Low | Document in README; could pre-build as self-contained if needed |
| Hot-reload occasionally loses state | Trivial | Stateless page — just refresh |
| SignalR circuit timeout on idle | Low | Irrelevant for screenshot use; set `CircuitOptions.DisconnectedCircuitRetentionPeriod` if it annoys during development |

### Trade-offs Made

1. **Blazor Server over Blazor WASM** — Server is simpler (no separate hosting of static files), faster startup, and hot-reload works better. The SignalR overhead is irrelevant for local single-user.
2. **Inline SVG over charting library** — Libraries like `Radzen.Blazor` or `ApexCharts.Blazor` add complexity and opinionated styling that would fight the pixel-perfect design. Hand-coded SVG in Razor gives total control.
3. **JSON over database** — A SQLite database would be over-engineering. The data changes monthly and is edited by hand.
4. **No component library (MudBlazor, Radzen)** — These add CSS resets and themes that would conflict with the custom design. Pure HTML/CSS is simpler and matches the reference exactly.

---

## Open Questions

1. **How often does the data change?** If monthly, manual JSON editing is fine. If weekly, consider adding a simple edit form (Phase 2).
2. **Should the page auto-refresh?** If the JSON is updated while the page is open, should it reflect immediately? (Easy to add with `FileSystemWatcher` + SignalR push.)
3. **Multiple projects?** Is this always one dashboard per project, or should it support switching between projects? (Could be handled with multiple JSON files and a query parameter: `/?project=privacy-automation`.)
4. **Print/PDF export?** If PowerPoint embedding is the goal, browser "Print to PDF" at 1920×1080 might be sufficient. Or add a "Download PNG" button using browser screenshot APIs.
5. **Who edits the JSON?** If non-technical PMs need to update it, consider adding a minimal JSON editor page (Phase 2) or providing a well-documented schema with examples.

---

## Implementation Recommendations

### Phase 1: MVP (1–2 days)

1. `dotnet new blazor -n ExecutiveReportingDashboard --interactivity Server`
2. Create `DashboardData.cs` model classes matching the JSON schema above
3. Create `DashboardDataService.cs` that reads and deserializes the JSON file
4. Build `Dashboard.razor` with three sections: Header, Timeline (SVG), Heatmap (CSS Grid)
5. Port the CSS from the HTML reference directly into `dashboard.css` or component-scoped `.razor.css` files
6. Create `dashboard-data.json` with sample data for a fictional project
7. Verify at 1920×1080 in browser — compare side-by-side with reference design

### Phase 2: Polish (Optional, 1 day)

- Add file-watching to auto-reload dashboard when JSON changes
- Add subtle CSS transitions on page load (fade-in)
- Add a "Last Updated" timestamp in the footer
- Support multiple project files via query string routing
- Add a print-friendly CSS media query

### Quick Wins

- **Immediate visual fidelity**: Copy the CSS from the HTML reference verbatim into the Blazor app — it will work as-is since Blazor renders standard HTML.
- **Hot-reload workflow**: Use `dotnet watch` during development to iterate on layout without manual restarts.
- **Sample data**: Ship a `dashboard-data.sample.json` alongside the real file so new users can see the expected format.

### Recommended File Naming Convention

```
wwwroot/data/dashboard-data.json          ← Active data file (gitignored or checked in)
wwwroot/data/dashboard-data.sample.json   ← Example with fictional project (always checked in)
```

### NuGet Packages Required

| Package | Version | Required? | Purpose |
|---------|---------|-----------|---------|
| (none beyond default template) | — | — | The default Blazor Server template includes everything needed |

**Zero additional NuGet packages are required.** The .NET 8 Blazor Server template includes `System.Text.Json`, Kestrel, static file serving, and Razor component support out of the box. This is the simplest possible dependency footprint.

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `AgentDocs/0ec1d026/OriginalDesignConcept.html`

**Type:** HTML Design Template

**Layout Structure:**
- **Header section** with title, subtitle, and legend
- **Timeline/Gantt section** with SVG milestone visualization
- **Heatmap grid** — status rows × month columns, color-coded by category
  - Shipped row (green tones)
  - In Progress row (blue tones)
  - Carryover row (yellow/amber tones)
  - Blockers row (red tones)

**Key CSS Patterns:**
- Uses CSS Grid layout
- Uses Flexbox layout
- Color palette: #FFFFFF, #111, #0078D4, #888, #FAFAFA, #F5F5F5, #999, #FFF0D0, #C07700, #333, #1B7A28, #E8F5E9, #F0FBF0, #D8F2DA, #34A853
- Font: Segoe UI
- Grid columns: `160px repeat(4,1fr)`
- Designed for 1920×1080 screenshot resolution

<details><summary>Full HTML Source</summary>

```html
<!DOCTYPE html><html lang="en"><head><meta charset="UTF-8">
<style>
*{margin:0;padding:0;box-sizing:border-box;}
body{width:1920px;height:1080px;overflow:hidden;background:#FFFFFF;
     font-family:'Segoe UI',Arial,sans-serif;color:#111;display:flex;flex-direction:column;}
a{color:#0078D4;text-decoration:none;}
.hdr{padding:12px 44px 10px;border-bottom:1px solid #E0E0E0;display:flex;
      align-items:center;justify-content:space-between;flex-shrink:0;}
.hdr h1{font-size:24px;font-weight:700;}
.sub{font-size:12px;color:#888;margin-top:2px;}
.tl-area{display:flex;align-items:stretch;padding:6px 44px 0;flex-shrink:0;height:196px;
          border-bottom:2px solid #E8E8E8;background:#FAFAFA;}
.tl-svg-box{flex:1;padding-left:12px;padding-top:6px;}
/* heatmap */
.hm-wrap{flex:1;min-height:0;display:flex;flex-direction:column;padding:10px 44px 10px;}
.hm-title{font-size:14px;font-weight:700;color:#888;letter-spacing:.5px;text-transform:uppercase;margin-bottom:8px;flex-shrink:0;}
.hm-grid{flex:1;min-height:0;display:grid;
          grid-template-columns:160px repeat(4,1fr);
          grid-template-rows:36px repeat(4,1fr);
          border:1px solid #E0E0E0;}
/* header cells */
.hm-corner{background:#F5F5F5;display:flex;align-items:center;justify-content:center;
            font-size:11px;font-weight:700;color:#999;text-transform:uppercase;
            border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr{display:flex;align-items:center;justify-content:center;
             font-size:16px;font-weight:700;background:#F5F5F5;
             border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr.apr-hdr{background:#FFF0D0;color:#C07700;}
/* row header */
.hm-row-hdr{display:flex;align-items:center;padding:0 12px;
             font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.7px;
             border-right:2px solid #CCC;border-bottom:1px solid #E0E0E0;}
/* data cells */
.hm-cell{padding:8px 12px;border-right:1px solid #E0E0E0;border-bottom:1px solid #E0E0E0;overflow:hidden;}
.hm-cell .it{font-size:12px;color:#333;padding:2px 0 2px 12px;position:relative;line-height:1.35;}
.hm-cell .it::before{content:'';position:absolute;left:0;top:7px;width:6px;height:6px;border-radius:50%;}
/* row colors */
.ship-hdr{color:#1B7A28;background:#E8F5E9;border-right:2px solid #CCC;}
.ship-cell{background:#F0FBF0;} .ship-cell.apr{background:#D8F2DA;}
.ship-cell .it::before{background:#34A853;}
.prog-hdr{color:#1565C0;background:#E3F2FD;border-right:2px solid #CCC;}
.prog-cell{background:#EEF4FE;} .prog-cell.apr{background:#DAE8FB;}
.prog-cell .it::before{background:#0078D4;}
.carry-hdr{color:#B45309;background:#FFF8E1;border-right:2px solid #CCC;}
.carry-cell{background:#FFFDE7;} .carry-cell.apr{background:#FFF0B0;}
.carry-cell .it::before{background:#F4B400;}
.block-hdr{color:#991B1B;background:#FEF2F2;border-right:2px solid #CCC;}
.block-cell{background:#FFF5F5;} .block-cell.apr{background:#FFE4E4;}
.block-cell .it::before{background:#EA4335;}
</style></head><body>
<div class="hdr">
  <div>
    <h1>Privacy Automation Release Roadmap <a href="#">⧉ ADO Backlog</a></h1>
    <div class="sub">Trusted Platform · Privacy Automation Workstream · April 2026</div>
  </div>
  
<div style="display:flex;gap:22px;align-items:center;">
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#F4B400;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>PoC Milestone
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#34A853;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>Production Release
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:8px;height:8px;border-radius:50%;background:#999;display:inline-block;flex-shrink:0;"></span>Checkpoint
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:2px;height:14px;background:#EA4335;display:inline-block;flex-shrink:0;"></span>Now (Apr 2026)
  </span>
</div>
</div>
<div class="tl-area">
  
<div style="width:230px;flex-shrink:0;display:flex;flex-direction:column;
            justify-content:space-around;padding:16px 12px 16px 0;
            border-right:1px solid #E0E0E0;">
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#0078D4;">
    M1<br/><span style="font-weight:400;color:#444;">Chatbot &amp; MS Role</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#00897B;">
    M2<br/><span style="font-weight:400;color:#444;">PDS &amp; Data Inventory</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#546E7A;">
    M3<br/><span style="font-weight:400;color:#444;">Auto Review DFD</span></div>
</div>
  <div class="tl-svg-box"><svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185" style="overflow:visible;display:block">
<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>
<line x1="0" y1="0" x2="0" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="5" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jan</text>
<line x1="260" y1="0" x2="260" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="265" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Feb</text>
<line x1="520" y1="0" x2="520" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="525" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Mar</text>
<line x1="780" y1="0" x2="780" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="785" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Apr</text>
<line x1="1040" y1="0" x2="1040" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1045" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">May</text>
<line x1="1300" y1="0" x2="1300" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1305" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jun</text>
<line x1="823" y1="0" x2="823" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
<text x="827" y="14" fill="#EA4335" font-size="10" font-weight="700" font-family="Segoe UI,Arial">NOW</text>
<line x1="0" y1="42" x2="1560" y2="42" stroke="#0078D4" stroke-width="3"/>
<circle cx="104" cy="42" r="7" fill="white" stroke="#0078D4" stroke-width="2.5"/>
<text x="104" y="26" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Jan 12</text>
<polygon points="745,31 756,42 745,53 734,42" fill="#F4B400" filter="url(#sh)"/><text x="745" y="66" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Mar 26 PoC</text>
<polygon points="1040,31 1051,42 1040,53 1029,42" fill="#34A853" filter="url(#sh)"/><text x="1040" y="18" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Apr Prod (TBD)</text>
<line x1="0" y1="98" x2="1560" y2="98" stroke="#00897B" stroke-width="3"/>
<circle cx="0" cy="98" r="7" fill="white" stroke="#00897B" stroke-width="2.5"/>
<text x="10" y="82" fill="#666" font-size="10" font-family="Segoe UI,Arial">Dec 19</text>
<circle cx="355" cy="98" r="5" fill="white" stroke="#888" stroke-width="2.5"/>
<text x="355" y="82" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Feb 11</text>
<circle cx="546" cy="98" r="4" fill="#999"/>
<circle cx="607" cy="98" r="4" fill="#999"/>
<circle cx="650" cy="98" r="4" fill="#999"/>
<circle cx="667" cy="98" r="4" fill="#999"/>
<polygon points="693,87 704,98 693,109 682,98" fill="
<!-- truncated -->
```
</details>

### `AgentDocs/0ec1d026/ReportingDashboardDesign.png`

**Type:** Design Image — engineers should reference this file visually


## Design Visual Previews

The following screenshots were rendered from the HTML design reference files. Engineers MUST match these visuals exactly.

### OriginalDesignConcept.html

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboardTest/a87e29adb6803dadc306a9889a2f90eae142feaa/AgentDocs/0ec1d026/design-screenshots/OriginalDesignConcept.png)

*Rendered from `AgentDocs/0ec1d026/OriginalDesignConcept.html` at 1920×1080*
