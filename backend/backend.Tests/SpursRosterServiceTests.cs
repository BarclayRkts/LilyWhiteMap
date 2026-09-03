using System.Net;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using LilyWhiteMap.Api.Data;
using LilyWhiteMap.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LilyWhiteMap.Api.Tests;

public sealed class SpursRosterServiceTests
{
    [Fact]
    public async Task GetPlayersAsync_returns_players_sorted_by_name()
    {
        await using var db = CreateDatabase();
        db.Players.AddRange(
            new PlayerEntity { Id = "z", Name = "Zack", Location = "England" },
            new PlayerEntity { Id = "a", Name = "Aaron", Location = "Ireland" });
        await db.SaveChangesAsync();
        var service = CreateService(db, new HttpClient());

        var players = await service.GetPlayersAsync(CancellationToken.None);

        Assert.Equal(["Aaron", "Zack"], players.Select(player => player.Name));
    }

    [Fact]
    public async Task SyncAsync_imports_country_and_coordinates_from_thfcdb_markup()
    {
        await using var db = CreateDatabase();
        var handler = new StubHandler("""
            event: datastar-patch-elements
            data: elements <div id="body">
            <a href="https://thfcdb.com/people/harry-kane">
              <img src="https://example.test/harry.webp">
              <h3><span>Harry Kane</span></h3>
              <svg id="flag-icons-gb-eng"></svg>
            </a>
            """);
        var service = CreateService(db, new HttpClient(handler));

        var imported = await service.SyncAsync(CancellationToken.None);
        var player = await db.Players.SingleAsync();

        Assert.Equal(1, imported);
        Assert.Equal("Harry Kane", player.Name);
        Assert.Equal("England", player.Location);
        Assert.Equal("https://thfcdb.com/people/harry-kane", player.ProfileUrl);
        Assert.InRange(player.Longitude, -0.2, 0.2);
        Assert.InRange(player.Latitude, 51.2, 51.9);
    }

    private static LilyWhiteMapDbContext CreateDatabase() =>
        new(new DbContextOptionsBuilder<LilyWhiteMapDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static SpursRosterService CreateService(LilyWhiteMapDbContext db, HttpClient client) =>
        new(client, new ConfigurationBuilder().Build(), db);

    private sealed class StubHandler(string response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response),
            });
    }
}
