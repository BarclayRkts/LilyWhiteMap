namespace LilyWhiteMap.Api.Data;

public sealed class PlayerEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string Monogram { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Headshot { get; set; }
    public bool IsEstimatedLocation { get; set; }
    public string ProfileUrl { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string? Appearances { get; set; }
    public string? Goals { get; set; }
    public string? Years { get; set; }
    public DateTime LastSyncedAtUtc { get; set; }
}
