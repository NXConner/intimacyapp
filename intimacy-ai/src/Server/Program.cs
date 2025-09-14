using IntimacyAI.Server.Data;
using IntimacyAI.Server.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultSqlite"))
);
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevAllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

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
app.Run();
