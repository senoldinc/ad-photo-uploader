using System.Security.Claims;

namespace AdPhotoManager.Core.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Generates JWT access token
    /// </summary>
    string GenerateAccessToken(ClaimsPrincipal user);

    /// <summary>
    /// Generates refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates JWT token and returns claims principal
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Gets user ID from claims
    /// </summary>
    string? GetUserIdFromClaims(ClaimsPrincipal principal);

    /// <summary>
    /// Gets user email from claims
    /// </summary>
    string? GetUserEmailFromClaims(ClaimsPrincipal principal);

    /// <summary>
    /// Gets user organization from claims
    /// </summary>
    string? GetUserOrganizationFromClaims(ClaimsPrincipal principal);
}
