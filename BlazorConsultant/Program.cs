using BlazorConsultant.Services;
using MudBlazor.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Enable static web assets from NuGet packages (required for MudBlazor)
builder.WebHost.UseStaticWebAssets();

// ============================================================================
// Service Configuration
// ============================================================================

// Configure forwarded headers for fly.io proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    // Increase circuit timeout for better stability on fly.io
    options.DetailedErrors = true;
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
});

// Add HttpClient for Python API communication
builder.Services.AddHttpClient("PythonAPI", client =>
{
    var pythonApiUrl = builder.Configuration["PythonApi:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(pythonApiUrl);
    client.Timeout = TimeSpan.FromMinutes(5); // Long timeout for streaming
});

// Add MudBlazor services
builder.Services.AddMudServices();

// Add scoped services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IInstructionService, InstructionService>();
builder.Services.AddScoped<IChatStateService, ChatStateService>();
builder.Services.AddScoped<IMultiAgentService, MultiAgentService>();

// ============================================================================
// Application Pipeline
// ============================================================================

var app = builder.Build();

// Use forwarded headers FIRST (required for fly.io proxy)
app.UseForwardedHeaders();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    // Don't use HTTPS redirection - fly.io handles this
}

app.UseStaticFiles();

app.UseRouting();

// Map Blazor endpoints
app.MapBlazorHub();
app.MapRazorPages();

app.MapFallbackToPage("/_Host");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "blazor-consultant" }));

Console.WriteLine($"✅ [STARTUP] TailorBlend AI Consultant - Blazor Server");
Console.WriteLine($"✅ [STARTUP] Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"✅ [STARTUP] Python API: {builder.Configuration["PythonApi:BaseUrl"]}");
Console.WriteLine($"✅ [STARTUP] ASPNETCORE_URLS: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "not set"}");

app.Run();
