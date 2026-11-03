using LilyWhiteMap.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LilyWhiteMap.Api.Extensions;

public static class DatabaseServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="LilyWhiteMapDbContext"/> using the connection string from (in order of
    /// precedence) appsettings "ConnectionStrings:LilyWhiteMap", POSTGRES_CONNECTION_STRING, or
    /// DATABASE_URL. Throws at startup if none are configured.
    /// </summary>
    public static IServiceCollection AddLilyWhiteMapDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LilyWhiteMap");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        }
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing PostgreSQL connection string. Set ConnectionStrings:LilyWhiteMap or POSTGRES_CONNECTION_STRING or DATABASE_URL before starting the backend.");
        }

        services.AddDbContext<LilyWhiteMapDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
