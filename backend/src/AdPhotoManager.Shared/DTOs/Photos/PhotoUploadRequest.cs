namespace AdPhotoManager.Shared.DTOs.Photos;

/// <summary>
/// Request model for photo upload
/// </summary>
public class PhotoUploadRequest
{
    /// <summary>
    /// JPEG quality (0.3 to 1.0, default 0.95)
    /// </summary>
    public double Quality { get; set; } = 0.95;
}
