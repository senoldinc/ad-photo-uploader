using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Shared.Constants;

namespace AdPhotoManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly Dictionary<string, string> _stateStore = new();

    public AuthService(
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<LoginResult> LoginAsync(string? returnUrl = null)
    {
        // Generate state for CSRF protection
        var state = Guid.NewGuid().ToString("N");

        // Store state temporarily (in production, use distributed cache)
        _stateStore[state] = returnUrl ?? "/";

        // Build OIDC authorization URL
        var tenantId = _configuration[ConfigurationKeys.AzureAdTenantId];
        var clientId = _configuration[ConfigurationKeys.AzureAdClientId];
        var instance = _configuration[ConfigurationKeys.AzureAdInstance];
        var callbackPath = _configuration[ConfigurationKeys.AzureAdCallbackPath];

        var redirectUri = $"https://localhost:5001{callbackPath}";
        var authorizationUrl = $"{instance}{tenantId}/oauth2/v2.0/authorize?" +
            $"client_id={clientId}&" +
            $"response_type=code&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"response_mode=query&" +
            $"scope=openid%20profile%20email&" +
            $"state={state}";

        _logger.LogInformation("OIDC login initiated with state: {State}", state);

        return Task.FromResult(new LoginResult
        {
            RedirectUrl = authorizationUrl,
            State = state
        });
    }

    public Task<TokenResult> CallbackAsync(string code, string state)
    {
        // Validate state
        if (!_stateStore.ContainsKey(state))
        {
            _logger.LogWarning("Invalid state received: {State}", state);
            throw new UnauthorizedAccessException(ErrorCodes.GetMessage(ErrorCodes.INVALID_STATE));
        }

        // Remove state from store
        _stateStore.Remove(state);

        // In a real implementation, exchange code for tokens with Azure AD
        // For now, return a placeholder
        _logger.LogInformation("OIDC callback processed for code: {Code}", code);

        // TODO: Implement actual token exchange with Azure AD
        // This is a simplified version - actual implementation would:
        // 1. Exchange authorization code for access token
        // 2. Validate the token
        // 3. Extract user claims
        // 4. Check organization authorization
        // 5. Generate JWT tokens

        throw new NotImplementedException("Token exchange not yet implemented");
    }

    public Task<TokenResult> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Implement refresh token logic
        // In production, validate refresh token and issue new access token
        _logger.LogInformation("Refresh token requested");

        throw new NotImplementedException("Refresh token not yet implemented");
    }

    public Task LogoutAsync(string token)
    {
        // TODO: Implement token invalidation
        // In production, add token to blacklist or revoke in identity provider
        _logger.LogInformation("Logout requested");

        return Task.CompletedTask;
    }
}
