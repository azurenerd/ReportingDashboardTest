using ExecutiveReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<ExecutiveReportingDashboard.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();