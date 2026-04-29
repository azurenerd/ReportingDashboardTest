# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and monthly execution progress in a format optimized for PowerPoint screenshot capture. The application loads all display data from a checked-in JSON configuration file, requires zero authentication or cloud dependencies, and renders a pixel-perfect 1920×1080 layout matching the provided HTML design concept (`OriginalDesignConcept.html`). This gives program managers a dead-simple, always-current project status view they can screenshot and paste directly into executive slide decks.

## Business Goals

1. **Reduce status reporting friction** — Eliminate manual PowerPoint chart creation by providing a live, screenshot-ready dashboard that is always current with project status.
2. **Improve executive visibility** — Provide a single-glance view of project health combining timeline milestones, shipped items, in-progress work, carryovers, and blockers.
3. **Enable self-service updates** — Allow any team member to update project status by editing a simple JSON file—no database, no deployment, no credentials required.
4. **Maximize screenshot fidelity** — Deliver a fixed 1920×1080 layout that produces clean, professional images for executive presentations without cropping or resizing.
5. **Minimize operational overhead** — Zero infrastructure cost, zero authentication, zero external dependencies. Clone, run, use.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** program manager, I want to see the project title, organizational context, and a link to the ADO backlog at the top of the dashboard, so that executives immediately know which project and time period they are looking at.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` class.

**Acceptance Criteria:**
- [ ] Dashboard displays the project title in bold 24px font on the left side of the header
- [ ] A clickable hyperlink to the ADO backlog appears inline with the title
- [ ] Subtitle displays team/workstream name and current reporting month in 12px gray text
- [ ] Legend icons appear on the right side of the header showing: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), Now indicator (red vertical line)
- [ ] All values are driven by the JSON configuration file

### US-2: View Milestone Timeline

**As an** executive, I want to see a horizontal timeline showing major project milestones across months, so that I can understand delivery cadence and upcoming dates at a glance.

**Visual Reference:** Timeline area of `OriginalDesignConcept.html` — `.tl-area` and SVG section.

**Acceptance Criteria:**
- [ ] Timeline displays month grid lines (Jan–Jun or as configured) with labeled columns
- [ ] Each milestone track is rendered as a colored horizontal line with its label on the left sidebar
- [ ] Checkpoints appear as open circles on the milestone track
- [ ] PoC milestones appear as gold diamonds with drop shadow and date label
- [ ] Production releases appear as green diamonds with drop shadow and date label
- [ ] A red dashed vertical "NOW" line indicates the current date position
- [ ] Milestone data (dates, types, positions) is entirely driven by JSON configuration
- [ ] Supports 1–5 milestone tracks without layout breakage

### US-3: View Monthly Execution Heatmap

**As an** executive, I want to see a color-coded grid showing what shipped, what's in progress, what carried over, and what's blocked each month, so that I can assess execution health and identify problems.

**Visual Reference:** Heatmap grid of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` classes.

**Acceptance Criteria:**
- [ ] Grid displays with row categories: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Grid displays month columns matching the configured months, with the current month highlighted
- [ ] Each cell lists work items as bullet points with colored dot indicators
- [ ] Empty cells display a dash placeholder
- [ ] Current month column has a visually distinct highlighted background
- [ ] Row headers use uppercase text with category-appropriate colors
- [ ] All items and categories are driven by JSON configuration

### US-4: Load Dashboard Data from Configuration File

**As a** team member, I want to update the dashboard by editing a single JSON file, so that I don't need developer skills, database access, or deployment pipelines to keep the status current.

**Acceptance Criteria:**
- [ ] Application reads all display data from `wwwroot/data/dashboard-data.json`
- [ ] A sample file `wwwroot/data/dashboard-data.sample.json` ships with the repository containing fictional example data
- [ ] JSON schema is human-readable with clear property names and no computed fields
- [ ] Changes to the JSON file are reflected on page refresh (no restart required)
- [ ] Application displays a clear error message if JSON file is missing or malformed
- [ ] JSON file is diffable and mergeable in Git

### US-5: Run Dashboard Locally with Zero Setup

**As a** developer, I want to clone the repo and run `dotnet run` to see the dashboard immediately, so that there is no setup friction or dependency on external services.

