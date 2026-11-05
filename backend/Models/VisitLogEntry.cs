namespace LilyWhiteMap.Api.Models;

/// <summary>Lightweight DTO captured on the request hot path before being queued for persistence.</summary>
public sealed record VisitLogEntry(
    DateTime TimestampUtc,
    string Path,
    string? QueryString,
    string? IpAddress,
    string? UserAgent,
    string? Referrer,
    string VisitorId,
    long ResponseTimeMs,
    bool IsSuspicious);
