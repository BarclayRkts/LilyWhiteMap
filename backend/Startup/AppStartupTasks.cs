using LilyWhiteMap.Api.Data;
using LilyWhiteMap.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace LilyWhiteMap.Api.Startup;

/// <summary>One-time work run after the app starts: ensure schema exists and seed/sync players.</summary>
public static class AppStartupTasks
{
    public static void RunInBackground(WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() => _ = RunAsync(app));
    }

    private static async Task RunAsync(WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LilyWhiteMapDbContext>();
            var rosterService = scope.ServiceProvider.GetRequiredService<SpursRosterService>();

            await db.Database.EnsureCreatedAsync();
            await VisitLogSchema.EnsureCreatedAsync(db);

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
                // Players already exist; skip the slow re-scrape on every restart.
                app.Logger.LogInformation("Players table already populated; skipping startup sync.");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Initial roster sync failed during app startup");
        }
    }
}
