using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using AdPhotoManager.Shared.Constants;
using AdPhotoManager.Shared.DTOs;

namespace AdPhotoManager.Api.Middleware;

public class OrganizationAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IConfiguration _configuration;

    public OrganizationAuthorizationFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Skip if endpoint allows anonymous
        if (context.ActionDescriptor.EndpointMetadata
            .Any(m => m.GetType().Name == "AllowAnonymousAttribute"))
        {
            return Task.CompletedTask;
        }

        // Check if user is authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask; // Let [Authorize] attribute handle this
        }

        // Get user's organization from claims
        var userOrganization = context.HttpContext.User
            .FindFirst("organization")?.Value;

        if (string.IsNullOrEmpty(userOrganization))
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponse(
                ErrorCodes.UNAUTHORIZED_ORGANIZATION,
                ErrorCodes.GetMessage(ErrorCodes.UNAUTHORIZED_ORGANIZATION),
                "User organization not found in claims"
            ));
            return Task.CompletedTask;
        }

        // Get allowed organizations from configuration
        var allowedOrganizations = _configuration
            .GetSection(ConfigurationKeys.AdAllowedOrganizations)
            .Get<string[]>() ?? Array.Empty<string>();

        // Check if user's organization is in allowed list
        var isAuthorized = allowedOrganizations.Any(org =>
            userOrganization.Contains(org, StringComparison.OrdinalIgnoreCase));

        if (!isAuthorized)
        {
            context.Result = new ForbidResult();
        }

        return Task.CompletedTask;
    }
}
