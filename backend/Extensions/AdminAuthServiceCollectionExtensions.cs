using System.Text;
using LilyWhiteMap.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LilyWhiteMap.Api.Extensions;

public static class AdminAuthServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT bearer authentication for the admin dashboard. The signing key comes from
    /// (in order of precedence) appsettings "Jwt:SigningKey" or the ADMIN_JWT_SECRET env var, and
    /// must be at least 32 bytes (256 bits) since tokens are signed with HS256.
    /// </summary>
    public static IServiceCollection AddAdminJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var signingKey = ResolveSigningKey(configuration);

        var jwtOptions = new JwtOptions
        {
            SigningKey = signingKey,
            Issuer = configuration["Jwt:Issuer"] ?? "LilyWhiteMap.Api",
            Audience = configuration["Jwt:Audience"] ?? "LilyWhiteMap.Admin",
            ExpiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var minutes) ? minutes : 60,
        };

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtOptions));
        services.AddSingleton<JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });
        services.AddAuthorization();

        return services;
    }

    private static string ResolveSigningKey(IConfiguration configuration)
    {
        var signingKey = configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            signingKey = Environment.GetEnvironmentVariable("ADMIN_JWT_SECRET");
        }

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "Missing JWT signing key. Set Jwt:SigningKey or ADMIN_JWT_SECRET before starting the backend.");
        }

        var keyByteCount = Encoding.UTF8.GetByteCount(signingKey);
        if (keyByteCount < 32)
        {
            throw new InvalidOperationException(
                $"ADMIN_JWT_SECRET/Jwt:SigningKey must be at least 32 bytes (256 bits) for HS256, but it's only {keyByteCount} bytes. " +
                "Use a longer random value, e.g.: $bytes = New-Object byte[] 32; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes); [Convert]::ToBase64String($bytes)");
        }

        return signingKey;
    }
}
