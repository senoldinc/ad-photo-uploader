namespace AdPhotoManager.Shared.DTOs.Photos;

/// <summary>
/// Response model for photo upload
/// </summary>
public class PhotoUploadResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int OutputSizeKb { get; set; }
    public double QualityUsed { get; set; }
    public DateTime UploadedAt { get; set; }
}
