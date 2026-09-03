using System.Net;
using System.Text.RegularExpressions;
using LilyWhiteMap.Api.Data;
using LilyWhiteMap.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LilyWhiteMap.Api.Services;

public sealed partial class SpursRosterService(
    HttpClient httpClient,
    IConfiguration configuration,
    LilyWhiteMapDbContext db)
{
    private const string DefaultActionUrl =
        "https://thfcdb.com/index.php?p=actions/datastar-module&config=9ad050aa47a70493e3fead69c1b113d83ce5c8139a4615cc4501e1bed72a5e1e{%22siteId%22%3A1%2C%22route%22%3A%22people%5C%2F_results%22}";

    public async Task<IReadOnlyList<Player>> GetPlayersAsync(CancellationToken cancellationToken)
    {
        return await db.Players.AsNoTracking()
            .OrderBy(player => player.Name)
            .Select(player => ToDto(player))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Player?> GetPlayerAsync(string slug, CancellationToken cancellationToken)
    {
        var entity = await db.Players.AsNoTracking()
            .SingleOrDefaultAsync(player => player.Id == slug, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<int> SyncAsync(CancellationToken cancellationToken)
    {
        var actionUrl = configuration["SpursApi:PeopleActionUrl"] ?? DefaultActionUrl;
        var players = new List<Player>();

        for (var page = 1; page <= 100; page++)
        {
            var html = await GetPeoplePageAsync(actionUrl, page, cancellationToken);
            var pagePlayers = ParsePlayers(html);
            players.AddRange(pagePlayers);
            if (pagePlayers.Count < 100) break;
        }

        var normalized = players
            .GroupBy(player => player.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        var now = DateTime.UtcNow;
        var existing = await db.Players.ToDictionaryAsync(player => player.Id, cancellationToken);

        foreach (var player in normalized)
        {
            var entity = existing.GetValueOrDefault(player.Id) ?? new PlayerEntity { Id = player.Id };
            entity.Name = player.Name;
            entity.Location = player.Location;
            entity.Longitude = player.Longitude;
            entity.Latitude = player.Latitude;
            entity.Monogram = player.Monogram;
            entity.Position = player.Position;
            entity.Headshot = player.Headshot;
            entity.IsEstimatedLocation = player.IsEstimatedLocation;
            entity.ProfileUrl = player.ProfileUrl;
            entity.DateOfBirth = player.DateOfBirth;
            entity.Appearances = player.Appearances;
            entity.Goals = player.Goals;
            entity.LastSyncedAtUtc = now;
            if (!existing.ContainsKey(player.Id)) db.Players.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        return normalized.Length;
    }

    private async Task<string> GetPeoplePageAsync(string actionUrl, int page, CancellationToken cancellationToken)
    {
        var signals = $$"""{"section":"people","search":"","page":{{page}},"perPage":100,"orderBy":"legacy_desc","selectedView":"default","activeFilters":"","team":"","country":"","legacyList":"","personType":"","position":"","preferredFoot":""}""";
        using var response = await httpClient.GetAsync($"{actionUrl}&datastar={Uri.EscapeDataString(signals)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static IReadOnlyList<Player> ParsePlayers(string response)
    {
        var body = response.Replace("data: elements ", string.Empty);
        return PlayerAnchorRegex().Matches(body).Select(match =>
        {
            var slug = match.Groups["slug"].Value;
            var content = match.Groups["content"].Value;
            var name = CleanText(NameRegex().Match(content).Groups["name"].Value);
            var countryCode = CountryRegex().Match(content).Groups["country"].Value.ToUpperInvariant();
            var location = GetLocation(countryCode, slug);
            return new Player(slug, name, location.Name, location.Longitude, location.Latitude,
                GetMonogram(name), "Tottenham player", ImageRegex().Match(content).Groups["image"].Value, true,
                $"https://thfcdb.com/people/{slug}");
        }).Where(player => !string.IsNullOrWhiteSpace(player.Name)).ToArray();
    }

    private static Player ToDto(PlayerEntity player) => new(
        player.Id, player.Name, player.Location, player.Longitude, player.Latitude, player.Monogram,
        player.Position, player.Headshot, player.IsEstimatedLocation, player.ProfileUrl,
        player.DateOfBirth, player.Appearances, player.Goals);

    private static string CleanText(string value) => WebUtility.HtmlDecode(Regex.Replace(value, "<.*?>", string.Empty)).Trim();
    private static string CountryName(string code) => code switch
    {
        "GB" or "GB-ENG" or "UK" or "ENG" => "England", "IE" => "Ireland", "MA" => "Morocco", "EG" => "Egypt",
        "BR" => "Brazil", "AR" => "Argentina", "FR" => "France", "ES" => "Spain", "DE" => "Germany",
        "IT" => "Italy", "NL" => "Netherlands", "PT" => "Portugal", "US" => "United States",
        "AU" => "Australia", "KR" => "South Korea", "GH" => "Ghana", "CY" => "Cyprus",
        "SE" => "Sweden", "SN" => "Senegal", "BE" => "Belgium", "HR" => "Croatia",
        "NO" => "Norway", "IS" => "Iceland", "RO" => "Romania", "CI" => "Ivory Coast",
        "DK" => "Denmark", "CZ" => "Czech Republic", "JP" => "Japan", "CM" => "Cameroon",
        "ZA" => "South Africa", "NG" => "Nigeria", "CO" => "Colombia",
        "CL" => "Chile", "MX" => "Mexico", "CA" => "Canada", "NZ" => "New Zealand",
        "AT" => "Austria", "CH" => "Switzerland", "PL" => "Poland", "GR" => "Greece",
        "TR" => "Turkey", "TN" => "Tunisia", "DZ" => "Algeria", "ML" => "Mali",
        "CD" => "DR Congo", "ZW" => "Zimbabwe", "JM" => "Jamaica", "TT" => "Trinidad and Tobago",
        "UA" => "Ukraine", "RS" => "Serbia", "BA" => "Bosnia and Herzegovina", "FI" => "Finland",
        _ => string.IsNullOrWhiteSpace(code) ? "England" : code
    };
    private static string GetMonogram(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant() : name[..Math.Min(2, name.Length)].ToUpperInvariant();
    }
    private static (string Name, double Longitude, double Latitude) GetLocation(string code, string slug)
    {
        var location = code switch
        {
            "EG" => ("Cairo", 31.2357, 30.0444), "IE" => ("Dublin", -6.2603, 53.3498),
            "FR" => ("Paris", 2.3522, 48.8566), "ES" => ("Madrid", -3.7038, 40.4168),
            "DE" => ("Berlin", 13.4050, 52.5200), "IT" => ("Rome", 12.4964, 41.9028),
            "NL" => ("Amsterdam", 4.9041, 52.3676), "PT" => ("Lisbon", -9.1393, 38.7223),
            "BR" => ("Brasilia", -47.8825, -15.7942), "AR" => ("Buenos Aires", -58.3816, -34.6037),
            "US" => ("New York", -74.0060, 40.7128), "AU" => ("Sydney", 151.2093, -33.8688),
            "MA" => ("Rabat", -6.8498, 33.9716), "KR" => ("Seoul", 126.9780, 37.5665),
            "BE" => ("Brussels", 4.3517, 50.8503), "HR" => ("Zagreb", 15.9819, 45.8150),
            "NO" => ("Oslo", 10.7522, 59.9139), "IS" => ("Reykjavik", -21.9426, 64.1466),
            "RO" => ("Bucharest", 26.1025, 44.4268), "DK" => ("Copenhagen", 12.5683, 55.6761),
            "CZ" => ("Prague", 14.4378, 50.0755), "JP" => ("Tokyo", 139.6917, 35.6895),
            "CM" => ("Yaounde", 11.5021, 3.8480), "ZA" => ("Johannesburg", 28.0473, -26.2041),
            "NG" => ("Lagos", 3.3792, 6.5244), "CO" => ("Bogota", -74.0721, 4.7110),
            "CL" => ("Santiago", -70.6693, -33.4489), "MX" => ("Mexico City", -99.1332, 19.4326),
            "CA" => ("Toronto", -79.3832, 43.6532), "NZ" => ("Auckland", 174.7633, -36.8509),
            "AT" => ("Vienna", 16.3738, 48.2082), "CH" => ("Zurich", 8.5417, 47.3769),
            "PL" => ("Warsaw", 21.0122, 52.2297), "GR" => ("Athens", 23.7275, 37.9838),
            "TR" => ("Istanbul", 28.9784, 41.0082), "UA" => ("Kyiv", 30.5234, 50.4501),
            "GB" or "GB-ENG" or "UK" or "ENG" => ("London", -0.1276, 51.5072),
            _ => ("North London", -0.0166, 51.6043)
        };
        var country = CountryName(code);
        unchecked
        {
            var hash = slug.Aggregate(17, (current, character) => current * 31 + character);
            var longitude = location.Item2 + (((hash & int.MaxValue) % 11) - 5) * .08;
            var latitude = location.Item3 + ((((hash * 97) & int.MaxValue) % 9) - 4) * .05;
            return (country, longitude, latitude);
        }
    }

    [GeneratedRegex("""<a[^>]+href="https://thfcdb\.com/people/(?<slug>[^"]+)"[^>]*>(?<content>.*?)</a>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex PlayerAnchorRegex();
    [GeneratedRegex("""<h3[^>]*>.*?<span[^>]*>(?<name>.*?)</span>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex NameRegex();
    [GeneratedRegex("id=\"(?:[^\\\"]+-)?flag-icons-(?<country>[a-z]{2}(?:-[a-z]{2,3})?)\"", RegexOptions.IgnoreCase)]
    private static partial Regex CountryRegex();
    [GeneratedRegex("<img[^>]+src=\"(?<image>[^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex ImageRegex();
}
