using System.Diagnostics;
using LilyWhiteMap.Api.Models;
using LilyWhiteMap.Api.Services;

namespace LilyWhiteMap.Api.Middleware;

/// <summary>
/// Captures lightweight traffic metrics for every non-excluded request and enqueues them for
/// asynchronous persistence. Assigns an anonymous, cookie-backed VisitorId on first contact.
/// Never throws and never delays the response — all failures are swallowed/logged.
/// </summary>
public sealed class VisitorTrackingMiddleware(RequestDelegate next, ILogger<VisitorTrackingMiddleware> logger)
{
    public const string VisitorCookieName = "lwm_visitor_id";

    private static readonly string[] ExcludedPathPrefixes =
    [
        "/health",
        "/swagger",
        "/openapi",
        "/favicon.ico",
    ];

    private static readonly string[] StaticExtensions =
    [
        ".css", ".js", ".map", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf",
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (ShouldSkip(path))
        {
            await next(context);
            return;
        }

        var visitorId = EnsureVisitorId(context);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            TryEnqueue(context, path, visitorId, stopwatch.ElapsedMilliseconds);
        }
    }

    private static bool ShouldSkip(string path)
    {
        foreach (var prefix in ExcludedPathPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        foreach (var ext in StaticExtensions)
        {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string EnsureVisitorId(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(VisitorCookieName, out var existing) && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var visitorId = Guid.NewGuid().ToString("N");

        // SameSite=None + Secure is required so the cookie survives cross-origin fetches
        // from the statically-hosted Next.js frontend (different domain from the API).
        context.Response.Cookies.Append(VisitorCookieName, visitorId, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddYears(2),
            Path = "/",
        });

        return visitorId;
    }

    private void TryEnqueue(HttpContext context, string path, string visitorId, long elapsedMs)
    {
        try
        {
            var queue = context.RequestServices.GetRequiredService<VisitLogQueue>();
            var entry = new VisitLogEntry(
                TimestampUtc: DateTime.UtcNow,
                Path: path,
                QueryString: context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                IpAddress: context.Connection.RemoteIpAddress?.ToString(),
                UserAgent: context.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null,
                Referrer: context.Request.Headers.Referer.ToString() is { Length: > 0 } referrer ? referrer : null,
                VisitorId: visitorId,
                ResponseTimeMs: elapsedMs);

            if (!queue.TryEnqueue(entry))
            {
                logger.LogDebug("Visit log queue is full; dropping entry for {Path}.", path);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to enqueue visit log for {Path}.", path);
        }
    }
}
