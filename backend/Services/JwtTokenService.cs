using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LilyWhiteMap.Api.Services;

public sealed class JwtOptions
{
    public string SigningKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = "LilyWhiteMap.Api";
    public string Audience { get; init; } = "LilyWhiteMap.Admin";
    public int ExpiryMinutes { get; init; } = 60;
}

/// <summary>Issues short-lived JWTs for the single-admin dashboard login.</summary>
public sealed class JwtTokenService(Microsoft.Extensions.Options.IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public string CreateToken(string subject)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
