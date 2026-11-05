using System.Threading.RateLimiting;
using LilyWhiteMap.Api.Extensions;
using LilyWhiteMap.Api.Middleware;
using LilyWhiteMap.Api.Services;
using LilyWhiteMap.Api.Startup;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Production secrets (JWT signing key, admin password hash, DB connection string) come from
// SSM Parameter Store instead of plain Elastic Beanstalk environment properties. Local dev keeps
// using `dotnet user-secrets`, so this is skipped entirely outside Production.
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddSystemsManager("/lilywhitemap/prod");
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddLilyWhiteMapDatabase(builder.Configuration);

builder.Services.AddHttpClient<SpursRosterService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("LilyWhiteMap/1.0 (local development)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

// Visitor tracking: non-blocking queue + background batch writer.
builder.Services.AddSingleton<VisitLogQueue>();
builder.Services.AddHostedService<VisitLogWriterService>();

builder.Services.AddAdminJwtAuth(builder.Configuration);
builder.Services.AddFrontendCors();

// Global per-IP rate limit: throttles bots/scanners and abusive clients without affecting
// normal browsing. Rejected requests get 429 instead of consuming app/database resources.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    });
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "lilywhite-map-api" }));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendCorsExtensions.FrontendPolicyName);

app.UseRateLimiter();

app.UseMiddleware<VisitorTrackingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

AppStartupTasks.RunInBackground(app);

app.Run();

