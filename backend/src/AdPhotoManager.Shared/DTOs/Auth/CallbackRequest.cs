namespace AdPhotoManager.Shared.DTOs.Auth;

public class CallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