**Acceptance Criteria:**
- [ ] Application starts with `dotnet run` from the project directory
- [ ] No database connection, API keys, or external services required
- [ ] Dashboard is accessible at `http://localhost:5000` (or configured port)
- [ ] No authentication prompts or login screens
- [ ] README documents the single command needed to run

### US-6: Capture Screenshot for Executive Presentations

**As a** program manager, I want the dashboard to render at exactly 1920×1080 pixels with no scrollbars, so that I can take a browser screenshot and paste it directly into a PowerPoint slide without resizing.

**Acceptance Criteria:**
- [ ] Page body is fixed at 1920px width and 1080px height
- [ ] `overflow: hidden` prevents any scrollbars
- [ ] All content fits within the viewport without clipping important information
- [ ] Page renders identically in Chrome and Edge
- [ ] No browser chrome (navigation bars, tabs) is needed in the screenshot area

## Visual Design Specification

**Reference File:** `AgentDocs/0ec1d026/OriginalDesignConcept.html` and `AgentDocs/0ec1d026/ReportingDashboardDesign.png`

### Overall Layout

- **Viewport:** Fixed 1920×1080px, `overflow: hidden`
- **Direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Background:** `#FFFFFF`
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (flex-shrink: 0), approximately 50px
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Layout:** Flexbox, `align-items: center; justify-content: space-between`
- **Left side:**
  - Title: `font-size: 24px; font-weight: 700` with inline hyperlink (`color: #0078D4`)
  - Subtitle: `font-size: 12px; color: #888; margin-top: 2px`
- **Right side (Legend):**
  - Horizontal flex with `gap: 22px`
  - Each legend item: `font-size: 12px` with inline shape indicator
  - PoC Milestone: 12×12px gold (`#F4B400`) square rotated 45°
  - Production Release: 12×12px green (`#34A853`) square rotated 45°
  - Checkpoint: 8×8px gray (`#999`) circle
  - Now indicator: 2×14px red (`#EA4335`) vertical bar

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px
- **Background:** `#FAFAFA`
- **Padding:** `6px 44px 0`
- **Border:** Bottom `2px solid #E8E8E8`
- **Layout:** Flexbox horizontal (`align-items: stretch`)
- **Left Sidebar (Milestone Labels):**
  - Width: 230px, flex-shrink: 0
  - Vertical flex with `justify-content: space-around`
  - Border right: `1px solid #E0E0E0`
  - Each label: `font-size: 12px; font-weight: 600` with milestone ID in color and description in `#444`
  - Milestone colors: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`
- **SVG Timeline Area (`.tl-svg-box`):**
  - Flex: 1 (fills remaining width)
  - SVG dimensions: 1560×185px, `overflow: visible`
  - Month grid lines: vertical `stroke: #bbb; stroke-opacity: 0.4`
  - Month labels: `font-size: 11; font-weight: 600; fill: #666`
  - NOW line: `stroke: #EA4335; stroke-width: 2; stroke-dasharray: 5,3`
  - NOW label: `font-size: 10; font-weight: 700; fill: #EA4335`
  - Milestone tracks: horizontal lines with `stroke-width: 3` in milestone color
  - Checkpoints: `<circle>` with white fill, colored stroke, `stroke-width: 2.5`
  - PoC diamonds: `<polygon>` filled `#F4B400` with drop shadow filter
  - Production diamonds: `<polygon>` filled `#34A853` with drop shadow filter
  - Date labels: `font-size: 10; fill: #666; text-anchor: middle`
  - Drop shadow filter: `feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1; min-height: 0`
- **Padding:** `10px 44px 10px`
- **Title:** `font-size: 14px; font-weight: 700; color: #888; text-transform: uppercase; letter-spacing: 0.5px`
- **Grid (`.hm-grid`):**
  - CSS Grid: `grid-template-columns: 160px repeat(4, 1fr)`
  - CSS Grid: `grid-template-rows: 36px repeat(4, 1fr)`
  - Border: `1px solid #E0E0E0`
  - Flex: 1 (fills remaining vertical space)

