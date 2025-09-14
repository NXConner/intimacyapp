using IntimacyAI.Server.Data;
using IntimacyAI.Server.Models;
using IntimacyAI.Server.Security;
using IntimacyAI.Server.Services;
using IntimacyAI.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using IntimacyAI.Server.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IntimacyAI API", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. Use 'X-API-Key' header.",
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultSqlite"))
);
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevAllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddSignalR();
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter("fixed", options =>
{
    options.Window = TimeSpan.FromSeconds(1);
    options.PermitLimit = 20;
    options.QueueLimit = 0;
}));
builder.Services.AddSingleton<IAnalysisQueue, AnalysisQueue>();
builder.Services.AddHostedService<AnalysisWorker>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<AppDbContext>(name: "db");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors("DevAllowAll");
app.UseRateLimiter();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/api/analytics", async (AppDbContext db) =>
{
    return await db.UsageAnalytics.OrderByDescending(x => x.Id).Take(100).ToListAsync();
}).WithOpenApi();

app.MapPost("/api/analytics", async (UsageAnalytics payload, AppDbContext db) =>
{
    payload.CreatedAtUtc = DateTime.UtcNow;
    db.UsageAnalytics.Add(payload);
    await db.SaveChangesAsync();
    return Results.Created($"/api/analytics/{payload.Id}", payload);
}).WithOpenApi();

app.MapGet("/api/model-performance", async (AppDbContext db) =>
{
    return await db.ModelPerformances.OrderByDescending(x => x.Id).Take(100).ToListAsync();
}).WithOpenApi();

app.MapPost("/api/model-performance", async (ModelPerformance payload, AppDbContext db) =>
{
    payload.CreatedAtUtc = DateTime.UtcNow;
    db.ModelPerformances.Add(payload);
    await db.SaveChangesAsync();
    return Results.Created($"/api/model-performance/{payload.Id}", payload);
}).WithOpenApi();

app.MapHub<AnalysisHub>("/hubs/analysis").RequireRateLimiting("fixed");

app.MapPost("/api/analyze", async (HttpRequest request, IAnalysisQueue queue) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("multipart/form-data required");
    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0) return Results.BadRequest("file is required");
    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    var req = new AnalysisRequest
    {
        AnalysisType = "image",
        Data = ms.ToArray(),
        Metadata = new Dictionary<string, string> { { "filename", file.FileName } }
    };
    queue.Enqueue(req);
    return Results.Accepted($"/api/analysis/{req.SessionId}", new { sessionId = req.SessionId });
}).DisableAntiforgery().RequireRateLimiting("fixed").WithOpenApi();

app.MapGet("/api/analysis/{sessionId}", async (string sessionId, AppDbContext db) =>
{
    var item = await db.AnalysisHistories.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.SessionId == sessionId);
    return item is null ? Results.NotFound() : Results.Ok(item);
}).RequireRateLimiting("fixed").WithOpenApi();

app.MapGet("/api/preferences/{userId}", async (string userId, AppDbContext db, IEncryptionService enc) =>
{
    var pref = await db.UserPreferences.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.UserId == userId);
    if (pref is null) return Results.NotFound();
    Dictionary<string, object>? prefs = null;
    if (!string.IsNullOrWhiteSpace(pref.PreferencesJsonEncrypted))
    {
        var json = enc.Decrypt(pref.PreferencesJsonEncrypted!);
        prefs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    }
    return Results.Ok(new PreferencesDto { UserId = userId, Preferences = prefs });
}).WithOpenApi();

app.MapPost("/api/preferences/{userId}", async (string userId, Dictionary<string, object> payload, AppDbContext db, IEncryptionService enc) =>
{
    var json = System.Text.Json.JsonSerializer.Serialize(payload);
    var encrypted = enc.Encrypt(json);
    var now = DateTime.UtcNow;
    var entity = await db.UserPreferences.FirstOrDefaultAsync(x => x.UserId == userId);
    if (entity is null)
    {
        entity = new UserPreferences { UserId = userId, PreferencesJsonEncrypted = encrypted, CreatedAtUtc = now, UpdatedAtUtc = now };
        db.UserPreferences.Add(entity);
    }
    else
    {
        entity.PreferencesJsonEncrypted = encrypted;
        entity.UpdatedAtUtc = now;
    }
    await db.SaveChangesAsync();
    return Results.Ok(entity);
}).WithOpenApi();

app.MapPost("/api/coaching/{sessionId}", async (string sessionId, Dictionary<string, object> payload, AppDbContext db, IEncryptionService enc) =>
{
    var encrypted = enc.Encrypt(System.Text.Json.JsonSerializer.Serialize(payload));
    var entity = new CoachingSession { SessionId = sessionId, SuggestionsJsonEncrypted = encrypted, CreatedAtUtc = DateTime.UtcNow };
    db.CoachingSessions.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/coaching/{entity.Id}", entity);
}).WithOpenApi();

app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "ok", database = canConnect ? "connected" : "unavailable" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Health check failed: {ex.Message}");
    }
}).WithOpenApi();
app.MapHealthChecks("/healthz");
app.Run();
