using LilyWhiteMap.Api.Services;
using LilyWhiteMap.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LilyWhiteMapDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LilyWhiteMap")));
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
                "http://127.0.0.1:3000")
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

            if (!await db.Players.AnyAsync())
            {
                await rosterService.SeedFallbackPlayersAsync(CancellationToken.None);
            }

            await rosterService.SyncAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Initial roster sync failed during app startup");
        }
    });
});

app.Run();