#### Grid Header Row
- **Corner cell:** `background: #F5F5F5; font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase`
- **Column headers:** `font-size: 16px; font-weight: 700; background: #F5F5F5; border-bottom: 2px solid #CCC`
- **Current month highlight:** `background: #FFF0D0; color: #C07700`

#### Grid Data Rows (per category)

| Category | Header BG | Header Text | Cell BG | Current Month BG | Dot Color |
|----------|-----------|-------------|---------|------------------|-----------|
| Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Grid Cell Items
- **Font:** `font-size: 12px; color: #333; line-height: 1.35`
- **Padding:** `2px 0 2px 12px` (relative positioned)
- **Bullet dot:** `::before` pseudo-element, `width: 6px; height: 6px; border-radius: 50%` positioned absolutely at `left: 0; top: 7px`
- **Row headers:** `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px; border-right: 2px solid #CCC`

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders with Project Data**
User navigates to `http://localhost:5000`. The page loads and immediately displays the full dashboard: header with project title and legend, timeline with milestone markers, and the heatmap grid with status items. All data is read from `dashboard-data.json`. The page is fully rendered within 2 seconds with no loading spinner needed.

**Scenario 2: User Views Timeline and Identifies Current Position**
User looks at the timeline section and sees a red dashed vertical "NOW" line indicating today's position relative to milestones. Gold diamonds mark PoC dates and green diamonds mark production release dates. The user can immediately see which milestones are ahead vs. behind the current date.

**Scenario 3: User Scans Heatmap for Monthly Execution Health**
User looks at the heatmap grid and sees the current month column highlighted in a warm gold tone. Shipped items in green rows indicate completed work; blockers in red rows highlight immediate risks. The visual density of items per cell communicates velocity at a glance.

**Scenario 4: User Clicks ADO Backlog Link**
User clicks the hyperlink next to the project title. A new browser tab opens to the configured Azure DevOps backlog URL. The dashboard remains unchanged in the original tab.

**Scenario 5: User Takes Screenshot for PowerPoint**
User presses their screenshot tool (Win+Shift+S or Snipping Tool) and captures the browser viewport. The resulting image is exactly 1920×1080 and contains the complete dashboard with no scrollbars, no clipped content, and no browser chrome within the capture area.

**Scenario 6: Data-Driven Rendering — JSON Updated with New Items**
User edits `dashboard-data.json` to add a new item under "Shipped" for the current month. User refreshes the browser. The heatmap grid immediately shows the new item with a green bullet dot in the appropriate cell.

**Scenario 7: Empty State — Category with No Items**
A month/category cell in the JSON contains an empty array. The dashboard renders a single gray dash character (`-`) in that cell rather than leaving it blank, maintaining grid visual consistency.

**Scenario 8: Error State — Missing or Malformed JSON File**
The `dashboard-data.json` file is deleted or contains invalid JSON. The application displays a centered error message on the page: "Dashboard data could not be loaded. Please check wwwroot/data/dashboard-data.json." No stack trace or technical error is shown.

**Scenario 9: Multiple Milestones Rendered**
The JSON file defines 3 milestone tracks (M1, M2, M3). The timeline renders three horizontal colored lines vertically spaced at even intervals, each with their own set of checkpoint circles and diamond markers. Labels appear in the left sidebar.

**Scenario 10: Current Month Column Highlighting**
The `currentMonth` field in JSON is set to "Apr". The April column header in the heatmap renders with a warm gold background (`#FFF0D0`) and darker text (`#C07700`). All data cells in that column render with the highlighted variant of their category color.

## Scope

### In Scope

- Single-page Blazor Server application (.NET 8) rendering the executive dashboard
- Header component with configurable title, subtitle, backlog URL, and legend
- SVG timeline component with configurable milestones, events, and "NOW" indicator
- CSS Grid heatmap component with 4 status categories × N month columns
- JSON configuration file (`dashboard-data.json`) as the sole data source
- Sample JSON file (`dashboard-data.sample.json`) with fictional project data
- Fixed 1920×1080 layout optimized for screenshot capture
- CSS styling matching the reference design pixel-for-pixel
- Local-only hosting via Kestrel (`dotnet run`)
- README with setup instructions

### Out of Scope

