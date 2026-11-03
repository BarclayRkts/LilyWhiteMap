using LilyWhiteMap.Api.Data;
using LilyWhiteMap.Api.Models;

namespace LilyWhiteMap.Api.Services;

/// <summary>
/// Drains <see cref="VisitLogQueue"/> and persists entries in small batches so that
/// individual request threads never block on a database write.
/// </summary>
public sealed class VisitLogWriterService(
    VisitLogQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<VisitLogWriterService> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<VisitLogEntry>(BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            buffer.Clear();

            try
            {
                using var batchTimeout = new CancellationTokenSource(FlushInterval);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, batchTimeout.Token);

                while (buffer.Count < BatchSize)
                {
                    var entry = await queue.Reader.ReadAsync(linked.Token);
                    buffer.Add(entry);
                }
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // Flush interval elapsed with a partial (or empty) batch; fall through and persist below.
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (buffer.Count == 0)
            {
                continue;
            }

            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LilyWhiteMapDbContext>();
                var logs = buffer.Select(e => new VisitLog
                {
                    TimestampUtc = e.TimestampUtc,
                    Path = e.Path,
                    QueryString = e.QueryString,
                    IpAddress = e.IpAddress,
                    UserAgent = e.UserAgent,
                    Referrer = e.Referrer,
                    VisitorId = e.VisitorId,
                    ResponseTimeMs = e.ResponseTimeMs,
                });
                db.VisitLogs.AddRange(logs);
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to persist {Count} queued visit log(s); entries dropped.", buffer.Count);
            }
        }
    }
}
