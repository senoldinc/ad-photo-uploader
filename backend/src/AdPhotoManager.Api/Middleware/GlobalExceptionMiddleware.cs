using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AdPhotoManager.Shared.Constants;
using AdPhotoManager.Shared.DTOs;

namespace AdPhotoManager.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionMiddleware : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception occurred: {Message}", context.Exception.Message);

        var statusCode = context.Exception switch
        {
            UnauthorizedAccessException => 401,
            KeyNotFoundException => 404,
            ArgumentException => 400,
            InvalidOperationException => 400,
            _ => 500
        };

        var errorCode = statusCode switch
        {
            401 => ErrorCodes.UNAUTHORIZED,
            404 => ErrorCodes.USER_NOT_FOUND,
            400 => ErrorCodes.INVALID_SEARCH_QUERY,
            _ => ErrorCodes.INTERNAL_SERVER_ERROR
        };

        context.Result = new ObjectResult(new ErrorResponse(
            errorCode,
            ErrorCodes.GetMessage(errorCode),
            context.Exception.Message
        ))
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
