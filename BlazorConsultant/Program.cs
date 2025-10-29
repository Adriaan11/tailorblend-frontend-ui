using BlazorConsultant.Data;
using BlazorConsultant.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

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

// Add HttpClient for Python API communication with SSE streaming optimizations
builder.Services.AddHttpClient("PythonAPI", client =>
{
    var pythonApiUrl = builder.Configuration["PythonApi:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(pythonApiUrl);

    // Use infinite timeout - we'll control timeouts via CancellationToken in services
    // This prevents the HttpClient from terminating long-running SSE streams
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    // Connection pool settings optimized for SSE
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),  // Refresh connections periodically
    ResponseDrainTimeout = TimeSpan.FromSeconds(30),      // Time to drain responses before closing
    EnableMultipleHttp2Connections = false                // Disable HTTP/2 multiplexing for simpler streaming
});

// Add database services for prompt management
builder.Services.AddScoped<ISystemPromptRepository, SystemPromptRepository>();
builder.Services.AddScoped<IPromptManagementService, PromptManagementService>();
builder.Services.AddSingleton<DatabaseSetupService>();

// Add scoped services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IInstructionService, InstructionService>();
builder.Services.AddScoped<IChatStateService, ChatStateService>();
builder.Services.AddScoped<IMultiAgentService, MultiAgentService>();
// SseStreamManager removed - now using client-side StreamSimulator for fake streaming

// ============================================================================
// Application Pipeline
// ============================================================================

var app = builder.Build();

// ============================================================================
// Database Setup (Auto-Migration)
// ============================================================================

// Ensure database schema exists on startup
using (var scope = app.Services.CreateScope())
{
    var dbSetup = scope.ServiceProvider.GetRequiredService<DatabaseSetupService>();
    try
    {
        await dbSetup.EnsureDatabaseSetupAsync();
        Console.WriteLine("✅ [DATABASE] Schema verification completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ [DATABASE] Setup failed: {ex.Message}");
        Console.WriteLine($"   Connection string check: {builder.Configuration.GetConnectionString("DefaultConnection") != null}");
        // Don't exit - app can still function without database features
    }
}

// Use forwarded headers FIRST (required for fly.io proxy)
app.UseForwardedHeaders();

// Security headers middleware
app.Use(async (context, next) =>
{
    // Clickjacking protection
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // HSTS (only in production, fly.io handles HTTPS)
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append(
            "Strict-Transport-Security",
            "max-age=31536000; includeSubDomains");
    }

    // CSP - Blazor Server requires unsafe-inline and unsafe-eval for SignalR
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' ws: wss:; " +
        "frame-ancestors 'self'");

    // Cross-Origin isolation for better security
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");

    await next();
});

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    // Don't use HTTPS redirection - fly.io handles this
}

// Static files with cache headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache assets with version query string for 1 year
        if (ctx.Context.Request.Query.ContainsKey("v"))
        {
            ctx.Context.Response.Headers.Append(
                "Cache-Control", "public,max-age=31536000,immutable");
        }
        else
        {
            // Default: allow caching but revalidate
            ctx.Context.Response.Headers.Append(
                "Cache-Control", "public,max-age=3600");
        }
    }
});

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
