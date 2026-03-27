namespace AdPhotoManager.Core.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Initiates OIDC login flow
    /// </summary>
    Task<LoginResult> LoginAsync(string? returnUrl = null);

    /// <summary>
    /// Handles OIDC callback and issues JWT tokens
    /// </summary>
    Task<TokenResult> CallbackAsync(string code, string state);

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    Task<TokenResult> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Logs out user and invalidates tokens
    /// </summary>
    Task LogoutAsync(string token);
}

public class LoginResult
{
    public string RedirectUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public class TokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
}
