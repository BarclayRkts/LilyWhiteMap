using LilyWhiteMap.Api.Data;
using LilyWhiteMap.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LilyWhiteMap.Api.Controllers;

/// <summary>Read-only traffic analytics for the admin dashboard. Requires a valid admin JWT.</summary>
[ApiController]
[Authorize]
[Route("api/admin/analytics")]
public sealed class AdminAnalyticsController(LilyWhiteMapDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<TrafficSummary>> GetSummary(CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var todayStart = nowUtc.Date;
        var sevenDaysAgo = nowUtc.AddDays(-7);

        var logs = db.VisitLogs.AsNoTracking();

        var totalAllTime = await logs.LongCountAsync(cancellationToken);
        var totalToday = await logs.LongCountAsync(v => v.TimestampUtc >= todayStart, cancellationToken);
        var totalLast7Days = await logs.LongCountAsync(v => v.TimestampUtc >= sevenDaysAgo, cancellationToken);

        var uniqueToday = await logs.Where(v => v.TimestampUtc >= todayStart)
            .Select(v => v.VisitorId).Distinct().LongCountAsync(cancellationToken);
        var uniqueLast7Days = await logs.Where(v => v.TimestampUtc >= sevenDaysAgo)
            .Select(v => v.VisitorId).Distinct().LongCountAsync(cancellationToken);
        var uniqueAllTime = await logs.Select(v => v.VisitorId).Distinct().LongCountAsync(cancellationToken);

        var avgResponseMs = totalAllTime == 0
            ? 0
            : await logs.AverageAsync(v => (double)v.ResponseTimeMs, cancellationToken);

        return Ok(new TrafficSummary(
            TotalViewsAllTime: totalAllTime,
            TotalViewsToday: totalToday,
            TotalViewsLast7Days: totalLast7Days,
            UniqueVisitorsToday: uniqueToday,
            UniqueVisitorsLast7Days: uniqueLast7Days,
            UniqueVisitorsAllTime: uniqueAllTime,
            AverageResponseTimeMs: Math.Round(avgResponseMs, 1)));
    }

    [HttpGet("recent")]
    public async Task<ActionResult<PagedResult<RecentActivityItem>>> GetRecentActivity(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = db.VisitLogs.AsNoTracking().OrderByDescending(v => v.TimestampUtc);

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new RecentActivityItem(
                v.Id, v.TimestampUtc, v.Path, v.QueryString, v.IpAddress, v.Referrer, v.VisitorId, v.ResponseTimeMs))
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<RecentActivityItem>(items, page, pageSize, totalCount));
    }

    [HttpGet("top-routes")]
    public async Task<ActionResult<IReadOnlyList<RouteCount>>> GetTopRoutes(
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);

        var results = await db.VisitLogs.AsNoTracking()
            .GroupBy(v => v.Path)
            .Select(g => new { Path = g.Key, Views = g.Count() })
            .OrderByDescending(r => r.Views)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Ok(results.Select(r => new RouteCount(r.Path, r.Views)));
    }

    [HttpGet("top-referrers")]
    public async Task<ActionResult<IReadOnlyList<ReferrerCount>>> GetTopReferrers(
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);

        var results = await db.VisitLogs.AsNoTracking()
            .Where(v => v.Referrer != null && v.Referrer != "")
            .GroupBy(v => v.Referrer!)
            .Select(g => new { Referrer = g.Key, Views = g.Count() })
            .OrderByDescending(r => r.Views)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Ok(results.Select(r => new ReferrerCount(r.Referrer, r.Views)));
    }
}
