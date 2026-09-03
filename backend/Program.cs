using LilyWhiteMap.Api.Services;
using LilyWhiteMap.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LilyWhiteMapDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LilyWhiteMap")));
builder.Services.AddHttpClient<SpursRosterService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("LilyWhiteMap/1.0 (local development)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LilyWhiteMapDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Players.AnyAsync())
    {
        await scope.ServiceProvider.GetRequiredService<SpursRosterService>()
            .SyncAsync(CancellationToken.None);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.MapControllers();

app.Run();