- Authentication or authorization of any kind
- User login or role-based access control
- Database or persistent storage backend
- REST API or external data integrations
- Responsive/mobile layout or breakpoints
- JavaScript interactivity (hover tooltips, animations, drag-and-drop)
- Multi-page navigation or routing beyond `/`
- Real-time auto-refresh or WebSocket push on file change (Phase 2)
- Print/PDF export functionality
- Multi-project switching via query parameters (Phase 2)
- JSON editor UI for non-technical users (Phase 2)
- CI/CD pipeline or automated deployment
- Docker containerization
- Unit tests or integration tests (nice-to-have, not blocking)
- Accessibility compliance (WCAG)
- Internationalization or localization
- Dark mode or theme switching

## Non-Functional Requirements

| Requirement | Target | Rationale |
|-------------|--------|-----------|
| **Page Load Time** | < 2 seconds on localhost | Dashboard must feel instant for screenshot workflow |
| **Rendering Fidelity** | Pixel-match to `OriginalDesignConcept.html` at 1920×1080 | Screenshots go directly into executive decks |
| **Browser Support** | Chrome 120+ and Edge 120+ | Windows enterprise standard; these share Chromium renderer |
| **JSON File Size** | Support up to 100KB config file | Enough for 12 months × 4 categories × 20 items each |
| **Startup Time** | < 5 seconds from `dotnet run` to browser-ready | Developer experience during iteration |
| **Memory Usage** | < 100MB RSS | Single-user local app should not tax the machine |
| **Zero External Dependencies** | No network calls at runtime | Works offline, air-gapped, on any corporate network |
| **Security** | None required | Local-only, no PII, no auth, no encryption |
| **Availability** | N/A (local tool) | User starts/stops as needed |
| **Data Freshness** | Reflects JSON file state on each page load/refresh | No caching beyond single request lifecycle |

## Success Metrics

1. **Functional Completeness** — All three visual sections (header, timeline, heatmap) render correctly from JSON data with zero hardcoded values.
2. **Visual Fidelity** — Side-by-side comparison of dashboard screenshot vs. `OriginalDesignConcept.html` reference shows < 5% deviation in layout, colors, and typography.
3. **Setup Simplicity** — A new developer can clone the repo and see the running dashboard in under 3 commands (`git clone`, `cd`, `dotnet run`).
4. **Data Editability** — A non-developer can update the JSON file, refresh the browser, and see changes reflected within 5 seconds.
5. **Screenshot Quality** — A full-page browser screenshot at 1920×1080 produces an image suitable for direct insertion into a PowerPoint deck without manual resizing or cropping.
6. **Zero Dependencies** — Application runs with only the .NET 8 SDK installed; no additional NuGet packages beyond the default Blazor Server template.

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8.0 LTS (Blazor Server) — already established in the repository's tech stack
- **Fixed Resolution:** 1920×1080px — non-negotiable for PowerPoint screenshot use case
- **No JavaScript:** All rendering via server-side Blazor + CSS + inline SVG
- **No External Packages:** Zero NuGet additions beyond the default `dotnet new blazor` template
- **Local Hosting Only:** Kestrel on localhost; no reverse proxy, no HTTPS certificate management
- **Windows Primary:** Target developer machines run Windows with Segoe UI font available natively

### Timeline Assumptions

- **Implementation Effort:** 2–3 developer days for Phase 1 MVP
- **Iteration Cycle:** Developer uses `dotnet watch` for rapid visual iteration against the reference design
- **Delivery:** Single PR containing the complete working application with sample data

### Dependency Assumptions

- Developer has .NET 8 SDK installed (free download)
- Developer has Chrome or Edge for viewing/screenshotting
- JSON file is edited manually by the PM or tech lead (no editor UI needed for v1)
- The reference design file (`OriginalDesignConcept.html`) is the authoritative visual specification
- `ReportingDashboardDesign.png` provides supplementary visual context for ambiguous areas

### Data Assumptions

- Dashboard reports on a single project per instance
- Data changes infrequently (weekly to monthly)
- Maximum 6 months displayed on the timeline
- Maximum 4 status categories in the heatmap (Shipped, In Progress, Carryover, Blockers)
- Maximum 5 milestone tracks in the timeline
- Maximum 10 items per heatmap cell before visual overflow