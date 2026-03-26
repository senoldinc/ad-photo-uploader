using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Shared.Constants;
using AdPhotoManager.Shared.DTOs;
using AdPhotoManager.Shared.DTOs.Auth;

namespace AdPhotoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Initiates OIDC login flow
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request.ReturnUrl);

            return Ok(new LoginResponse
            {
                RedirectUrl = result.RedirectUrl,
                State = result.State
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.AUTH_SERVICE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.AUTH_SERVICE_ERROR),
                ex.Message
            ));
        }
    }

    /// <summary>
    /// Handles OIDC callback and issues JWT tokens
    /// </summary>
    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Callback([FromBody] CallbackRequest request)
    {
        try
        {
            var result = await _authService.CallbackAsync(request.Code, request.State);

            return Ok(new TokenResponse
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresIn = result.ExpiresIn,
                TokenType = result.TokenType,
                User = result.User != null ? new UserResponse
                {
                    Id = result.User.Id,
                    DisplayName = result.User.DisplayName,
                    Email = result.User.Email,
                    Organization = result.User.Organization
                } : null
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized callback attempt");
            return Unauthorized(new ErrorResponse(
                ErrorCodes.UNAUTHORIZED,
                ex.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Callback failed");
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.AUTH_SERVICE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.AUTH_SERVICE_ERROR),
                ex.Message
            ));
        }
    }

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            return Ok(new TokenResponse
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresIn = result.ExpiresIn,
                TokenType = result.TokenType
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Invalid refresh token");
            return Unauthorized(new ErrorResponse(
                ErrorCodes.INVALID_REFRESH_TOKEN,
                ErrorCodes.GetMessage(ErrorCodes.INVALID_REFRESH_TOKEN)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.AUTH_SERVICE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.AUTH_SERVICE_ERROR),
                ex.Message
            ));
        }
    }

    /// <summary>
    /// Logs out user and invalidates tokens
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _authService.LogoutAsync(token);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.AUTH_SERVICE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.AUTH_SERVICE_ERROR),
                ex.Message
            ));
        }
    }
}
