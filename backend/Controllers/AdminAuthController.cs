using System.Security.Cryptography;
using System.Text;
using LilyWhiteMap.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LilyWhiteMap.Api.Controllers;

public sealed record AdminLoginRequest(string Password);

public sealed record AdminLoginResponse(string Token, DateTime ExpiresAtUtcHint);

/// <summary>Issues JWTs for the admin analytics dashboard. No ASP.NET Identity/user table required.</summary>
[ApiController]
[Route("api/admin/auth")]
public sealed class AdminAuthController(JwtTokenService tokenService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<AdminLoginResponse> Login([FromBody] AdminLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new { error = "Invalid credentials." });
        }

        // Preferred for production: a pre-computed SHA256 hash, so the real password
        // never has to be stored/typed into an env var.
        var expectedHash = configuration["Admin:PasswordHash"];
        if (string.IsNullOrWhiteSpace(expectedHash))
        {
            expectedHash = Environment.GetEnvironmentVariable("ADMIN_PASSWORD_HASH");
        }

        if (!string.IsNullOrWhiteSpace(expectedHash))
        {
            return VerifyPassword(request.Password, expectedHash)
                ? Ok(new AdminLoginResponse(tokenService.CreateToken(subject: "admin"), DateTime.UtcNow.AddMinutes(60)))
                : Unauthorized(new { error = "Invalid credentials." });
        }

        // Convenience for local development: a plain-text password, e.g. ADMIN_PASSWORD=test.
        var expectedPlainTextPassword = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(expectedPlainTextPassword))
        {
            expectedPlainTextPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        }

        if (!string.IsNullOrWhiteSpace(expectedPlainTextPassword))
        {
            return VerifyPlainText(request.Password, expectedPlainTextPassword)
                ? Ok(new AdminLoginResponse(tokenService.CreateToken(subject: "admin"), DateTime.UtcNow.AddMinutes(60)))
                : Unauthorized(new { error = "Invalid credentials." });
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Admin password is not configured on the server." });
    }

    private static bool VerifyPlainText(string password, string expectedPassword)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedPassword);

        // Pad to equal length before comparing so FixedTimeEquals doesn't short-circuit on length,
        // which would leak the expected password's length via timing.
        if (passwordBytes.Length != expectedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(passwordBytes, expectedBytes);
    }

    private static bool VerifyPassword(string password, string expectedHashHex)
    {
        var computedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash.ToUpperInvariant()),
            Encoding.UTF8.GetBytes(expectedHashHex.ToUpperInvariant()));
    }
}
