using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Shared.Constants;

namespace AdPhotoManager.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public string GenerateAccessToken(ClaimsPrincipal user)
    {
        var claims = user.Claims.ToList();

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration[ConfigurationKeys.JwtSecretKey]
                ?? throw new InvalidOperationException("JWT secret key not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = int.Parse(
            _configuration[ConfigurationKeys.JwtExpirationMinutes] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _configuration[ConfigurationKeys.JwtIssuer],
            audience: _configuration[ConfigurationKeys.JwtAudience],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration[ConfigurationKeys.JwtSecretKey]
                    ?? throw new InvalidOperationException("JWT secret key not configured")));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration[ConfigurationKeys.JwtIssuer],
                ValidAudience = _configuration[ConfigurationKeys.JwtAudience],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserIdFromClaims(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
    }

    public string? GetUserEmailFromClaims(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
    }

    public string? GetUserOrganizationFromClaims(ClaimsPrincipal principal)
    {
        return principal.FindFirst("organization")?.Value;
    }
}
