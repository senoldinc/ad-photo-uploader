namespace AdPhotoManager.Shared.DTOs;

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }

    public ErrorResponse()
    {
    }

    public ErrorResponse(string error, string message, object? details = null)
    {
        Error = error;
        Message = message;
        Details = details;
    }
}
