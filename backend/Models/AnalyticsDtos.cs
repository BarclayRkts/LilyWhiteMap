namespace LilyWhiteMap.Api.Models;

public sealed record TrafficSummary(
    long TotalViewsAllTime,
    long TotalViewsToday,
    long TotalViewsLast7Days,
    long UniqueVisitorsToday,
    long UniqueVisitorsLast7Days,
    long UniqueVisitorsAllTime,
    double AverageResponseTimeMs);

public sealed record RecentActivityItem(
    long Id,
    DateTime TimestampUtc,
    string Path,
    string? QueryString,
    string? IpAddress,
    string? Referrer,
    string VisitorId,
    long ResponseTimeMs);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);

public sealed record RouteCount(string Path, long Views);

public sealed record ReferrerCount(string Referrer, long Views);
