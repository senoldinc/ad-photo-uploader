namespace AdPhotoManager.Shared.DTOs.Auth;

public class LoginResponse
{
    public string RedirectUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
