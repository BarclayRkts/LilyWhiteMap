namespace LilyWhiteMap.Api.Models;

public sealed record Player(
    string Id,
    string Name,
    string Location,
    double Longitude,
    double Latitude,
    string Monogram,
    string Position,
    string? Headshot,
    bool IsEstimatedLocation,
    string ProfileUrl,
    string? DateOfBirth = null,
    string? Appearances = null,
    string? Goals = null,
    string? Years = null);
