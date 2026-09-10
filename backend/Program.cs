using LilyWhiteMap.Api.Services;
using LilyWhiteMap.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var connString = builder.Configuration.GetConnectionString("LilyWhiteMap");
if (string.IsNullOrWhiteSpace(connString))
{
    connString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
}
if (string.IsNullOrWhiteSpace(connString))
{
    connString = Environment.GetEnvironmentVariable("DATABASE_URL");
}

if (string.IsNullOrWhiteSpace(connString))
{
    throw new InvalidOperationException("Missing PostgreSQL connection string. Set ConnectionStrings:LilyWhiteMap or POSTGRES_CONNECTION_STRING or DATABASE_URL before starting the backend.");
}

builder.Services.AddDbContext<LilyWhiteMapDbContext>(options =>
    options.UseNpgsql(connString));
builder.Services.AddHttpClient<SpursRosterService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("LilyWhiteMap/1.0 (local development)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "https://master.d3uj4k8iqldq7q.amplifyapp.com",
                "https://tottenhamplayersmap.com",
                "https://www.tottenhamplayersmap.com"
                )
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "lilywhite-map-api" }));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LilyWhiteMapDbContext>();
            var rosterService = scope.ServiceProvider.GetRequiredService<SpursRosterService>();

            await db.Database.EnsureCreatedAsync();

            var hasPlayers = await db.Players.AnyAsync();
            if (!hasPlayers)
            {
                try
                {
                    var syncedCount = await rosterService.SyncAsync(CancellationToken.None);
                    app.Logger.LogInformation("Startup sync imported {Count} players.", syncedCount);
                    if (syncedCount == 0)
                    {
                        throw new InvalidOperationException("Live roster sync returned zero players; falling back to the seeded dataset.");
                    }
                }
                catch (Exception ex)
                {
                    app.Logger.LogWarning(ex, "Live roster sync failed on startup; seeding fallback players instead.");
                    await rosterService.SeedFallbackPlayersAsync(CancellationToken.None);
                }
            }
            else
            {
                var syncedCount = await rosterService.SyncAsync(CancellationToken.None);
                app.Logger.LogInformation("Refresh sync imported {Count} players.", syncedCount);
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Initial roster sync failed during app startup");
        }
    });
});

app.Run();
