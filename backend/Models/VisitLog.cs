namespace LilyWhiteMap.Api.Models;

public sealed class VisitLog
{
    public long Id { get; set; }

    public DateTime TimestampUtc { get; set; }

    public string Path { get; set; } = string.Empty;

    public string? QueryString { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Referrer { get; set; }
    
    public string VisitorId { get; set; } = string.Empty;

    public long ResponseTimeMs { get; set; }
}
